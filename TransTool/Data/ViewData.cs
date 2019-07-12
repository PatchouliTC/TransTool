using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TransTool
{
    /// <summary>
    /// 封装通知消息方法
    /// </summary>
    public class UpdateData : INotifyPropertyChanged
    {
        /// <summary>
        /// 数据变更的消息通知事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 数据变更的消息通知调用
        /// </summary>
        /// <param name="propertyName">变更的属性名称</param>
        protected void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public object GetValue(string propertyName)
        {
            return this.GetType().GetProperty(propertyName).GetValue(this,null);
        }
    }
    public class ViewData: UpdateData
    {
        private string[] cndata=null;//每个数据块的中文文本
        private string[] endata=null;//每个数据块的英文文本
        private string cn=null;//中文文本
        private StringBuilder en=null;//英文文本
        #region 基本的读取获取
        /// <summary>
        /// 该数据块是否已经被初始化
        /// </summary>
        public bool IsInit { get; private set; }
        /// <summary>
        /// 该文本块是否为选项块[-1参数说明不是]
        /// </summary>
        public int IsSelectBlock { get; private set; }
        public string SelectStr { get; set; }
        public ViewData() {this.IsSelectBlock = -1;this.IsInit = false; }
        /// <summary>
        /// 选项块初始化
        /// </summary>
        /// <param name="type">选项++++在块中的位置</param>
        public ViewData(int type=-1) {this.IsSelectBlock = type; this.IsInit = false; }
        public string[] CNData
        {
            get
            {
                return this.cndata;
            }
            set
            {
                this.cndata = value;
                this.IsInit = false;
            }
        }
        public string[] ENData
        {
            get
            {
                return this.endata;
            }
            set
            {
                this.endata = value;
                this.IsInit = false;
            }
        }
        #endregion
        #region 读取获取文本块拼接好的字符串
        /// <summary>
        /// 读取中文文本
        /// </summary>
        public string CN
        {
            get
            {
                if (!this.IsInit)
                    InitText();
                return this.cn;
            }
            set
            {

                if (this.cn!=null&&value != this.cn)
                {
                    this.cn = value;
                    NotifyPropertyChanged("CN");
                }
            }
        }
        /// <summary>
        /// 读取英文文本
        /// </summary>
        public string EN
        {
            get
            {
                if (!this.IsInit)
                    InitText();
                return this.en==null?null:this.en.ToString();
            }
            set
            {
                //设置NewData同时更新data相关数据
                if (this.en != null && value != this.en.ToString())
                {
                    this.en.Clear();
                    this.en.Append(value);
                    this.endata= Regex.Split(value, Environment.NewLine, RegexOptions.IgnoreCase);
                    NotifyPropertyChanged("EN");
                }
            }
        }
        #endregion
        /// <summary>
        /// 初始化显示用数据
        /// </summary>
        private void InitText()
        {
            if (this.endata != null)
            {
                StringBuilder sb = new StringBuilder();
                //将endata中所有非null值拼接到sb中
                Array.ForEach(this.endata, s => { if (!string.IsNullOrEmpty(s)) sb.AppendLine(s); });
                //去掉最后加上的换行符
                this.en = sb.Remove(sb.Length - (Environment.NewLine).Length, (Environment.NewLine).Length);
            }
            if (this.endata != null)
            {
                StringBuilder sbc = new StringBuilder();
                //将cndata中所有非null值拼接到sb中
                Array.ForEach(this.cndata, s => { if (!string.IsNullOrEmpty(s)) sbc.AppendLine(s); });
                //去掉最后加上的换行符
                this.cn = sbc.Remove(sbc.Length - (Environment.NewLine).Length, (Environment.NewLine).Length).ToString();
            }
            this.IsInit = true;
            NotifyPropertyChanged();
        }
    }
}
