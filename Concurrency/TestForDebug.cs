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

            var task = new Task(() =>
            {
                Trace.WriteLine("start");

                for (int i = 0; i < 10; i++)
                {
                    Thread.Sleep(100);
                }
            });

            task.Start();

            try
            {
                task.Wait();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
