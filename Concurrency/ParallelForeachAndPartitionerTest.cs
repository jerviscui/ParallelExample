using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Concurrency
{
    public class ParallelForeachAndPartitionerTest : BaseTest
    {
        /// <summary>
        /// 使用分区器切分数据
        /// </summary>
        [Fact]
        public void ForEachUsePartitioner()
        {
            Parallel.ForEach(Partitioner.Create(1, 50), range =>
            {
                Trace.WriteLine($"range [{range.Item1}, {range.Item2})");
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Trace.WriteLine(i);
                }
            });
        }

        /// <summary>
        /// 根据处理器内核数优化分区方案
        /// </summary>
        [Fact]
        public void OptimizeByProcessor()
        {
            Parallel.ForEach(Partitioner.Create(1, 500, 500 / Environment.ProcessorCount + 1), range =>
            {
                Trace.WriteLine($"range [{range.Item1}, {range.Item2})");
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Trace.WriteLine(i);
                }
            });
        }

        /// <summary>
        /// 通过LoopState停止执行计划
        /// </summary>
        [Fact]
        public void ParallelLoopState()
        {
            var list = Enumerable.Range(1, 50);
            var loopResult = Parallel.ForEach(list, (i, state) =>
            {
                if (state.ShouldExitCurrentIteration)
                {
                    return;
                }

                Trace.WriteLine(i);
                if (i == 25)
                {
                    state.Break();
                }
            });

            Trace.WriteLine("last task index: " + loopResult.LowestBreakIteration);
        }

        /// <summary>
        /// Parallel.ForEach异常处理
        /// </summary>
        [Fact]
        public void ExceptionHandle()
        {
            var list = Enumerable.Range(1, 50);
            try
            {
                var loopResult = Parallel.ForEach(list, (i, state) =>
                    {
                        Trace.WriteLine(i);
                        if (i % 2 == 0)
                        {
                            throw new Exception(i.ToString());
                        }
                    });
            }
            catch (AggregateException exception)
            {
                foreach (var innerException in exception.InnerExceptions)
                {
                    Trace.WriteLine("ex.Message: " + innerException.Message);
                }
            }
        }

        [Fact]
        public void UseParallelOptions()
        {
            var options = new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };
            //use all processers
            //options.MaxDegreeOfParallelism = -1;
            //Parallel.ForEach(Partitioner.Create(1, 500 * 1000, 500 * 1000 / Environment.ProcessorCount + 1), options, range =>
            Parallel.ForEach(Partitioner.Create(1, 500 * 1000, 500 * 1000 / (Environment.ProcessorCount - 1) + 1), options, range =>
            {
                Trace.WriteLine($"range [{range.Item1}, {range.Item2})");
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    Trace.WriteLine(i);
                }
            });
        }
    }
}
