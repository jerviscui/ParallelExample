using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp
{
    public static class Common
    {
        private static readonly string[] _fileNames = {
            "file01.txt",
            "file02.txt",
            "file03.txt",
            "file04.txt",
            "file05.txt",
            "file06.txt",
            "file07.txt",
            "file08.txt" };

        public static int FilesCount()
        {
            return _fileNames.Length;
        }

        public static string[] GetAllFiles()
        {
            //.../bin/Debug/
            var dir = Environment.CurrentDirectory; //System.AppDomain.CurrentDomain.BaseDirectory;

            //.../
            var projDir = Directory.GetParent(dir.TrimEnd('\\'))?.Parent?.FullName;
            if (projDir == null)
            {
                throw new DirectoryNotFoundException();
            }

            return _fileNames.Select(o => Path.Combine(projDir, o)).ToArray();
        }
    }
}
