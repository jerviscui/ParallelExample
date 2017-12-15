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
    }
}
