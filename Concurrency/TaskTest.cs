using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
    }
}
