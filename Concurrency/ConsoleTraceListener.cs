using System;
using System.Diagnostics;

namespace Concurrency
{
    public class ConsoleTraceListener : TraceListener
    {
        /// <summary>在派生类中被重写时，向在该派生类中所创建的侦听器写入指定消息。</summary>
        /// <param name="message">要写入的消息。</param>
        public override void Write(string message)
        {
            Console.Write(message);
        }

        /// <summary>在派生类中被重写时，向在该派生类中所创建的侦听器写入消息，后跟行结束符。</summary>
        /// <param name="message">要写入的消息。</param>
        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
