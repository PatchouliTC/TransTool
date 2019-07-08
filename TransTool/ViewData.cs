using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TransTool
{
    public class ViewData: INotifyPropertyChanged
    {
        private int MaxLine = 4;//RMXP最大行数
        private string[] data;
        public ViewData(int ml=4)
        {
            MaxLine = ml;
            //初始化数组
            data = new string[MaxLine];
        }
        public string ViewData
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                Array.ForEach(this.data,s=>sb.Append(s));
                return sb.ToString();
            }
            set
            {
                if (value != this.data)
                {
                    this.data = value;
                    NotifyPropertyChanged("Data");
                }
            }
        }
        /// <summary>
        /// 数据变更的消息通知事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 数据变更的消息通知调用
        /// </summary>
        /// <param name="propertyName">变更的属性名称</param>
        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
