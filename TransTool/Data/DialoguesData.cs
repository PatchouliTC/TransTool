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
        /// 文本块组织结构[MAP--->EVENT-->PAGE-->textblocks]
        /// </summary>
        private Dictionary<int, Dictionary<int, Dictionary<int, List<ViewData>>>> structure=null;
        public DialoguesData(string DM_V) {
            this.DM_Version = DM_V;
            this.structure = new Dictionary<int, Dictionary<int, Dictionary<int, List<ViewData>>>>();
            this.dialogues = new ObservableCollection<ViewData>();
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
                    ReadLocation(sc, null, type);
                }
            }
            return true;
        }
        /// <summary>
        /// 读取定位块数据
        /// </summary>
        /// <param name="sc">文本流</param>
        private bool ReadLocation(StreamReader sc,object o,TextType t)
        {
            string data = sc.ReadLine();//读取#下一行，判断该值为那个类型
            if (Const.LocationMap.IsMatch(data))
            {
                //获取作为MAP的int数值
                int num=int.Parse(Const.LocationMap.Match(data).Value);
                //将该map添加进字典
                Dictionary<int, Dictionary<int, List<ViewData>>> temp;
                //从现有队列尝试获取已有值，如果没有建立新的
                if (!this.structure.TryGetValue(num, out temp))
                {
                    temp = new Dictionary<int, Dictionary<int, List<ViewData>>>();
                    this.structure.Add(num, temp);
                }
                //根据规则 MAP下一级为Event
                //跳过下一个#
                sc.ReadLine();
                //跳过两者间空格
                sc.ReadLine();
                //跳过EVENT的上方#
                sc.ReadLine();
                //递归读取
                this.ReadLocation(sc, temp, t);
            }
            if (Const.LocationEvent.IsMatch(data))
            {
                //进入EVENT块，说明object已经传递了Map对应的索引
                //获取作为Event的int数值
                int num = int.Parse(Const.LocationEvent.Match(data).Value);
                //获取该索引字典
                Dictionary<int, List<ViewData>> temp;
                if (!((o as Dictionary<int, Dictionary<int, List<ViewData>>>)).TryGetValue(num, out temp))
                {
                    temp = new Dictionary<int, List<ViewData>>();
                    (o as Dictionary<int, Dictionary<int, List<ViewData>>>).Add(num, temp);
                }
                //跳过下一个#
                sc.ReadLine();
                //跳过两者间空格
                sc.ReadLine();
                //跳过PAGE的上方#
                sc.ReadLine();
                this.ReadLocation(sc, temp, t);
            }
            if (Const.LocationPage.IsMatch(data))
            {
                //进入PAGE块，说明object已经传递了Page对应的索引
                //获取作为Page的int数值
                int num = int.Parse(Const.LocationPage.Match(data).Value);
                bool isHas = true;
                //特殊处理，准备进入Page内部的文本块递归
                List<ViewData> temp;
                if(!(o as Dictionary<int, List<ViewData>>).TryGetValue(num,out temp))
                {
                    temp = new List<ViewData>();
                    (o as Dictionary<int, List<ViewData>>).Add(num, temp);
                    isHas = false;
                }    
                //temp用于存储文本块的链表
                //跳过Page下面的#
                sc.ReadLine();
                //如果该page存在
                if (isHas)
                {
                    //两者的文本块结构应该相同
                    IEnumerator<ViewData> next = temp.GetEnumerator();
                    ReadNewBlock(sc, next, t);
                }
                else
                {
                    //page不存在，直接压入数据
                    ViewData dt = new ViewData();
                    //读取PAGE下第一个----对应的参数并压入相应的List中
                    SaveAs(t, dt, new string[1] { sc.ReadLine() });
                    temp.Add(dt);
                    ReadNewBlock(sc, temp,t);
                }
            }
            //都没匹配到，说明该行非法【DM规则#与#之间应为map/event/page】
            return false;
        }

        private void ReadNewBlock(StreamReader sc, List<ViewData> temp,TextType t)
        {
            string data = sc.ReadLine();//此时获取的是正文本或者空格,空格表示该page结束
            if (data == "")//空行，page结束
                return ;
            List<string> lblock=new List<string>();
            int isChoice = -1;
            //此时读取到的data即为第一行文本，避免发生第一行即为选项+在此进行验证
            if (Const.ChoiceBlock.IsMatch(data))
            {
                isChoice++;
            }
            else
            {
                lblock.Add(data);
            }
            string endline = ReadTextLine(sc, lblock, ref isChoice);
            string[] block = lblock.ToArray();
            ViewData vdata = new ViewData(isChoice);
            SaveAs(t, vdata, block);
            temp.Add(vdata);
            ViewData end = new ViewData();
            SaveAs(t, end, new string[1] { endline });
            temp.Add(end);
            ReadNewBlock(sc, temp, t);
        }
        private void ReadNewBlock(StreamReader sc, IEnumerator<ViewData> temp, TextType t)
        {
            string data = sc.ReadLine();//此时获取的是正文本或者空格,空格表示该page结束
            if (data == "")//空行，page结束
                return;
            List<string> lblock = new List<string>();
            int isChoice = -1;
            string endline = ReadTextLine(sc, lblock, ref isChoice);
            string[] block = lblock.ToArray();
            //储存文本块
            SaveAs(t, temp.Current, block);
            temp.MoveNext();
            //储存最后的虚线块
            SaveAs(t, temp.Current, new string[1] { endline });
            ReadNewBlock(sc, temp, t);
        }
        /// <summary>
        /// 文本块递归
        /// </summary>
        /// <param name="sc"></param>
        /// <param name="Block"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private string ReadTextLine(StreamReader sc, List<string> Block,ref int i)
        {
            string data = sc.ReadLine();
            if (Const.TextBlock.IsMatch(data))//---,文本结束终止递归
                return data;
            if (Const.ChoiceBlock.IsMatch(data))//+号 选择块
            {
                i++;
                return ReadTextLine(sc,Block,ref i);
            }
            Block.Add(data);
            return ReadTextLine(sc, Block, ref i);
        }
        /// <summary>
        /// 用于根据类型填充对应文本块不同位置的数据
        /// </summary>
        /// <param name="type"></param>
        /// <param name="vd">数据来源，需要初始化</param>
        /// <param name="data"></param>
        private void SaveAs(TextType type,ViewData vd,string[] data)
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
