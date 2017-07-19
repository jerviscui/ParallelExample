using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Concurrency
{
    public class FileTraceListener : TraceListener
    {
        private static readonly ConcurrentQueue<Message> Messages = new ConcurrentQueue<Message>();
        
        private static string _fileName;

        public FileTraceListener(string fileName = "result.txt")
        {
            _fileName = fileName;
        }

        /// <summary>在派生类中被重写时，向在该派生类中所创建的侦听器写入指定消息。</summary>
        /// <param name="message">要写入的消息。</param>
        public override void Write(string message)
        {
            Messages.Enqueue(new Message(message));
        }

        /// <summary>在派生类中被重写时，向在该派生类中所创建的侦听器写入消息，后跟行结束符。</summary>
        /// <param name="message">要写入的消息。</param>
        public override void WriteLine(string message)
        {
            Messages.Enqueue(new Message(message));
        }

        public static void WriteLog()
        {
            if (!Messages.IsEmpty)
            {
                File.WriteAllLines(Path.GetFullPath(_fileName), Messages.ToArray().OrderBy(o => o.CurTime).Select(o => o.Content));
            }
        }
    }

    class Message
    {
        public Message(string message)
        {
            Content = message;
            CurTime = DateTime.Now;
        }

        public DateTime CurTime { get; set; }

        public string Content { get; set; }
    }
}
