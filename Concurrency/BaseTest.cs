using System;
using System.Diagnostics;
using System.Security.Permissions;

namespace Concurrency
{
    public abstract class BaseTest : IDisposable
    {
        private readonly Stopwatch _timmer;

        protected bool SeeMemoryInfo { get; set; }

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

            if (SeeMemoryInfo)
            {
                GetMemoryInfo();
            }

            FileTraceListener.WriteLog();
        }

        public static void Exit()
        {
            Console.WriteLine(Environment.NewLine + "press Enter to exit.");
            Console.ReadLine();

            Program3.Exit();
        }

        private void GetMemoryInfo()
        {
            var process = Process.GetCurrentProcess();

            PerformanceCounter pf1 = new PerformanceCounter("Process", "Working Set - Private", process.ProcessName);
            PerformanceCounter pf2 = new PerformanceCounter("Process", "Working Set", process.ProcessName);
            Trace.WriteLine($"{process.ProcessName}:工作集(进程类)  {process.WorkingSet64 / 1024,12:N3} KB");
            Trace.WriteLine($"{process.ProcessName}:工作集          {pf2.NextValue() / 1024,12:N3} KB");
            //私有工作集
            Trace.WriteLine($"{process.ProcessName}:专用工作集      {pf1.NextValue() / 1024,12:N3} KB");
        }
    }
}
