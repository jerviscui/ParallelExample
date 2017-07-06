using System;
using System.Threading.Tasks;

namespace Concurrency
{
    class Program2
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();

            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            
        }
    }
}
