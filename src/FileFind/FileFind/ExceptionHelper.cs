using System;
using System.Threading.Tasks;

namespace FileFind
{
    public static class ExceptionHelper
    {
        internal static void WaitForTaskAndTranslateAggregateExceptions(Task taskToWaitFor)
        {
            TranslateAggregateExceptions(taskToWaitFor.Wait);
        }

        internal static void TranslateAggregateExceptions(Action action)
        {
            try
            {
                action();
            }
            catch (AggregateException agg)
            {
                if (agg.InnerExceptions.Count == 1)
                    throw agg.InnerExceptions[0];

                throw;
            }
        }
    }
}