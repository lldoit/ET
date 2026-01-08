using System;
using System.Collections.Generic;

namespace ET
{
    public static class ETTaskHelper
    {
        public static async ETTask<T> GetContextAsync<T>() where T: class
        {
            ETTask<object> tcs = ETTask<object>.Create(true);
            tcs.TaskType = TaskType.ContextTask;
            object ret = await tcs;
            if (ret == null)
            {
                return null;
            }
            return (T)ret;
        }
        
        public static bool IsCancel(this ETCancellationToken self)
        {
            if (self == null)
            {
                return false;
            }
            return self.IsDispose();
        }
        
        private class CoroutineBlocker
        {
            private int count;

            private ETTask tcs;

            public CoroutineBlocker(int count)
            {
                this.count = count;
            }
            
            public async ETTask RunSubCoroutineAsync(ETTask task)
            {
                try
                {
                    await task;
                }
                finally
                {
                    --this.count;
                
                    if (this.count <= 0 && this.tcs != null)
                    {
                        ETTask t = this.tcs;
                        this.tcs = null;
                        t.SetResult();
                    }
                }
            }

            public async ETTask WaitAsync()
            {
                if (this.count <= 0)
                {
                    return;
                }
                this.tcs = ETTask.Create(true);
                await tcs;
            }
        }

        public static async ETTask WaitAny(List<ETTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAny(ETTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(1);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAll(ETTask[] tasks)
        {
            if (tasks.Length == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Length);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        public static async ETTask WaitAll(List<ETTask> tasks)
        {
            if (tasks.Count == 0)
            {
                return;
            }

            CoroutineBlocker coroutineBlocker = new CoroutineBlocker(tasks.Count);

            foreach (ETTask task in tasks)
            {
                coroutineBlocker.RunSubCoroutineAsync(task).NoContext();
            }

            await coroutineBlocker.WaitAsync();
        }

        /// <summary>
        /// 带超时的任务执行
        /// </summary>
        /// <param name="task">要执行的任务</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <returns>任务是否在超时前完成，true=完成，false=超时</returns>
        public static async ETTask<bool> WithTimeout(this ETTask task, long timeoutMs)
        {
            if (timeoutMs <= 0)
            {
                await task;
                return true;
            }

            bool completed = false;
            bool timeout = false;

            async ETTask RunTask()
            {
                await task;
                completed = true;
            }

            async ETTask RunTimeout()
            {
                await TimerComponent.Instance.WaitAsync(timeoutMs);
                timeout = true;
            }

            await WaitAny(new ETTask[] { RunTask(), RunTimeout() });

            return completed && !timeout;
        }

        /// <summary>
        /// 带超时的任务执行（带返回值）
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="task">要执行的任务</param>
        /// <param name="timeoutMs">超时时间(毫秒)</param>
        /// <param name="defaultValue">超时时的默认返回值</param>
        /// <returns>任务执行结果，超时返回defaultValue</returns>
        public static async ETTask<(bool completed, T result)> WithTimeout<T>(this ETTask<T> task, long timeoutMs, T defaultValue = default)
        {
            if (timeoutMs <= 0)
            {
                T result = await task;
                return (true, result);
            }

            bool completed = false;
            bool timeout = false;
            T taskResult = defaultValue;

            async ETTask RunTask()
            {
                taskResult = await task;
                completed = true;
            }

            async ETTask RunTimeout()
            {
                await TimerComponent.Instance.WaitAsync(timeoutMs);
                timeout = true;
            }

            await WaitAny(new ETTask[] { RunTask(), RunTimeout() });

            if (completed && !timeout)
            {
                return (true, taskResult);
            }
            
            return (false, defaultValue);
        }
    }
}