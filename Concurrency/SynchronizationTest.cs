using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Concurrency
{
    public class SynchronizationTest : BaseTest
    {
        //Barrier
        //CountdownEvent
        //ManualResetEventSlim
        //SemaphoreSlim
        //SpinLock
        //SpinWait

        [Fact]
        public void BarrierTest()
        {
            var count = Environment.ProcessorCount;
            var barrier = new Barrier(count, barrier1 =>
            {
                Trace.WriteLine($"phase: {barrier1.CurrentPhaseNumber}");
            });

            var tasks = new Task[count];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 10; j++)
                    {
                        Thread.Sleep(100);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 1.");
                        barrier.SignalAndWait();

                        Thread.Sleep(100);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 2.");
                        barrier.SignalAndWait();

                        Thread.Sleep(100);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 3.");
                        barrier.SignalAndWait();

                        Thread.Sleep(100);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 4.");
                        barrier.SignalAndWait();
                    }
                });
            }

            var task = Task.Factory.ContinueWhenAll(tasks, tasks1 =>
            {
                Trace.WriteLine("all tasks were executed.");
                barrier.Dispose();
            });

            task.Wait();
        }

        [Fact]
        public void BarrierExceptionTest()
        {
            var barrier = new Barrier(3, barrier1 =>
            {
                if (barrier1.CurrentPhaseNumber == 2)
                {
                    throw new Exception("phase number 2.");
                }
            });

            var tasks = new Task[3];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(100);
                    Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 1.");
                    barrier.SignalAndWait();

                    Thread.Sleep(100);
                    Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 2.");
                    barrier.SignalAndWait();

                    Thread.Sleep(100);
                    Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 3.");
                    try
                    {
                        barrier.SignalAndWait();
                    }
                    catch (BarrierPostPhaseException ex)
                    {
                        Trace.WriteLine(ex.Message);
                        Trace.WriteLine(ex.InnerException.Message);
                    }
                });
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                barrier.Dispose();
            }
        }

        [Fact]
        public void BarrierCancellationTest()
        {
            var count = Environment.ProcessorCount;
            var barrier = new Barrier(count, barrier1 =>
            {
                Trace.WriteLine($"phase: {barrier1.CurrentPhaseNumber}");
            });

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            var tasks = new Task[count];

            for (int i = 0; i < tasks.Length; i++)
            {
                var i1 = i;
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        Thread.Sleep(i1 == 0 ? 1000 : 10);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 1.");
                        if (!barrier.SignalAndWait(100, token))
                        {
                            throw new TimeoutException($"phase_{barrier.CurrentPhaseNumber} #{Thread.CurrentThread.ManagedThreadId} task execute time exceed 100ms.");
                        }

                        Thread.Sleep(100);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 2.");
                        barrier.SignalAndWait(token);

                        Thread.Sleep(100);
                        Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId} operation 3.");
                        barrier.SignalAndWait(token);
                    }
                }, token);
            }

            var task = Task.Factory.ContinueWhenAll(tasks, tasks1 =>
            {
                //这个等待是为了让tasks的异常抛出到这里
                Task.WaitAll(tasks1);

                Trace.WriteLine("all tasks were executed.");
            }, token);

            try
            {
                if (!task.Wait(100 * 3 * 3))
                {
                    Trace.WriteLine("finally task timeout.");

                    //get all faulted task
                    foreach (var task1 in tasks)
                    {
                        if (task1.Status != TaskStatus.RanToCompletion)
                        {
                            if (task1.Status == TaskStatus.Faulted)
                            {
                                //get errors
                                foreach (var innerException in task1.Exception.InnerExceptions)
                                {
                                    Trace.WriteLine($"faulted task: {innerException.Message}");
                                }
                            }
                        }
                    }

                    tokenSource.Cancel();
                }
            }
            catch (AggregateException ex)
            {
                //innerException is AggregateException
                foreach (var exception in ex.InnerExceptions)
                {
                    var innerException = (AggregateException)exception;
                    foreach (var aggregateInner in innerException.InnerExceptions)
                    {
                        Trace.WriteLine(aggregateInner.Message);
                    }
                }
            }
            finally
            {
                barrier.Dispose();
            }
        }

        [Fact]
        public void MonitorTest()
        {
            var lockObj = new object();

            var tasks = new Task[3];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    Trace.WriteLine($"parallel execution. #{Thread.CurrentThread.ManagedThreadId,-4} {DateTime.Now.Ticks,19}");

                    bool lockTaken = false;
                    try
                    {
                        Monitor.Enter(lockObj, ref lockTaken);

                        Thread.Sleep(50);
                        Trace.WriteLine($"serial execution. #{Thread.CurrentThread.ManagedThreadId,-4} {DateTime.Now.Ticks,19}");
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            Monitor.Exit(lockObj);
                        }
                    }
                });
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void MonitorTimeoutTest()
        {
            var lockObj = new object();

            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(lockObj, 500, ref lockTaken);

                if (!lockTaken)
                {
                    throw new TimeoutException("lock timeout.");
                }
                Thread.Sleep(50);
                Trace.WriteLine($"serial execution. #{Thread.CurrentThread.ManagedThreadId,-4} {DateTime.Now.Ticks,19}");
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(lockObj);
                }
            }
        }

        [Fact]
        public void SpinLockTest()
        {
            var spinLock = new SpinLock(false);

            var tasks = new Task[3];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    Trace.WriteLine($"parallel execution. #{Thread.CurrentThread.ManagedThreadId,-4} {DateTime.Now.Ticks,19}");

                    bool lockTaken = false;
                    try
                    {
                        spinLock.Enter(ref lockTaken);
                        Thread.Sleep(10);
                        Trace.WriteLine($"serial execution. #{Thread.CurrentThread.ManagedThreadId,-4} {DateTime.Now.Ticks,19}");
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            spinLock.Exit();
                        }
                    }
                });
            }

            try
            {
                Task.WaitAll(tasks);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SpinWaitTest()
        {
            var wait = new SpinWait();
        }

        [Fact]
        public void ManualResetEventSlimTest()
        {
            var manualReset = new ManualResetEventSlim(false, 100);

            var task1 = Task.Factory.StartNew(() =>
            {
                manualReset.Wait();

                Trace.WriteLine("signal was seted.");
                Trace.WriteLine("task1 is running.");
            });

            var task2 = Task.Factory.StartNew(() =>
            {
                Trace.WriteLine("task2 is running.");

                manualReset.Set();
            });

            try
            {
                Task.WaitAll(task1, task2);
            }
            finally 
            {
                manualReset.Dispose();
            }
        }

        [Fact]
        public void SemaphoreSlimTest()
        {
            var semaphore = new SemaphoreSlim(5);

            var tasks = new Task[Environment.ProcessorCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        semaphore.Wait();

                        Trace.WriteLine($"thread#{Thread.CurrentThread.ManagedThreadId} {j}th enter. avaliable count: {semaphore.CurrentCount}");
                        Thread.Sleep(100);

                        semaphore.Release();
                        Trace.WriteLine($"thread#{Thread.CurrentThread.ManagedThreadId} {j}th leave. avaliable count: {semaphore.CurrentCount}");
                    }
                });
            }

            var lastest = Task.Factory.ContinueWhenAll(tasks, tasks1 =>
                {
                    Task.WaitAll(tasks1);
                    Trace.WriteLine("all tasks were compleated.");
                });

            try
            {
                lastest.Wait();
            }
            finally
            {
                semaphore.Dispose();
            }
        }


        private volatile int count;
        [Fact]
        public void SemaphoreTest()
        {
            var semaphore = new Semaphore(3, 5);
            var queue = new ConcurrentDictionary<string, DateTime>();

            var tasks = new Task[Environment.ProcessorCount];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var cur = DateTime.Now;
                        queue.TryAdd($"thread#{Thread.CurrentThread.ManagedThreadId} {j}th wait. count:{count} {cur.ToString("HH:mm:ss.fff")}", cur);

                        semaphore.WaitOne();

                        count -= 1;
                        cur = DateTime.Now;
                        queue.TryAdd($"thread#{Thread.CurrentThread.ManagedThreadId} {j}th enter. count:{count} {cur.ToString("HH:mm:ss.fff")}", cur);

                        Thread.Sleep(j == 0 ? 3000 : 3000);

                        semaphore.Release();

                        count += 1;
                        cur = DateTime.Now;
                        queue.TryAdd($"thread#{Thread.CurrentThread.ManagedThreadId} {j}th leave. count:{count} {cur.ToString("HH:mm:ss.fff")}", cur);
                    }
                });
            }

            var lastest = Task.Factory.ContinueWhenAll(tasks, tasks1 =>
                {
                    Task.WaitAll(tasks1);

                    foreach (var keyValuePair in queue.OrderBy(o => o.Value))
                    {
                        Trace.WriteLine(keyValuePair.Key);
                    }

                    Trace.WriteLine("all tasks were compleated.");
                    Trace.WriteLine($"count: {count}");
                });

            try
            {
                lastest.Wait();
            }
            finally
            {
                semaphore.Dispose();
            }
        }

        /// <summary>
        /// Countdowns 轻量级同步原语，信号计数
        /// </summary>
        [Fact]
        public void CountdownEventTest()
        {
            var count = Environment.ProcessorCount;
            var countdown = new CountdownEvent(count);

            var tasks = new Task[count];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Thread.Sleep(100);

                        throw new Exception("catch this without Task.WaitAll() ?");
                    }
                    finally
                    {
                        countdown.Signal();
                        Trace.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} was completed. CurrentCount: {countdown.CurrentCount}");
                    }
                });
            }

            try
            {
                countdown.Wait();

                //catch tasks exceptions
                Task.WaitAll(tasks);

                Trace.WriteLine("all tasks over.");
            }
            finally
            {
                countdown.Dispose();   
            }
        }
    }
}
