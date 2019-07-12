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
        public struct FileData
        {
            public string data { get; set; }
            public FileInfo Fdata { get; set; }
        }
        DataGrid selectgrid;
        ObservableCollection<FileData> translist = new ObservableCollection<FileData>();
        ObservableCollection<FileData> reflist = new ObservableCollection<FileData>();

        public MainWindow()
        {
            InitializeComponent();
            transGrid.ItemsSource = translist;
            refGrid.ItemsSource = reflist;
            this.addBtn.IsEnabled = false;
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
                return;
            }
            foreach (string t in temp)
            {
                if (t.Split('\\').Last().ToLower().Equals("data"))
                {
                    List<FileInfo> file = FileOperator.GetFiles(t, FileType.txt);
                    FileData dt = new FileData();
                    foreach (FileInfo fi in file)
                    {
                        dt.data = fi.Name.Replace(fi.Extension, "");
                        dt.Fdata = fi;
                        translist.Add(dt);
                    }
                }
                else if (t.Split('\\').Last().ToLower().Equals("reference"))
                {
                    List<FileInfo> file = FileOperator.GetFiles(t, FileType.json);
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
                        s.Close();
                        MessageBoxResult result = MessageBox.Show("尝试打开一个非DM提取的文本文件！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    DialoguesData data = new DialoguesData(start);
                    this.textmGird.DataContext = data;
                    data.ReadDialogues(s,TextType.Original);
                    s.Close();
                    //从翻译后文本文件读取文本
                    if (!FileOperator.FileExist(info.Fdata.DirectoryName, (info.Fdata.Name.Replace(info.Fdata.Extension, "") + Const.FinishName), FileType.all))
                    {
                        info.Fdata.CopyTo(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName);
                    }
                    FileInfo f= FileOperator.GetFile(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName);
                    s = new StreamReader(f.Open(FileMode.Open, FileAccess.Read));
                    data.ReadDialogues(s, TextType.Posttranslation);
                    s.Close();
                }
            }
        }
        /// <summary>
        /// 双击选择要读取的参考JSON
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                DataGrid grid = sender as DataGrid;
                if (grid != null && grid.SelectedItems != null && grid.SelectedItems.Count == 1)
                {
                    FileData info = (FileData)grid.SelectedItem;
                    RefData rd=new RefData();
                    this.refDataGrid.DataContext = rd;
                    if (!FileOperator.ReadJson(info.Fdata, ref rd))
                    {
                        MessageBoxResult result = MessageBox.Show("参考文本读取失败！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    this.refgridCBox.SelectedIndex = 0;
                    this.addBtn.IsEnabled = true;
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
        /// 文本编辑后提交按钮
        /// </summary>
        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (editGrid.DataContext != null)
            {
                this.editBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                //(editGrid.DataContext as ViewData).NewData = editBox.Text;
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
            DataGridCellInfo cell = (sender as DataGrid).CurrentCell;
            //剪切板添加选择的数据[GetValue在UpdateData中已实现]
            Clipboard.SetText((cell.Item as UpdateData).GetValue(cell.Column.SortMemberPath).ToString());
            string temp = e.Parameter.ToString();
            if (temp == null) temp = "Undefined";
            messageShow.Content = e.Parameter.ToString();
            Storyboard storyboard = Resources["labelAnimation"] as Storyboard;
            storyboard.Begin(messageShow);
            e.Handled = false;
        }
        /// <summary>
        /// 参考文本选中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefgridCBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //将当前选择项目的文本列表引用给refdata
            this.refdata.ItemsSource = ((sender as ComboBox).SelectedValue as ObservableCollection<DataBlock>);
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            if (this.refDataGrid.DataContext == null)
                return;
            switch (this.reftabControl.SelectedIndex)
            {
                case 0:
                    (this.refDataGrid.DataContext as RefData).AddTranslate(this.refgridCBox.Text, new DataBlock("", ""));
                    break;
                case 1:
                    (this.refDataGrid.DataContext as RefData).AddTemplate(new DataBlock("", ""));
                    break;
                case 2:
                    (this.refDataGrid.DataContext as RefData).AddNotice(new MyString(""));
                    break;
                default:
                    break;
            }
        }

    }
}
