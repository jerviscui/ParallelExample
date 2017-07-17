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
            
            MainAsync(args).Wait();

            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            using (var test = new ParallelForeachAndPartitionerTest())
            {
                test.UseParallelOptions();
            }
        }
    }
}
