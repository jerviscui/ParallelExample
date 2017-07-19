using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Concurrency
{
    public class AwaitTest : BaseTest
    {
        /// <summary>
        /// await 和 没有await时的执行顺序
        /// </summary>
        /// <returns>Task.</returns>
        [Fact]
        public async Task MainAsync()
        {
            //期望出现执行结果： run1 run20 run21 
            //或者：run1 run21 run20 
            await Run1();
            _task.Wait();
        }

        private Task _task;

        private async Task Run1()
        {
            _task = Run2();

            Trace.WriteLine("run1");

            await Run2();
        }

        private static int _num;

        private async Task Run2()
        {
            var i = _num++;
            await Task.Delay(1000);
            Trace.WriteLine("run2" + i);
        }
    }
}
