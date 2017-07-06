using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Concurrency
{
    public class ParallelForeachTest
    {
        public void Test()
        {
            while (true)
            {
                ParallelFor();

                Console.ReadLine();
                Trace.WriteLine("next time:");
            }
        }

        public static void ParallelFor()
        {
            int i = 100;
            var len = 100000000;

            var list = new int[len];

            var list2 = new List<int>();
            for (int j = 0; j < len; j++)
            {
                list2.Add(j);
            }

            var list3 = new List<A>();
            for (int j = 0; j < len; j++)
            {
                list3.Add(new A());
            }

            var timmer = new Stopwatch();
            var allElas = new List<long>();

            while (i-- > 0)
            {
                timmer.Restart();
                Parallel.ForEach(list, (item, state) => Do());
                timmer.Stop();
                Trace.WriteLine("parallel: " + timmer.Elapsed);
                allElas.Add(timmer.ElapsedMilliseconds);

                //timmer.Restart();
                //foreach (var item in list)
                //{
                //    Do();
                //}
                //timmer.Stop();
                //Trace.WriteLine("sync: " + timmer.Elapsed);
                //allElas.Add(timmer.ElapsedMilliseconds);
            }

            //analyze
            var result = allElas.GroupBy(o => o).OrderBy(o => o.Key).Select(o => new { o.Key, Count = o.Count() }).ToList();
            var min = result.Min(o => o.Key);
            var max = result.Max(o => o.Key);

            //by 5 ms
            int space = 5;
            var count = (max - min) / space;
            count = count + (max >= min + count * space ? 1 : 0);
            var distribution = new int[count];
            int index = 0;
            var leftValue = min;
            foreach (var item in result)
            {
                reduce:
                if (item.Key >= leftValue && item.Key < leftValue + space)
                {
                    distribution[index] = distribution[index] + item.Count;
                    continue;
                }
                else
                {
                    leftValue = min + ++index * space;
                    goto reduce;
                }
            }

            for (int j = 0; j < distribution.Length; j++)
            {
                Trace.WriteLine(min + j * space + "\t" + distribution[j] + "\t" + $"{distribution[j] / (float)allElas.Count:F2}");
            }
        }

        public static void Do()
        {

        }
    }

    class A
    {
        public int Type { get; set; }
    }
}
