using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Concurrency
{
    public class ParallelLinqTest : BaseTest
    {
        public void Test()
        {
            //System.Linq.ParallelEnumerable

        }

        private static readonly List<string> Words = new List<string>
        {
            "aaaaaa", "bbbbb", "ccccccc", "abcd", "audit", "bubble", "declare", "a wide variety of", "variety", "announcing",
            "community", "collapse","refer","alias","reduce","likewise","related","retrieved","adjust","case insensitive",
            "insensitive","term","visible","latex","orthogonal","specified","manual","obtain","angular","cotained","retrieval",
            "slice","remote","technique","fetch","obtained","interator","plain","separate","unary","quote","manipulation","atomicity",
            "consistecy","isolation","durability","interactive","stuff","1announcing","1community","1collapse","1refer","1alias",
            "1reduce","1likewise","1related","1retrieved","1adjust","1case insensitive","1insensitive","1term","1visible","1latex",
            "1orthogonal","1specified","1manual","1obtain","1angular","1cotained","1retrieval","1slice","1remote","1technique",
            "1fetch","1obtained","1interator","1plain","1separate","1unary","1quote","1manipulation","1atomicity","1consistecy",
            "1isolation","1durability","1interactive","1stuff","2announcing","2community","2collapse","2refer","2alias","2reduce",
            "2likewise",
            "2related",
            "2retrieved",
            "2adjust",
            "2case insensitive",
            "2insensitive",
            "2term",
            "2visible",
            "2latex",
            "2orthogonal",
            "2specified",
            "2manual",
            "2obtain",
            "2angular",
            "2cotained",
            "2retrieval",
            "2slice",
            "2remote",
            "2technique",
            "2fetch",
            "2obtained",
            "2interator",
            "2plain",
            "2separate",
            "2unary",
            "2quote",
            "2manipulation",
            "2atomicity",
            "2consistecy",
            "2isolation",
            "2durability",
            "2interactive",
            "2stuff",
            "1reduce","1likewise","1related","1retrieved","1adjust","1case insensitive","1insensitive","1term","1visible","1latex",
            "1orthogonal","1specified","1manual","1obtain","1angular","1cotained","1retrieval","1slice","1remote","1technique"
        };

        /// <summary>
        /// 以并行方式运行消耗更多时间的原因是？线程调度的开销，但是提高了系统的利用率
        /// </summary>
        [Fact]
        public void AsParallelTest()
        {
            var watch = Stopwatch.StartNew();
            int i = 1000;

            var query1 = from word in Words
                         where word.Contains("a") && (word.Contains("b") || word.Contains("d")) && !word.Contains("1")
                         select word;
            var list1 = new List<string>();
            while (i-- > 0)
            {
                list1 = query1.ToList();
            }

            Trace.WriteLine($"linq: {watch.Elapsed}");

            watch.Restart();
            var query2 = from word in Words.AsParallel()//.AsSequential()
                         where word.Contains("a") && (word.Contains("b") || word.Contains("d")) && !word.Contains("1")
                         select word;

            i = 1000;
            var list2 = new List<string>();
            while (i-- > 0)
            {
                list2 = query2.ToList();
            }

            Trace.WriteLine($"plinq: {watch.Elapsed}");
        }

        /// <summary>
        /// AsOrdered 保证数据源的顺序
        /// </summary>
        [Fact]
        public void AsOrderedTest()
        {
            //var words = Words.Take(10).ToList();
            var words = Words;

            var watch = Stopwatch.StartNew();
            var query1 = from word in words.AsParallel()
                         where word.Contains("a") && (word.Contains("b") || word.Contains("d")) && !word.Contains("1")
                         select word;
            var list1 = query1.ToList();
            Trace.WriteLine($"plinq: {watch.Elapsed}");
            //foreach (var item in list1)
            //{
            //    Trace.WriteLine(item);
            //}

            watch.Restart();
            var query2 = from word in words.AsParallel().AsOrdered()
                         where word.Contains("a") && (word.Contains("b") || word.Contains("d")) && !word.Contains("1")
                         select word;

            var list2 = query2.ToList();
            Trace.WriteLine($"AsOrdered : {watch.Elapsed}");
            //foreach (var item in list2)
            //{
            //    Trace.WriteLine(item);
            //}
            for (int i = 0; i < list2.Count; i++)
            {
                if (!list1[i].Equals(list2[i]))
                {
                    Trace.WriteLine($"index:{i} str:{list1[i]} {list2[i]}");
                }
            }
        }

        /// <summary>
        /// ParallelExecutionMode.ForceParallelism 强制并行方式执行
        /// </summary>
        [Fact]
        public void ExecutionModeTest()
        {
            var watch = Stopwatch.StartNew();
            int i = 1;

            var query1 = from word in Words.AsParallel()
                         where word.Contains("a") && (word.Contains("b") || word.Contains("d")) && !word.Contains("1")
                         select word;
            var list1 = new List<string>();
            while (i-- > 0)
            {
                list1 = query1.ToList();
            }

            Trace.WriteLine($"plinq: {watch.Elapsed}");

            watch.Restart();
            var query2 = from word in Words.AsParallel().WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                         where word.Contains("a") && (word.Contains("b") || word.Contains("d")) && !word.Contains("1")
                         select word;

            i = 1;
            var list2 = new List<string>();
            while (i-- > 0)
            {
                list2 = query2.ToList();
            }

            Trace.WriteLine($"plinq: {watch.Elapsed}");
        }

        [Fact]
        public void ParallelMergeTest()
        {
            var watch = Stopwatch.StartNew();
            int count = 1000000000;
            var paraList = ParallelEnumerable.Range(1, count);

            //var query1 = from word in paraList.AsParallel()
            //                 //.WithMergeOptions(ParallelMergeOptions.NotBuffered)
            //             where (word * 2 + 1) % 5 != 0 && word % 2 == 0
            //             select word;

            var query2 = from word in Words.AsParallel()
                         .WithMergeOptions(ParallelMergeOptions.NotBuffered)
                         where word.Contains("a") && (word.Contains("b") && word.Contains("d")) && !word.Contains("1")
                         select word;


            Trace.WriteLine($"{query2.ElementAt(0)}");
            Trace.WriteLine($"plinq: {watch.Elapsed}");
            var list = query2.ToList();
            Trace.WriteLine($"{watch.Elapsed}");
        }

        /// <summary>
        /// 以映射规约架构执行
        /// </summary>
        [Fact]
        public void MapReduceTest()
        {
            var map = Words.AsParallel().ToLookup(s => s, o => 1);

            var query = from item in map.AsParallel()
                        where item.Count() > 1
                        select item;

            foreach (var item in query)
            {
                Trace.WriteLine($"{item.Key}, count:{item.Count()}");
            }
        }

        /// <summary>
        /// 归约操作并行操作需要使用并行数据源
        /// </summary>
        [Fact]
        public void ReducedOperationTest()
        {
            int count = 500000000;

            var list = Enumerable.Range(1, count);
            var average = (from i in list
                           where i % 5 == 0
                           select i / Math.PI).Sum();

            Trace.WriteLine(average);

            var paraList = ParallelEnumerable.Range(1, count);
            var paraAverage = (from i in paraList.AsParallel()
                               where i % 5 == 0
                               select i / Math.PI).Sum();

            Trace.WriteLine(paraAverage);

            var average3 = (from i in list.AsParallel()
                            where i % 5 == 0
                            select i / Math.PI).Sum();

            Trace.WriteLine(average3);
        }

        [Fact]
        public void CancellationTest()
        {
            int count = 500000000;

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var paraList = ParallelEnumerable.Range(1, count);
            var paraAverage = (from i in paraList.AsParallel().WithCancellation(token)
                               where i % 5 == 0
                               select i / Math.PI).Sum();

            Trace.WriteLine(paraAverage);
        }

        [Fact]
        public void ForAllTest()
        {
            int count = 500000000;

            var bag = new ConcurrentBag<double>();
            var list = new List<double>();

            var paraList = ParallelEnumerable.Range(1, count);
            var parallQuery = (from i in paraList.AsParallel()
                               where i % 5 == 0
                               select i / Math.PI);

            var parallQuery2 = (from i in paraList.AsParallel()
                                where i % 5 == 0
                                select i / Math.PI);

            var watch = Stopwatch.StartNew();

            parallQuery2.ForAll(d => bag.Add(d));
            Trace.WriteLine(watch.Elapsed);

            watch.Restart();
            var toList = parallQuery.ToList();
            //foreach (var d in parallQuery)
            //{
            //    list.Add(d);
            //}
            Trace.WriteLine(watch.Elapsed);

            Trace.WriteLine(bag.Count());
        }
    }
}
