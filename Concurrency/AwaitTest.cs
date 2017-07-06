using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Concurrency
{
    public static class AwaitTest
    {
        static async Task MainAsync()
        {
            await Run1();
        }

        public static async Task Run1()
        {
            var task = Run2();

            Trace.WriteLine("run1");

            await Run2();
        }

        public static int Num;

        public static Task Run2()
        {
            var i = Num++;
            return Task.Run(() => Trace.WriteLine("run2" + i));
        }
    }
}
