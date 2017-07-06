using System;

namespace Concurrency
{
    public abstract class BaseTest : IDisposable
    {
        protected BaseTest()
        {
            //Trace.Listeners.Clear();
            //Trace.Listeners.Add(new TextWriterTraceListener(System.Console.Out));
        }

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            FileTraceListener.WriteLog();
        }

        public static void Exit()
        {
            Console.WriteLine(Environment.NewLine + "press Enter to exit.");
            Console.ReadLine();

            Program.Exit();
        }
    }
}
