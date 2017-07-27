using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Concurrency
{
    public class TaskTest : BaseTest
    {
        [Fact]
        public void UseParallelStracksWindowTest()
        {
            var t1 = new Task(() => Trace.WriteLine("t1"));
            var t2 = new Task(() => Trace.WriteLine("t2"));

            t1.Start();
            t2.Start();
            
            Task.WaitAll(t1, t2);
        }
    }
}
