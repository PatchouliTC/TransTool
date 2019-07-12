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
    public enum LastType
    {
        NA=1,
        Map,
        Event,
        Page,
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
        public DialoguesData()
        {
            this.DM_Version = "";
            this.structure = new Dictionary<int, Dictionary<int, Dictionary<int, List<ViewData>>>>();
            this.dialogues = new ObservableCollection<ViewData>();
        }
        public DialoguesData(string DM_V) {
            this.DM_Version = DM_V;
            this.structure = new Dictionary<int, Dictionary<int, Dictionary<int, List<ViewData>>>>();
            this.dialogues = new ObservableCollection<ViewData>();
        }
        private int nowMap = -1;
        private string templatestr = null;
        /// <summary>
        /// 获取的文本信息总块
        /// </summary>
        public ObservableCollection<ViewData> Dialogues
        {
            get { return this.dialogues; }
            set {
                if (this.dialogues != null && value != this.dialogues)
                {
                    this.dialogues = value;
                    NotifyPropertyChanged("Dialogues");
                }
            }
        }
        /// <summary>
        /// 每次重用时请在此初始化数据
        /// </summary>
        public void InitData(string version)
        {
            this.DM_Version = version;
            this.dialogues.Clear();
            this.structure.Clear();
            this.nowMap = -1;
            this.templatestr = null;
        }
        /// <summary>
        /// 格式化对话文本
        /// </summary>
        /// <param name="sc">对话文本流</param>
        /// <param name="type">文本类型</param>
        public void ReadDialogues(StreamReader sc,TextType type)
        {
            string _temp = null;
            while (!sc.EndOfStream)
            {
                //将当前行读入内存
                _temp=sc.ReadLine();
                if (Const.LocationBlock.IsMatch(_temp))
                {
                    this.templatestr = (this.templatestr == null) ? _temp : this.templatestr;
                    //发现坐标块开头###标记
                    ReadLocation(sc, null, type);
                }
            }
        }
        /// <summary>
        /// 将文本逐行写入文件
        /// </summary>
        /// <param name="sw"></param>
        public bool SaveDialogues(StreamWriter sw)
        {
            try
            {
                sw.WriteLine(this.DM_Version);//写入DM版本
                sw.WriteLine("");
                sw.WriteLine("");//文本规则 两行空行
                                 //遍历所有MAP
                foreach (KeyValuePair<int, Dictionary<int, Dictionary<int, List<ViewData>>>> map in this.structure)
                {
                    sw.WriteLine(templatestr);//添加#模板
                    sw.WriteLine($"MAP : {map.Key} [{map.Key}]");//添加MAP标签
                    sw.WriteLine(templatestr);//添加#模板
                    sw.WriteLine("");
                    foreach (KeyValuePair<int, Dictionary<int, List<ViewData>>> eve in map.Value)
                    {
                        sw.WriteLine(templatestr);//添加#模板
                        sw.WriteLine($"EVENT {eve.Key} [{eve.Key}]");//添加EVENT标签
                        sw.WriteLine(templatestr);//添加#模板
                        sw.WriteLine("");
                        foreach (KeyValuePair<int, List<ViewData>> page in eve.Value)
                        {
                            sw.WriteLine(templatestr);//添加#模板
                            sw.WriteLine($"PAGE {page.Key} [{page.Key}]");//添加PAGE标签
                            sw.WriteLine(templatestr);//添加#模板
                                                      //PAGE紧接对应的文本块信息，不需要空行
                            foreach (ViewData data in page.Value)
                            {
                                if (data.IsSelectBlock == -1)
                                {
                                    //当前块为纯文本块
                                    foreach (string s in data.ENData)
                                    {
                                        sw.WriteLine(s);
                                    }
                                }
                                else
                                {
                                    List<string> text = data.ENData.ToList();
                                    text.Insert(data.IsSelectBlock, data.SelectStr);
                                    foreach (string s in text)
                                    {
                                        sw.WriteLine(s);
                                    }
                                }
                            }
                            sw.WriteLine("");//每个page结束都有一个空行
                        }
                    }
                }
                sw.Close();
                return true;
            }
            catch
            {
                sw.Close();
                return false;
            }
        }
        /// <summary>
        /// 读取定位块数据
        /// </summary>
        /// <param name="sc">文本流</param>
        private void ReadLocation(StreamReader sc,object o,TextType t,LastType l= LastType.NA)
        {
            if (sc.EndOfStream)
                return;
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
                this.nowMap = num;
                //递归读取
                this.ReadLocation(sc, temp, t,LastType.Map);
            }
            if (Const.LocationEvent.IsMatch(data))
            {
                if (o == null)
                    return;
                //获取作为Event的int数值
                int num = int.Parse(Const.LocationEvent.Match(data).Value);
                Dictionary<int, List<ViewData>> temp=null;
                //EVENT块属于三级数据关系中位，需要额外考虑
                if (l == LastType.Map)//说明o来自于map传递值，此时只需要直接在其中添加新的Event项目
                {
                    if (!((o as Dictionary<int, Dictionary<int, List<ViewData>>>)).TryGetValue(num, out temp))
                    {
                        temp = new Dictionary<int, List<ViewData>>();
                        (o as Dictionary<int, Dictionary<int, List<ViewData>>>).Add(num, temp);
                    }
                }
                else if(l==LastType.Page)//说明o来自于page结束后的传值，需要对应的map下的event字典
                {
                    //获取当前map下的event字典
                    Dictionary<int, Dictionary<int, List<ViewData>>>  _temp = this.structure[this.nowMap];
                    if (!(_temp.TryGetValue(num, out temp)))
                    {
                        temp = new Dictionary<int, List<ViewData>>();
                        _temp.Add(num, temp);
                    }                                                          
                }
                //跳过下一个#
                sc.ReadLine();
                //跳过两者间空格
                sc.ReadLine();
                //跳过PAGE的上方#
                sc.ReadLine();
                this.ReadLocation(sc, temp, t,LastType.Event);
            }
            if (Const.LocationPage.IsMatch(data))
            {
                if (o == null)
                    return;
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
                    next.MoveNext();//推进到第一个元素
                    SaveAs(t, next.Current, new string[1] { sc.ReadLine() });//第一个元素应该为文本块开始的第一个虚线
                    next.MoveNext();//推进到正式文本块
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
                if (sc.EndOfStream)//到达最后一个page
                    return;
                sc.ReadLine();//根据规则，page块结束不是文档结束就是下一个EVENT/MAP/EVENT,跳过###
                ReadLocation(sc, o, t, LastType.Page);
            }
            //都没匹配到，说明该行非法【DM规则#与#之间应为map/event/page】
            return;
        }

        private void ReadNewBlock(StreamReader sc, List<ViewData> temp,TextType t)
        {
            if (sc.EndOfStream)
                return;
            string data = sc.ReadLine();//此时获取的是正文本或者空格,空格表示该page结束
            if (data == "")//空行，page结束
                return;
            List<string> lblock = new List<string>();
            int isChoice = -1;
            if (Const.ChoiceBlock.IsMatch(data))//第一行是++表示选择块
                isChoice = 0;
            lblock.Add(data);
            string endline = ReadTextLine(sc, lblock, ref isChoice);

            ViewData vdata = new ViewData(isChoice);
            if (isChoice != -1)
            {
                vdata.SelectStr = lblock[isChoice];
                lblock.RemoveAt(isChoice);
            }
            string[] block = lblock.ToArray();
            SaveAs(t, vdata, block);
            temp.Add(vdata);
            //新数据加入，同时该文本块将用于显示内容
            this.dialogues.Add(vdata);
            ViewData end = new ViewData();
            SaveAs(t, end, new string[1] { endline });
            temp.Add(end);
            ReadNewBlock(sc, temp, t);
        }
        private void ReadNewBlock(StreamReader sc, IEnumerator<ViewData> temp, TextType t)
        {
            if (sc.EndOfStream)
                return;
            string data = sc.ReadLine();//此时获取的是正文本或者空格,空格表示该page结束
            if (data == "")//空行，page结束
                return;
            List<string> lblock = new List<string>();
            int isChoice = -1;
            if (Const.ChoiceBlock.IsMatch(data))//第一行是++表示选择块
                isChoice = 0;
            lblock.Add(data);
            string endline = ReadTextLine(sc, lblock, ref isChoice);
            if (isChoice != -1)
            {
                temp.Current.SelectStr = lblock[isChoice];
                lblock.RemoveAt(isChoice);
            }
            string[] block = lblock.ToArray();
            //储存文本块
            SaveAs(t, temp.Current, block);
            temp.MoveNext();
            //储存最后的虚线块
            SaveAs(t, temp.Current, new string[1] { endline });
            temp.MoveNext();//推进到下一个块
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
            if (sc.EndOfStream)
                return null;
            string data = sc.ReadLine();
            if (Const.TextBlock.IsMatch(data))//---,文本结束终止递归
                return data;
            if (Const.ChoiceBlock.IsMatch(data))//+号 选择块
            {
                Block.Add(data);
                i = Block.Count - 1;//i表示+在其中的位置
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
