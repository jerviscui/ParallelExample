using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        

        private List<Task<string>> fileTasks;
        private const int BufferSize = 0x2000;

        public MainWindow()
        {
            InitializeComponent();

            fileTasks = new List<Task<string>>(Common.FilesCount());

            this.Closed += (sender, args) => System.Environment.Exit(0);
        }

        private void BtnConcate_Click(object sender, RoutedEventArgs e)
        {
            

            if (fileTasks.Any())
            {
                fileTasks.Clear();
            }

            foreach (var fileName in Common.GetAllFiles())
            {
                fileTasks.Add(ReadAllTextAsync(fileName));
            }

            Task.WaitAll(fileTasks.ToArray());

            foreach (var fileTask in fileTasks)
            {
                Console.WriteLine(fileTask.Result);
                Console.WriteLine("==============");
            }
        }

        /// <summary>
        /// 异步方式读取文件并返回内容
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException"></exception>
        private Task<string> ReadAllTextAsync(string path)
        {
            var fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                throw new FileNotFoundException(path);
            }

            byte[] data = new byte[fileInfo.Length];

            var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);

            //APM模式异步操作
            //var iar = stream.BeginRead(data, 0, data.Length, ar => { }, null);
            //var count = stream.EndRead(iar);

            //create task the first way
            //var task = Task.Factory.FromAsync(stream.BeginRead, stream.EndRead, data, 0, data.Length, null,
            //    TaskCreationOptions.None);

            //must be set FEATURE_ASYNC_IO to use ReadAsync method
            var task = stream.ReadAsync(data, 0, data.Length);

            return task.ContinueWith(task1 =>
            {
                stream.Close();
                Console.WriteLine(Properties.Resources.ReadAllTextAsync_Completed, task1.Id, Thread.CurrentThread.ManagedThreadId,  task1.Result, stream.Name, DateTime.Now.ToString("HH:mm:ss.ffffff"));

                return task1.Result > 0 ? new UTF8Encoding().GetString(data) : "";
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void BtnUnblock_Click(object sender, RoutedEventArgs e)
        {
            if (fileTasks.Any())
            {
                fileTasks.Clear();
            }

            foreach (var fileName in Common.GetAllFiles())
            {
                fileTasks.Add(ReadAllTextAsync(fileName));
            }

            //不阻塞 UI 线程，通过任务延续方式完成读取后的操作
            Task.Factory.ContinueWhenAll(fileTasks.ToArray(), tasks =>
            {
                Task.WaitAll(tasks);

                foreach (var task in tasks)
                {
                    //Console.WriteLine(task.Result);
                    Console.WriteLine("==============");
                }
            });
        }
    }
}
