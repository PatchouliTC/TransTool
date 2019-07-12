using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        public static string[] DimExist(string path, string[] target = null)
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
        public static List<FileInfo> GetFile(string dimpath, FileType fileType = FileType.all)
        {
            DirectoryInfo fdir = new DirectoryInfo(dimpath);
            FileInfo[] temp = fdir.GetFiles();
            if (fileType == FileType.all) return temp.ToList();
            List<FileInfo> list = new List<FileInfo>();
            string ftype = fileType.ToString();
            foreach (FileInfo fi in temp)
            {
                if (fi.Extension.Contains(ftype))
                    list.Add(fi);
            }
            return list;
        }
        /// <summary>
        /// 指定路径下是否包含该文件名的文件类型的文件
        /// </summary>
        /// <param name="dimpath"></param>
        /// <param name="name"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public static bool FileExist(string dimpath,string name,FileType fileType = FileType.all)
        {
            DirectoryInfo fdir = new DirectoryInfo(dimpath);
            FileInfo[] temp = fdir.GetFiles();
            foreach (FileInfo i in temp)
            {
                if (i.Name.Equals(name))
                {
                    if (fileType == FileType.all)
                        return true;
                    if (i.Extension.Contains(fileType.ToString()))
                        return true;
                }

            }
            return false;
        }
        /// <summary>
        /// 读取指定的json文本
        /// </summary>
        /// <param name="info">FileInfo格式文件信息</param>
        /// <param name="context">ref的参数位置</param>
        /// <returns></returns>
        public static bool ReadJson(FileInfo info,ref RefData context)
        {
            try
            {
                if (context == null)
                    context = new RefData();
                StreamReader s = info.OpenText();
                JsonTextReader reader = new JsonTextReader(s);
                JObject jObject = (JObject)JToken.ReadFrom(reader);
                //设置参考版本
                context.Version=(int)jObject["version"];
                //读取并添加所有翻译参考文本
                context.RefTranSlation=JsonConvert.DeserializeObject<Dictionary<string, ObservableCollection<DataBlock>>>(Regex.Replace(jObject["translation"].Value<Object>().ToString(), Environment.NewLine, ""));
                //读取并添加所有模板参考文本
                context.RefTemplate = JsonConvert.DeserializeObject<ObservableCollection<DataBlock>>(Regex.Replace(jObject["template"].Value<Object>().ToString(), Environment.NewLine, ""));
                //读取并添加所有提示建议文本
                context.RefNotice = JsonConvert.DeserializeObject<ObservableCollection<MyString>>(Regex.Replace(jObject["notice"].Value<Object>().ToString(), Environment.NewLine, ""));

                s.Close();
                return true;
            }
            catch
            {
                context = null;
                return false;
            }
        }
    }
}
