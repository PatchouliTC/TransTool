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
        private int maxline = 4;//RMXP最大行数
        private string[] data;//每个数据块的数组
        private string olddata;//用于显示原文文本
        private StringBuilder newdata;//用于显示翻译后文本
        public bool IsInit { get; private set; }//该数据块是否已经被初始化
        public ViewData() { data = new string[maxline]; }
        public ViewData(int ml)
        {
            maxline = ml;
            //初始化数组
            data = new string[maxline];
        }
        /// <summary>
        /// Data的set方法仅用于初始化！
        /// </summary>
        public string[] Data
        {
            get
            {
                return this.data;
            }
            set
            {
                //参数行数小于等于最大行
                if (value.Length <= maxline)
                {
                    for (int i = 0; i < maxline; i++)
                        //以参数行数为准，赋值给data
                        if (i + 1 <= value.Length)
                            data[i] = value[i];
                    //超过参数行数没有到达最大行数，data设置为null
                        else
                            data[i] = null;
                }
                else
                {
                    for (int i = 0; i < maxline; i++)
                        data[i] = value[i];
                }
                InitShowText();
            }
        }

        public string OldData
        {
            get { return this.olddata; }
            set
            {
                if (this.IsInit) return;
                if (value != this.olddata)
                {
                    this.olddata = value;
                    NotifyPropertyChanged("OldData");
                }
            }
        }

        public string NewData
        {
            get { return (this.newdata!=null)?newdata.ToString():null; }
            set
            {
                //设置NewData同时更新data相关数据
                if (value != this.newdata.ToString())
                {
                    this.newdata.Clear();
                    this.newdata.Append(value);
                    string[] temp = Regex.Split(value,Environment.NewLine,RegexOptions.IgnoreCase);
                    for(int i = 0; i < maxline; i++)
                    {
                        if (i + 1 > temp.Length)
                        {
                            data[i] = null;
                            continue;
                        }
                        data[i] = temp[i];
                    }
                    NotifyPropertyChanged("NewData");
                }
            }
        }
        /// <summary>
        /// 初始化显示用数据
        /// </summary>
        private void InitShowText()
        {
            StringBuilder sb = new StringBuilder();
            //将data中所有非null值拼接到sb中
            Array.ForEach(this.data, s => { if (!string.IsNullOrEmpty(s)) sb.AppendLine(s); });
            newdata = sb.Remove(sb.Length - (Environment.NewLine).Length, (Environment.NewLine).Length);
            olddata = newdata.ToString();
            NotifyPropertyChanged();
            this.IsInit = true;
        }
    }
}
