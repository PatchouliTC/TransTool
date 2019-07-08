using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections;

namespace TransTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public class TextData
        {
            public string data { get; set; }
            public string newdata { get; set; }
        }
        public struct FileData
        {
            public string data { get; set; }
            public FileInfo Fdata { get; set; }
        }

        const int MaxLineCount = 4;
        DataGrid selectgrid;
        ObservableCollection<ViewData> textlist = new ObservableCollection<ViewData>();
        ObservableCollection<FileData> translist = new ObservableCollection<FileData>();
        ObservableCollection<FileData> reflist = new ObservableCollection<FileData>();

        public MainWindow()
        {
            InitializeComponent();
            textGird.AutoGenerateColumns = false;
            transGrid.AutoGenerateColumns = false;
            refGrid.AutoGenerateColumns = false;
            textGird.ItemsSource = textlist;
            transGrid.ItemsSource = translist;
            refGrid.ItemsSource = reflist;
        }
        /// <summary>
        /// 加载文件按钮
        /// </summary>
        private void loadBtn_Click(object sender, RoutedEventArgs e)
        {
            translist.Clear();
            reflist.Clear();
            string[] rule = new string[] {"data","reference"};
            string[] temp = FileOperator.DimExist(Environment.CurrentDirectory, rule);
            if (temp == null||temp.Length<2)
            {
                MessageBoxResult result = MessageBox.Show("当前程序目录下未找到data和reference目录！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            foreach (string t in temp)
            {
                if (t.Split('\\').Last().ToLower().Equals("data"))
                {
                    List<FileInfo> file = FileOperator.GetFile(temp[0], FileType.txt);
                    FileData dt = new FileData();
                    foreach (FileInfo fi in file)
                    {
                        dt.data = fi.Name.Replace(fi.Extension, "");
                        dt.Fdata = fi;
                        translist.Add(dt);
                    }
                }
                else
                {
                    List<FileInfo> file = FileOperator.GetFile(temp[1], FileType.json);
                    FileData dt = new FileData();
                    foreach (FileInfo fi in file)
                    {
                        dt.data = fi.Name.Replace(fi.Extension, "");
                        dt.Fdata = fi;
                        reflist.Add(dt);
                    }
                }
            }
        }
        /// <summary>
        /// 双击选择要读取的翻译文本
        /// </summary>
        private void TransGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    FileData info = (FileData)grid.SelectedItem;
                    StreamReader s=new StreamReader(info.Fdata.Open(FileMode.Open, FileAccess.Read));
                    string start = s.ReadLine();
                    if (start==null||!start.Contains("DMK"))
                    {
                        MessageBoxResult result = MessageBox.Show("尝试打开一个非DM提取的文本文件！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    Regex reg = new Regex("\\-{3,}");
                    Regex reg2 = new Regex("\\+{5,}");
                    
                    string readtmp="";
                    while(!s.EndOfStream)
                    {
                        if (!reg.IsMatch(readtmp))
                            readtmp = s.ReadLine();
                        if (reg.IsMatch(readtmp))
                        {

                            List<string> al = new List<string>();
                            readtmp = s.ReadLine();
                            while (!reg.IsMatch(readtmp))
                            {
                                if (!reg2.IsMatch(readtmp))//+++忽略
                                {
                                    al.Add(readtmp);
                                }
                                readtmp = s.ReadLine();
                                if (readtmp == null)
                                    break;
                            }
                            if (readtmp != null)
                            {
                                ViewData dt = new ViewData();
                                dt.Data = al.ToArray();
                                textlist.Add(dt);//此时readtmp里面是下一个虚线分隔符
                            }
                        }
                    }
                    s.Close();
                }
            }
        }
        /// <summary>
        /// 文本显示区焦点切换行时更新下方原文编辑区域文本
        /// </summary>
        private void TextGird_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null)
            {
                selectgrid = sender as DataGrid;
                if (selectgrid != null && selectgrid.SelectedItems != null && selectgrid.SelectedItems.Count == 1)
                {
                    //当前选择的Text条目
                    ViewData info = selectgrid.SelectedItem as ViewData;
                    if (info == null) return;
                    editGrid.DataContext = info;
                }
            }
        }
        /// <summary>
        /// 编辑区行数限制
        /// </summary>
        private void EditBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            int textLineCount = textBox.LineCount;
            if (textLineCount > MaxLineCount)
            {
                StringBuilder text = new StringBuilder();
                for (int i = textLineCount-1; i < MaxLineCount; i++)
                {
                    string tmp = textBox.GetLineText(i);
                    text.Append(tmp);
                }
                textBox.Text = text.ToString();
                textBox.SelectionStart = textBox.Text.Length;
            }
        }
        /// <summary>
        /// 文本编辑后提交按钮
        /// </summary>
        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (editGrid.DataContext != null)
            {
                (DataContext as ViewData).NewData = editBox.Text;
            }
            //if (selectgrid != null && selectgrid.SelectedItems != null && selectgrid.SelectedItems.Count == 1)
            //{
            //    (selectgrid.SelectedItem as OverviewData).NewData = editBox.Text;
                
            //    //TextData dt = new TextData();
            //    //dt.data = showBox.Text;
            //    //dt.newdata = editBox.Text;
            //    //textlist.Insert(selectgrid.SelectedIndex, dt);
            //    //textlist.RemoveAt(selectgrid.SelectedIndex);
            //}
        }
        /// <summary>
        /// Handle CTRL + C callback
        /// </summary>
        private void CopyCommand(object sender, ExecutedRoutedEventArgs e)
        {

            string temp = e.Parameter.ToString();
            if (temp == null) temp = "Undefined";
            messageShow.Content = e.Parameter.ToString();
            Storyboard storyboard = Resources["labelAnimation"] as Storyboard;
            storyboard.Begin(messageShow);
            e.Handled = true;
        }
    }
}
