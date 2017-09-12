using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace Concurrency
{
    public class TaskTest : BaseTest
    {
        [Fact]
        public void UseParallelStracksWindowTest()
        {
            var t1 = new Task(() => Trace.WriteLine("t1"));
            var t2 = new Task(() => Trace.WriteLine("t2"));

            t1.Start();
            t2.Start();

            Task.WaitAll(t1, t2);
        }

        /// <summary>
        /// Use CancellationToken to cancel task.
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task CancellationTokenTest()
        {
            var cts = new CancellationTokenSource();
            var cancellToken = cts.Token;

            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                cancellToken.ThrowIfCancellationRequested();
                Trace.WriteLine("end");
            }, cancellToken);

            cts.Cancel();

            try
            {
                //通过 await 语法执行Task，这里直接抛出TaskCanceledException异常
                //await task;

                task.Wait();
                //Task.WaitAll(new[] { task });
            }
            catch (AggregateException exception)
            {
                foreach (var inner in exception.InnerExceptions)
                {
                    if (inner is TaskCanceledException)
                    {
                        Trace.WriteLine("CancellationToken is cancelled.");
                    }
                    else
                    {
                        Trace.WriteLine($"other exception: {inner.Message}");
                    }
                }
            }
            catch (TaskCanceledException exception)
            {
                Trace.WriteLine("exception by TaskAwaiter.ThrowForNonSuccess method.");
            }
        }

        [Fact]
        public void ExecTaskExceptionTest()
        {
            var cts = new CancellationTokenSource();
            var cancellToken = cts.Token;

            var task = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);

                cancellToken.ThrowIfCancellationRequested();

                throw new ApplicationException("throw this exception for stop task.");

                Trace.WriteLine("task completed.");
            }, cancellToken);

            //以TaskCancel的方式停止任务
            cts.Cancel();

            try
            {
                task.Wait();
            }
            catch
            {
                if (task.IsFaulted)
                {
                    if (task.Exception != null)
                        foreach (var innerException in task.Exception.InnerExceptions)
                        {
                            Trace.WriteLine(innerException.Message);
                        }
                }
                else if (task.IsCanceled)
                {
                    //task.Exception ...
                    Trace.WriteLine("task was canceled.");
                }
            }
        }

        [Fact]
        public void ContinueWithTest()
        {
            var t1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                Trace.WriteLine("t1 complete.");

                return true;
            });

            var t2 = t1.ContinueWith(task =>
            {
                Trace.WriteLine("t2 run.");
                if (task.Result)
                {

                }
            });

            var t3 = t1.ContinueWith(task =>
            {
                Trace.WriteLine("t3 run.");
                return task.Result;
            });

            var t4 = t1.ContinueWith(task => Trace.WriteLine("t4. run."));

            Task.WaitAll(t2, t3, t4);

            Trace.WriteLine("the serial something.");

            //do other parallel tasks
        }

        /// <summary>
        /// Use TaskContinuationOptions to contrl task behaviour
        /// </summary>
        [Fact]
        public void TaskContinuationOptionsTest()
        {
            var tokenSource = new CancellationTokenSource(200);
            var token = tokenSource.Token;

            var t1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                if (token.IsCancellationRequested)
                {
                    Trace.WriteLine("t1 was canceled.");
                    token.ThrowIfCancellationRequested();
                }
                else
                {
                    Trace.WriteLine("t1 complete.");
                }
            }, token);

            var t2 = t1.ContinueWith(task => Trace.WriteLine("t2 continue with t1 when t1 is completed."), 
                TaskContinuationOptions.NotOnCanceled);

            try
            {
                t2.Wait();
            }
            catch (AggregateException ex)
            {
                
            }
        }

        /// <summary>
        /// 取消状态只能传递给直接延续任务
        /// </summary>
        [Fact]
        public void MultiContinuationTaskTest()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var t1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(500);
                if (token.IsCancellationRequested)
                {
                    Trace.WriteLine("t1 was canceled.");
                    token.ThrowIfCancellationRequested();
                }
                else
                {
                    Trace.WriteLine("t1 complete.");
                }
            }, token);

            var t2 = t1.ContinueWith(task => Trace.WriteLine("t2 was completed."),
                TaskContinuationOptions.None);

            var t3 = t1.ContinueWith(task => Trace.WriteLine("t3 continue with t1 when t1 is completed."),
                TaskContinuationOptions.NotOnCanceled);

            var t4 = t2.ContinueWith(task => Trace.WriteLine("t4 continue with t2 when t2 is completed."),
                TaskContinuationOptions.NotOnCanceled);

            tokenSource.Cancel();

            try
            {
                Task.WaitAll(t2, t3, t4);
            }
            catch (AggregateException ex)
            {
                foreach (var innerException in ex.InnerExceptions)
                {
                    Trace.WriteLine(innerException.Message);
                }
            }

        }
    }
}
