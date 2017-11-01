using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
            "2stuff"
        };

        /// <summary>
        /// 以并行方式运行消耗更多时间的原因是？
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
    }
}
