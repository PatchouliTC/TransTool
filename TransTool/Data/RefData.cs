using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransTool
{
    public enum RefType
    {
        CN=1,
        EN=2,
    }
    /// <summary>
    /// 参考文本块
    /// </summary>
    [Serializable]
    public class DataBlock : UpdateData
    {
        private string cn;
        private string en;
        public DataBlock(string cn,string en)
        {
            this.EN = en;
            this.CN = cn;
        }
        [JsonProperty("CN")]
        public string CN
        {
            get { return cn; }
            set
            {
                if (value != this.cn)
                {
                    this.cn = value;
                    NotifyPropertyChanged("CN");
                }
            }
        }
        [JsonProperty("EN")]
        public string EN
        {
            get { return en; }
            set
            {
                if (value != this.en)
                {
                    this.en = value;
                    NotifyPropertyChanged("CN");
                }
            }
        }
        public override string ToString()
        {
            return $"CN: {this.CN}\tEN: {this.EN}";
        }
    }
    /// <summary>
    /// 事件更新的字符串块
    /// </summary>
    [Serializable]
    public class MyString:UpdateData
    {
        private string str;
        public MyString(string str) { this.str = str; }
        [JsonProperty("Str")]
        public string Str
        {
            get { return this.str; }
            set
            {
                if (value != this.str)
                {
                    this.str = value;
                    NotifyPropertyChanged("Str");
                }
            }
        }
    }

    public class RefData:UpdateData
    {
        private int version;
        /// <summary>
        /// 参考翻译文本内容
        /// </summary>
        private Dictionary<string, ObservableCollection<DataBlock>> refTranslation=null;
        /// <summary>
        /// 参考模板文本内容
        /// </summary>
        private ObservableCollection<DataBlock> refTemplate = null;
        /// <summary>
        /// 注释文本内容
        /// </summary>
        private ObservableCollection<MyString> refNotice = null;
        /// <summary>
        /// 搜索文本框
        /// </summary>
        private string searchText = null;
        public RefData() { this.Version = version; }
        public RefData(int version)
        {
            this.version = version;
        }
        /// <summary>
        /// 获取版本信息
        /// </summary>
        public int Version { get
            {
                return this.version;
            }
            set
            {
                this.version = value; NotifyPropertyChanged("Version");
            }
        }
        public Dictionary<string, ObservableCollection<DataBlock>> RefTranSlation
        {
            get
            {
                if (this.refTranslation == null)
                    this.refTranslation = new Dictionary<string, ObservableCollection<DataBlock>>();
                return this.refTranslation;
            }
            set
            {
                this.refTranslation = value;
                NotifyPropertyChanged("RefTranslation");
            }
        }
        public ObservableCollection<DataBlock> RefTemplate
        {
            get
            {
                if (this.refTemplate == null)
                    this.refTemplate = new ObservableCollection<DataBlock>();
                return this.refTemplate;
            }
            set
            {
                this.refTemplate = value;
                NotifyPropertyChanged("RefTemplate");
            }
        }
        public ObservableCollection<MyString> RefNotice
        {

            get
            {
                if (this.refNotice == null)
                    this.refNotice = new ObservableCollection<MyString>();
                return this.refNotice;
            }
            set
            {
                this.refNotice = value;
                NotifyPropertyChanged("RefNotice");
            }
        }

        public string SearchText
        {
            get { return this.searchText; }
            set
            {
                this.searchText = value;
                NotifyPropertyChanged("SearchText");
                NotifyPropertyChanged("SearchItems");
            }
        }
        public IEnumerable<DataBlock> SearchItems
        {
            get
            {
                if (this.SearchText == null || this.SearchText == "")
                    return null;
                var res = from translation in this.RefTranSlation
                          from list in translation.Value
                          where (Const.Chinese.IsMatch(this.SearchText) ?
                          Const.ToSimplified(list.CN).Contains(Const.ToSimplified(this.SearchText)) :
                          list.EN.ToUpper().Contains(this.SearchText.ToUpper()))
                          select (list);
                return res;
            }
        }
        /// <summary>
        /// 向翻译列表字典添加数据
        /// </summary>
        /// <param name="type">数据所属数据类型</param>
        /// <param name="db">数据块</param>
        public void AddTranslate(string type,DataBlock db)
        {
            ObservableCollection<DataBlock> value=null;
            this.RefTranSlation.TryGetValue(type,out value);
            if (value == null)
            {
                value = new ObservableCollection<DataBlock>();
                this.refTranslation.Add(type, value);
            }
            value.Add(db);
        }
        /// <summary>
        /// 获取指定包含制定参数的文本块
        /// </summary>
        /// <param name="type">文本所属</param>
        /// <param name="data">文本</param>
        /// <param name="rt">文本类别</param>
        /// <returns></returns>
        public DataBlock GetTranslate(string type,string data, RefType rt)
        {
            ObservableCollection<DataBlock> value=null;
            this.RefTranSlation.TryGetValue(type, out value);
            if (value != null)
            {
                DataBlock db = null;
                switch (rt)
                {
                    case RefType.CN:
                        db = value.First(item => item.CN.Equals(data));
                        break;
                    case RefType.EN:
                        db = value.First(item => item.EN.Equals(data));
                        break;
                    default:
                        break;
                }
                if (db == null)
                    return null;
                return db;
                
            }
            return null;
        }
        /// <summary>
        /// 编辑指定文本块对应文本
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <param name="rt"></param>
        /// <returns></returns>
        public bool EditTranslate(string type, string data, RefType rt)
        {
            DataBlock db = this.GetTranslate(type, data, rt);
            if (db == null)
                return false;
            switch (rt)
            {
                case RefType.CN:
                    db.CN = data;
                    break;
                case RefType.EN:
                    db.EN = data;
                    break;
                default:
                    break;
            }
            return true;
        }

        public void AddTemplate(DataBlock db)
        {
            this.RefTemplate.Add(db);
        }
        public DataBlock GetTemplate(string data, RefType rt)
        {
            if(rt==RefType.EN)
                return this.RefTemplate.First(item => item.EN.Equals(data));
            return this.RefTemplate.First(item => item.CN.Equals(data));
        }
        public bool EditTemplate(string data, RefType rt)
        {
            DataBlock db = this.GetTemplate(data, rt);
            if (db == null)
                return false;
            switch (rt)
            {
                case RefType.CN:
                    db.CN = data;
                    break;
                case RefType.EN:
                    db.EN = data;
                    break;
                default:
                    break;
            }
            return true;
        }
        public void AddNotice(MyString s)
        {
            this.RefNotice.Add(s);
        }
        public string GetNotice(MyString data)
        {
            return this.RefNotice.First(i => (i.Str).Equals(data.Str)).Str;
        }
    }
}
