using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Concurrency
{
    public class TestForDebug : BaseTest
    {
        public void Test()
        {
            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var tasks = new Task[8];
            for (int i = 0; i < 8; i++)
            {
                var i1 = i;
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    Trace.WriteLine($"{i1}: {Thread.CurrentThread.ManagedThreadId} start");

                    Thread.Sleep(100);
                }, token);
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException ex)
            {
                foreach (var inner in ex.InnerExceptions)
                {
                    Trace.WriteLine(inner.Message);
                }
            }
        }

        private static object _lockOjb1 = new object();
        private static object _lockOjb2 = new object();

        public void DeadlockTest()
        {
            int count1 = 0;
            int count2 = 0;

            var task1 = Task.Factory.StartNew(() =>
            {
                lock (_lockOjb1)
                {
                    count1++;
                    Thread.Sleep(5000);

                    lock (_lockOjb2)
                    {
                        count2++;
                    }
                }
            });

            var task2 = Task.Factory.StartNew(() =>
            {
                lock (_lockOjb2)
                {
                    count2++;
                    Thread.Sleep(5000);

                    lock (_lockOjb1)
                    {
                        count1++;
                    }
                }
            });

            try
            {
                Task.WaitAll(task1, task2);
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
