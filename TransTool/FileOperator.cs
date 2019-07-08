using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransTool
{
    public enum FileType
    {
        all=1,
        txt,
        tmp,
        word,
        json,
    }
    /// <summary>
    /// 文件操作类
    /// </summary>
    public static class FileOperator
    {
        /// <summary>
        /// 判断目录下有没有目标子文件夹
        /// </summary>
        /// <param name="path">文件夹路径</param>
        /// <param name="target">需要包含的文件夹名</param>
        /// <returns></returns>
        public static string[] DimExist(string path,string[] target = null)
        {
            if (target == null) return null;
            string[] dir = Directory.GetDirectories(path);
            if (dir == null) return null;
            if (target.All(t => dir.Any(b => b.Split('\\').Last().ToLower().Contains(t))))
                return dir;
            return null;
        }

        /// <summary>
        /// 获取目录下所有指定类型的文件信息
        /// </summary>
        /// <param name="dimpath">目录路径</param>
        /// <param name="fileType">文件类型</param>
        /// <returns></returns>
        public static List<FileInfo> GetFile(string dimpath, FileType fileType =FileType.all)
        {
            DirectoryInfo fdir = new DirectoryInfo(dimpath);
            FileInfo[] temp = fdir.GetFiles();
            if (fileType == FileType.all) return temp.ToList();
            List<FileInfo> list = new List<FileInfo>();
            string ftype = fileType.ToString();
            foreach(FileInfo fi in temp)
            {
                if (fi.Extension.Contains(ftype))
                    list.Add(fi);
            }
            return list;
        }


    }
}
