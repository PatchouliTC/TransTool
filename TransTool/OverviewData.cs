using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TransTool
{
    /// <summary>
    /// 用于DataSource的自定义属性类
    /// </summary>
    public class OverviewData:INotifyPropertyChanged
    {
        private string olddata = null;
        private string newdata = null;
        public event PropertyChangedEventHandler PropertyChanged;
        public OverviewData() { }
        public OverviewData(string o,string n)
        {
            this.olddata = o;
            this.newdata = n;
        }
        public string OldData
        {
            get
            {
                return this.olddata;
            }
            set
            {
                if (value != this.olddata)
                {
                    this.olddata = value;
                    NotifyPropertyChanged("olddata");
                }
            }
        }
        public string NewData
        {
            get
            {
                return this.newdata;
            }
            set
            {
                if(value!= this.newdata)
                {
                    this.newdata = value;
                    NotifyPropertyChanged("newdata");
                }
            }
        }

        private void NotifyPropertyChanged(String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}
