using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Xunit;

namespace Concurrency
{
    public class ParallelCollectionTest : BaseTest
    {
        #region producer consumer queue

        private readonly ConcurrentQueue<int> _flourDepot = new ConcurrentQueue<int>();

        private readonly ConcurrentQueue<int> _bakery = new ConcurrentQueue<int>();

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// producer consumer queue test.
        /// </summary>
        [Theory]
        [InlineData(true)]
        public ParallelLoopResult FirstProducerQueueTest(bool needWait = true)
        {
            int producerCount = 5;
            int times = 1000 + 1;

            var producerOption = new ParallelOptions() { MaxDegreeOfParallelism = producerCount };

            //produce flour
            var result = Parallel.ForEach(Partitioner.Create(1, times), producerOption, tuple =>
            {
                int sum = 0;
                for (int i = tuple.Item1; i < tuple.Item2; i++)
                {
                    var count = i % 10 + 1;
                    sum += count;
                    _flourDepot.Enqueue(count);
                }

                Trace.WriteLine($"range: {tuple.Item1} to {tuple.Item2}, produce {sum} flour.");
            });

            //check total 55 * 100
            if (needWait)
            {
                while (!result.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                Trace.WriteLine($"total: {_flourDepot.Sum()}");
            }

            return result;
        }

        [Theory]
        [InlineData(true)]
        public Task<int>[] FirstConsumerQueueTest(bool needWait = true)
        {
            var result = FirstProducerQueueTest(false);

            int consumerCount = 3;
            var consumers = new Task<int>[consumerCount];
            for (int i = 0; i < consumers.Length; i++)
            {
                var index = i;
                consumers[i] = Task.Factory.StartNew(() =>
                {
                    int current = 0;
                    while (!result.IsCompleted || result.IsCompleted && !_flourDepot.IsEmpty)
                    {
                        int flour;
                        if (_flourDepot.TryDequeue(out flour))
                        {
                            current += flour;

                            //produce 1 bread cost 10 flour
                            if (current >= 10)
                            {
                                _bakery.Enqueue(current / 10);
                                current %= 10;
                            }
                        }
                    }

                    Trace.WriteLine($"{index} task has {current} flour.");
                    return current;
                });
            }

            if (needWait)
            {
                try
                {
                    Task.WaitAll(consumers);
                }
                catch (AggregateException ex)
                {
                    throw;
                }

                Trace.WriteLine($"has {_bakery.Sum()} breads.");
            }

            return consumers;
        }

        #endregion

        /// <summary>
        /// Test Range push/pop operation is exclusive（独占的）
        /// </summary>
        [Fact]
        public void ConcurrentStackRangeOperationTest()
        {
            var stack = new ConcurrentStack<int>();

            var t1 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);

                var array = new int[10];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i * 2;
                }

                stack.PushRange(array);
            });

            var t2 = Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);

                var array = new int[10];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = i * 2 + 1;
                }

                stack.PushRange(array);
            });

            try
            {
                Task.WaitAll(t1, t2);
            }
            catch (AggregateException ex)
            {
                throw;
            }

            var outArray = stack.ToArray();
            Trace.WriteLine($"stack items: {string.Join(", ", outArray)}");

            var a1 = new int[10];
            var a2 = new int[20];

            var popT1 = Task.Factory.StartNew(() =>
                {
                    return stack.TryPopRange(a1);
                });

            var popT2 = Task.Factory.StartNew(() =>
                {
                    return stack.TryPopRange(a2, 0, a2.Length);
                });

            try
            {
                Task.WaitAll(popT1, popT2);
            }
            catch (AggregateException ex)
            {
                throw;
            }

            Trace.WriteLine($"pop range result:");
            Trace.WriteLine($"a1: {string.Join(", ", a1)}");
            Trace.WriteLine($"a2: {string.Join(", ", a2)}");
        }

        /// <summary>
        /// range push and pop is atomicity test.
        /// </summary>
        [Fact]
        public void ConcurrentStackRangePushAndPopIsAtomicityTest()
        {
            var array = new int[] { 1, 2, 3, 4, 5, 6 };
            var stack = new ConcurrentStack<int>();

            var pushTask = Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    var value = i;
                    var input = array.Select(o => value * 10 + o).ToArray();
                    stack.PushRange(input);

                    Thread.Sleep(50);
                }
            });

            var popTask = Task.Factory.StartNew(() =>
            {
                while (!pushTask.IsCompleted || !stack.IsEmpty)
                {
                    var popArray = new int[6];
                    stack.TryPopRange(popArray);
                    Trace.WriteLine(string.Join(", ", popArray));

                    Thread.Sleep(100);
                }
            });

            try
            {
                Task.WaitAll(pushTask, popTask);
            }
            catch (AggregateException ex)
            {
                throw;
            }
        }

        public void ConcurrentBagTest()
        {
            var queue = new ConcurrentQueue<int>();
            var stack = new ConcurrentStack<int>();
            var bag = new ConcurrentBag<int>();
            var dic = new ConcurrentDictionary<int, int>();
        }

        public void BlockingCollectionTest()
        {
            var block = new BlockingCollection<int>();
        }
    }
}
