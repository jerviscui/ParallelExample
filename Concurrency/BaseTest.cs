using System;
using System.Diagnostics;
using System.Security.Permissions;

namespace Concurrency
{
    public abstract class BaseTest : IDisposable
    {
        private Stopwatch _timmer;

        protected BaseTest()
        {
            //Trace.Listeners.Clear();
            //Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));

            _timmer = Stopwatch.StartNew();
        }

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            _timmer.Stop();
            Trace.WriteLine(_timmer.Elapsed);
            FileTraceListener.WriteLog();
        }

        public static void Exit()
        {
            Console.WriteLine(Environment.NewLine + "press Enter to exit.");
            Console.ReadLine();

            Program3.Exit();
        }
    }
}
