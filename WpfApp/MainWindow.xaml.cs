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
using System.Windows.Navigation;

namespace WpfApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string[] _fileNames = {
            "file01.txt",
            "file02.txt",
            "file03.txt",
            "file04.txt",
            "file05.txt",
            "file06.txt",
            "file07.txt",
            "file08.txt" };

        private List<Task<string>> fileTasks;
        private const int BufferSize = 0x2000;

        public MainWindow()
        {
            InitializeComponent();

            fileTasks = new List<Task<string>>(_fileNames.Length);

            this.Closed += (sender, args) => System.Environment.Exit(0);
        }

        private void BtnConcate_Click(object sender, RoutedEventArgs e)
        {
            //.../bin/Debug/
            var dir = System.AppDomain.CurrentDomain.BaseDirectory;

            //.../
            var projDir = Directory.GetParent(dir.TrimEnd('\\'))?.Parent?.FullName;
            if (projDir == null)
            {
                throw new DirectoryNotFoundException();
            }

            foreach (var fileName in _fileNames)
            {
                fileTasks.Add(ReadAllTextAsync(Path.Combine(projDir, fileName)));
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
                Console.WriteLine(Properties.Resources.ReadAllTextAsync_Completed, task1.Id, task1.Result, stream.Name, DateTime.Now.ToString("HH:mm:ss.ffffff"));

                return task1.Result > 0 ? new UTF8Encoding().GetString(data) : "";
            }, TaskContinuationOptions.ExecuteSynchronously);
        }
    }
}
