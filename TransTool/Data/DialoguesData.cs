using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransTool
{
    public enum TextType
    {
        Original=1,
        Posttranslation,
    }
    /// <summary>
    /// 读取到的文本内文件
    /// </summary>
    public class DialoguesData:UpdateData
    {
        /// <summary>
        /// DM版本
        /// </summary>
        public string DM_Version { get;private set; }
        /// <summary>
        /// 保存的文本块列表
        /// </summary>
        private ObservableCollection<ViewData> dialogues=null;
        /// <summary>
        /// 文本块组织结构[MAP--->EVENT-->PAGE]
        /// </summary>
        private Dictionary<int, Dictionary<int, Dictionary<int, ViewData>>> structure=null;
        DialoguesData(string DM_V) {
            this.DM_Version = DM_V;
        }
        /// <summary>
        /// 获取的文本信息总块
        /// </summary>
        public ObservableCollection<ViewData> Dialogues
        {
            get { return this.dialogues; }
            private set { }
        }
        /// <summary>
        /// 格式化对话文本
        /// </summary>
        /// <param name="sc">对话文本流</param>
        /// <param name="type">文本类型</param>
        /// <returns></returns>
        public bool ReadDialogues(StreamReader sc,TextType type)
        {
            string _temp = null;
            while (!sc.EndOfStream)
            {
                //将当前行读入内存
                _temp=sc.ReadLine();
                if (Const.LocationBlock.IsMatch(_temp))
                {
                    //发现坐标块开头###标记

                }
                if (Const.TextBlock.IsMatch(_temp))
                {
                    //发现文本/选择块开头---标记

                }
            }
            return true;
        }
        /// <summary>
        /// 读取定位块数据
        /// </summary>
        /// <param name="sc">文本流</param>
        private void ReadLocation(StreamReader sc)
        {
            int num = -1;
            int 
        }
        /// <summary>
        /// 用于根据类型填充对应文本块不同位置的数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="vd">数据来源，需要初始化</param>
        /// <param name="data"></param>
        private void SaveAs(TextType type,ref ViewData vd,string[] data)
        {
            switch (type)
            {
                case TextType.Original:
                    vd.CNData = data;
                    break;
                case TextType.Posttranslation:
                    vd.ENData = data;
                    break;
                default:
                    break;
            }
        }
    }
}
