using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Concurrency
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BufferHeight = 10000;
            //Trace.Listeners.Clear();
            Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
            
            //MainAsync(args).Wait();

            using (Disable())
            {
                
            }

            Console.WriteLine(a);
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            using (var test = new ParallelForeachAndPartitionerTest())
            {
                test.UseParallelOptions();
            }
        }

        private static int a;

        static IDisposable Disable()
        {
            a = 0;
            return new DisposeAction(() => Enable());
        }

        static IDisposable Enable()
        {
            a = 1;
            return new DisposeAction(() => Disable());
        }
    }
}
