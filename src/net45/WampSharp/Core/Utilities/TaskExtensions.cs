﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using WampSharp.Core.Utilities;

namespace WampSharp.Core.Utilities
{
    internal static class TaskExtensions
    {
        private static readonly MethodInfo mCastTaskToGenericTask = GetCastTaskToGenericTaskMethod();
        private static readonly MethodInfo mCastToNonGenericTask = GetCastGenericTaskToNonGenericMethod();

        private static MethodInfo GetCastGenericTaskToNonGenericMethod()
        {
            return typeof(TaskExtensions).GetMethod("InnerCastTask",
                                                     BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static MethodInfo GetCastTaskToGenericTaskMethod()
        {
            return typeof(TaskExtensions).GetMethod("InternalCastTask",
                                                     BindingFlags.Static | BindingFlags.NonPublic);
        }

        public static Task Cast(this Task<object> task, Type taskType)
        {
            return (Task) mCastTaskToGenericTask.MakeGenericMethod(taskType)
                                                .Invoke(null, new object[] {task});
        }

        private static Task<T> InternalCastTask<T>(Task<object> task)
        {
            return task.ContinueWithSafe(x => (T)x.Result);
        }

        /// <summary>
        /// Unwraps the return type of a given method.
        /// </summary>
        /// <param name="returnType">The given return type.</param>
        /// <returns>The unwrapped return type.</returns>
        /// <example>
        /// void, Task -> object
        /// Task{string} -> string
        /// int -> int
        /// </example>
        public static Type UnwrapReturnType(Type returnType)
        {
            if (returnType == typeof(void) || returnType == typeof(Task))
            {
                return typeof(object);
            }

            Type taskType =
                returnType.GetClosedGenericTypeImplementation(typeof(Task<>));

            if (taskType != null)
            {
                return returnType.GetGenericArguments()[0];
            }

            return returnType;
        }

        public static bool HasReturnValue(this MethodInfo method)
        {
            Type returnType = method.ReturnType;

            if (returnType == typeof(void) || returnType == typeof(Task))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Casts a <see cref="Task"/> to a Task of type Task{object}.
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static Task<object> CastTask(this Task task)
        {
            Task<object> result;

            if (task.GetType() == typeof (Task))
            {
                result = task.ContinueWithSafe(x => (object) null);
            }
            else
            {
                Type underlyingType = UnwrapReturnType(task.GetType());

                MethodInfo method =
                    mCastToNonGenericTask.MakeGenericMethod(underlyingType);

                result = (Task<object>) method.Invoke(null, new object[] {task});
            }

            return result;
        }

        private static Task<object> InnerCastTask<T>(Task<T> task)
        {
            return task.ContinueWithSafe(t => (object)t.Result);
        }

        private static Task<TResult> ContinueWithSafe<TTask, TResult>(this TTask task, Func<TTask, TResult> transform)
            where TTask : Task
        {
            return task.ContinueWith(t => ContinueWithSafeCallback((TTask) t, transform),
                                     TaskContinuationOptions.ExecuteSynchronously);
        }

        private static TResult ContinueWithSafeCallback<TTask, TResult>(TTask task, Func<TTask, TResult> transform)
            where TTask : Task
        {
            AggregateException aggregateException = task.Exception;

            if (aggregateException != null)
            {
                throw aggregateException.InnerException;
            }

            TResult result = transform(task);

            return result;
        }
    }
}