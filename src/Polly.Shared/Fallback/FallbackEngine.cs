﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Polly.Fallback
{
    internal static partial class FallbackEngine
    {
        internal static TResult Implementation<TResult>(
            Func<CancellationToken, TResult> action,
            Context context,
            CancellationToken cancellationToken,
            IEnumerable<ExceptionPredicate> shouldHandleExceptionPredicates,
            IEnumerable<ResultPredicate<TResult>> shouldHandleResultPredicates,
            Action<DelegateResult<TResult>, Context> onFallback,
            Func<CancellationToken, TResult> fallbackAction)
        {
            DelegateResult<TResult> delegateOutcome;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                delegateOutcome = new DelegateResult<TResult>(action(cancellationToken));

                if (!shouldHandleResultPredicates.Any(predicate => predicate(delegateOutcome.Result)))
                {
                    return delegateOutcome.Result;
                }
            }
            catch (Exception ex)
            {
                if (!shouldHandleExceptionPredicates.Any(predicate => predicate(ex)))
                {
                    throw;
                }

                delegateOutcome = new DelegateResult<TResult>(ex);
            }

            onFallback(delegateOutcome, context);

            return fallbackAction(cancellationToken);
        }
    }
}
