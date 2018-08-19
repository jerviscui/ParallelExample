using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        private List<Task<string>> readTasks;
        //2k
        private const int BufferSize = 0x2000;

        public Window2()
        {
            InitializeComponent();

            readTasks = new List<Task<string>>(Common.FilesCount());
        }

        private void UpdateProgressBar()
        {
            readProgbar.Value++;
        }

        private void startReadBtn_Click(object sender, RoutedEventArgs e)
        {
            readTasks.Clear();

            readProgbar.Value = 0;
            readProgbar.Maximum = Common.FilesCount();

            //获取当前（UI线程）异步上下文
            var uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            foreach (var file in Common.GetAllFiles())
            {
                var task = ReadAllText(file);

                //读取文件之后通过异步方式更新 UI
                task.ContinueWith(task1 =>
                {
                    UpdateProgressBar();
                    readProgbar.Dispatcher.in
                    //readProgbar.Dispatcher.InvokeAsync(() => readProgbar.Value++);
                    //readProgbar.Dispatcher.Invoke(() => readProgbar.Value++);
                }, uiScheduler);

                readTasks.Add(task);
            }

            var sbTask = Task.Factory.ContinueWhenAll(readTasks.ToArray(), tasks =>
            {
                var stringBuilder = new StringBuilder();

                foreach (Task<string> task in tasks)
                {
                    stringBuilder.AppendLine(task.Result);
                }

                return stringBuilder.ToString();
            });

            sbTask.ContinueWith(task => txtAllContent.Text = task.Result, uiScheduler);
        }

        private Task<string> ReadAllText(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("no have file.", file);
            }

            var fileInfo = new FileInfo(file);
            var data = new byte[fileInfo.Length];

            var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, true);

            var task = Task.Factory.FromAsync(stream.BeginRead, stream.EndRead, data, 0, data.Length, null,
                TaskCreationOptions.None);

            return task.ContinueWith(task1 =>
            {
                stream.Close();

                return task1.Result > 0 ? new UTF8Encoding().GetString(data) : "";
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
