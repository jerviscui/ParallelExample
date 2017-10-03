﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
                    var innerException = (AggregateException) exception;
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
    }
}
