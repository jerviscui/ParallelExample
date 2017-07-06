using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;

namespace Concurrency
{
    public class FileTraceListener : TraceListener
    {
        private static readonly ConcurrentQueue<string> Messages = new ConcurrentQueue<string>();

        private static string _fileName;

        public FileTraceListener(string fileName)
        {
            _fileName = fileName;
        }

        /// <summary>在派生类中被重写时，向在该派生类中所创建的侦听器写入指定消息。</summary>
        /// <param name="message">要写入的消息。</param>
        public override void Write(string message)
        {
            Messages.Enqueue(message);
        }

        /// <summary>在派生类中被重写时，向在该派生类中所创建的侦听器写入消息，后跟行结束符。</summary>
        /// <param name="message">要写入的消息。</param>
        public override void WriteLine(string message)
        {
            Messages.Enqueue(message);
        }

        public static void WriteLog()
        {
            File.WriteAllLines(Path.GetFullPath(_fileName), Messages.ToArray());
        }
    }
}
