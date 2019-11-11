using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExecutionPipeline
{
    internal static class ExecutionPipeline
    {
        public static ExecutionPipeline<T> Wrap<T>(Func<T> action)
        {
            return new ExecutionPipeline<T>(action);
        }
    }

    internal class ExecutionPipeline<T>
    {
        public Func<T> Action { get; set; }

        public Func<T, ActionResult> OnSuccessAction { get; set; }

        public List<PipelineException> ErrorActions { get; set; } = new List<PipelineException>();

        public ExecutionPipeline(Func<T> action)
        {
            Action = action;
        }

        public ActionResult Run()
        {
            ActionResult result = null;

            try
            {
                result = OnSuccessAction(Action());
            }
            catch (Exception ex)
            {
                var handler = ErrorActions.FirstOrDefault(s => s.ExceptionType == ex.GetType());
                if (handler != null)
                {
                    return handler.Handler(ex);
                }

                throw;
            }

            return result;
        }

        /// <summary>
        /// Adds an exception handler action to the list
        /// </summary>
        public ExecutionPipeline<T> OnError<TException>(Func<Exception, ActionResult> action)
        {
            ErrorActions.Add(new PipelineException() { ExceptionType = typeof(TException), Handler = action });
            return this;
        }

        /// <summary>
        /// Initializes the OnSuccess method
        /// </summary>
        public ExecutionPipeline<T> OnSuccess(Func<T, ActionResult> action)
        {
            OnSuccessAction = action;
            return this;
        }
    }

    internal class PipelineException
    {
        public Type ExceptionType { get; set; }
        public Func<Exception, ActionResult> Handler { get; set; }
    }
}
