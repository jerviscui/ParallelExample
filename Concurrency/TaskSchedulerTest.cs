using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Concurrency
{
    public class TaskSchedulerTest : BaseTest
    {
        [Fact]
        public void Test()
        {
            var scheduler = new MyScheduler();
            var taskFactory = new TaskFactory(scheduler);

            taskFactory.StartNew(() => Trace.WriteLine(""));
        }
    }

    public class MyScheduler : TaskScheduler
    {
        /// <summary>将 <see cref="T:System.Threading.Tasks.Task" /> 排队到计划程序中。</summary>
        /// <param name="task">要排队的 <see cref="T:System.Threading.Tasks.Task" />。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="task" /> 参数为 null。</exception>
        protected override void QueueTask(Task task)
        {
            throw new NotImplementedException();
        }

        /// <summary>确定是否可以在此调用中同步执行提供的 <see cref="T:System.Threading.Tasks.Task" />，如果可以，将执行该任务。</summary>
        /// <returns>一个布尔值，该值指示是否已以内联方式执行该任务。</returns>
        /// <param name="task">要执行的 <see cref="T:System.Threading.Tasks.Task" />。</param>
        /// <param name="taskWasPreviouslyQueued">一个布尔值，该值指示任务之前是否已排队。如果此参数为 True，则该任务以前可能已排队（已计划）；如果为 False，则已知该任务尚未排队，此时将执行此调用，以便以内联方式执行该任务，而不用将其排队。</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="task" /> 参数为 null。</exception>
        /// <exception cref="T:System.InvalidOperationException">已执行的 <paramref name="task" />。</exception>
        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new NotImplementedException();
        }

        /// <summary>仅对于调试器支持，生成当前排队到计划程序中等待执行的 <see cref="T:System.Threading.Tasks.Task" /> 实例的枚举。</summary>
        /// <returns>一个允许调试器遍历当前排队到此计划程序中的任务的枚举。</returns>
        /// <exception cref="T:System.NotSupportedException">此计划程序无法在此时生成排队任务的列表。</exception>
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }
    }
}