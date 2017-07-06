using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Concurrency
{
    public class Program
    {
        /// <summary>
        /// 打开控制台
        /// </summary>
        /// <returns>Boolean.</returns>
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();

        /// <summary>
        /// 关闭控制台
        /// </summary>
        /// <returns>Boolean.</returns>
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();

        //[STAThread]
        //public static void Main(string[] args)
        //{
        //    AllocConsole();

        //    Application.EnableVisualStyles();
        //    Application.SetCompatibleTextRenderingDefault(false);
        //    Application.Run();
            
        //    FreeConsole();
        //}

        public static void Exit()
        {
            Application.Exit();
        }
    }
}
