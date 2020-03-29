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
                    //目标文本的编码格式获取
                    Encoding texttype = FileEncoding.GetType(info.Fdata);

                    StreamReader s=new StreamReader(info.Fdata.OpenRead(),texttype);
                    string start = s.ReadLine();
                    if (start==null||!start.Contains("DMK"))
                    {
                        s.Close();
                        MessageBoxResult result = MessageBox.Show("尝试打开一个非DM提取的文本文件！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    DialoguesData data = this.textmGird.DataContext as DialoguesData;
                    data.InitData(start);
                    data.ReadDialogues(s,TextType.Original);
                    //从翻译后文本文件读取文本
                    //如果不存在翻译后文本文件则直接创建新的文件，同时使用UTF8编码格式
                    if (!FileOperator.FileExist(info.Fdata.DirectoryName, (info.Fdata.Name.Replace(info.Fdata.Extension, "") + Const.FinishName), FileType.all))
                    {
                        s.BaseStream.Seek(0, SeekOrigin.Begin);
                        //创建UTF-8编码文本，最后判断避免创建UTF-8BOM格式
                        File.WriteAllText(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName, s.ReadToEnd(), new UTF8Encoding(false));
                        //info.Fdata.CopyTo(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName);
                    }
                    s.Close();
                    FileInfo f= FileOperator.GetFile(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName);
                    texttype= FileEncoding.GetType(f);
                    //更新历史翻译文本格式
                    if (texttype != Encoding.UTF8)
                    {
                        string tempcontext;
                        using (StreamReader r= new StreamReader(f.OpenRead(), texttype))
                        {
                          tempcontext = r.ReadToEnd();
                        }
                        f.Delete();
                        //创建UTF-8编码文本，最后判断避免创建UTF-8BOM格式
                        File.WriteAllText(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName, tempcontext, new UTF8Encoding(false));
                        f = FileOperator.GetFile(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName);
                    }
                    s = new StreamReader(f.OpenRead(),Encoding.UTF8);
                    data.ReadDialogues(s, TextType.Posttranslation);
                    s.Close();
                    this.SaveBtn.IsEnabled = true;
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
                    this.dunBtn.IsEnabled = true;
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
                    Const.IsChoice = (info.IsSelectBlock < 0) ? false : true;
                }
            }
        }
        /// <summary>
        /// 文本编辑后提交按钮
        /// </summary>
        private void updateBtn_Click(object sender, RoutedEventArgs e)
        {
            UpdateWithNextLine();
        }
        private void SaveCommand(object sender, object e)
        {
            UpdateWithNextLine();
        }
        private void UpdateWithNextLine()
        {
            if (editGrid.DataContext != null)
            {
                this.editBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
                ValidationError a = this.editBox.GetBindingExpression(TextBox.TextProperty).ValidationError;
                //为空说明验证通过
                if (a == null)
                {
                    if (textGird.SelectedIndex >= 0 && textGird.SelectedIndex < textGird.Items.Count - 1)
                    {
                        Const.SelectRowByIndex(textGird, textGird.SelectedIndex + 1);
                        editBox.Focus();
                    }
                }
            }
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
        /// <summary>
        /// 文本保存到文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            DialoguesData data = this.textmGird.DataContext as DialoguesData;
            FileData info = (FileData)this.transGrid.SelectedItem;
            FileInfo f = FileOperator.GetFile(info.Fdata.FullName.Replace(info.Fdata.Extension, "") + Const.FinishName);
            StreamWriter save = null;
            try
            {
                save = new StreamWriter(f.Create(), Encoding.UTF8);
            }
            catch
            {
                MessageBoxResult result = MessageBox.Show($"文件无法覆写！\n" +
                    $"[原因：{f.Name}已经存在但处于只读模式或没有写文件权限]\n" +
                    $"逐级尝试:\n" +
                    $"1.取消{f.Name}的只读模式\n" +
                    $"2.删除{f.Name}文件\n" +
                    $"3.关闭程序并尝试以管理员身份启动[警告：这将导致现有工作进度丢失！]", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (save!=null&&data.SaveDialogues(save))
            {
                MessageBoxResult result = MessageBox.Show($"文件成功写入到{f.FullName}", "通知", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBoxResult result = MessageBox.Show($"文件写入失败！", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void rsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem mi = sender as MenuItem;
            DataBlock rd = mi.DataContext as DataBlock;
            if (rd == null)
                return;
            MessageBoxResult result = MessageBox.Show($"确认对全文进行如下替换：{Environment.NewLine}{rd.CN}--->{rd.EN}", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                int count = (this.textmGird.DataContext as DialoguesData).ReplaceAll(rd.CN, rd.EN);
                MessageBox.Show($"一共替换了{count}处相同文本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void refMenuItem_Click(object sender,RoutedEventArgs e)
        {
            DataBlock rd = this.reftemplate.SelectedItem as DataBlock;
            if (rd == null)
                return;
            MessageBoxResult result = MessageBox.Show($"确认对全文进行如下替换：{Environment.NewLine}{rd.CN}--->{rd.EN}", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                int count = (this.textmGird.DataContext as DialoguesData).ReplaceAll(rd.CN, rd.EN);
                MessageBox.Show($"一共替换了{count}处相同文本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// 保存json文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savejsonBtn_Click(object sender, RoutedEventArgs e)
        {
            DataGrid grid = this.refGrid as DataGrid;
            if (grid.SelectedItem == null)
                return;
            FileData info = (FileData)grid.SelectedItem;
            FileOperator.SaveJson(info.Fdata,this.refDataGrid.DataContext as RefData);
        }

        private void Reftemplate_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.MouseRightButtonDown += (s, a) =>
            {
                a.Handled = true;
                (sender as DataGrid).SelectedIndex = (s as DataGridRow).GetIndex();
                (s as DataGridRow).Focus();
            };
        }

        private void DunBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (this.reftabControl.SelectedIndex)
            {
                case 0:
                    (this.refDataGrid.DataContext as RefData).DelTranSlate(this.refgridCBox.Text, this.refdata.SelectedItem as DataBlock);
                    break;
                case 1:
                    (this.refDataGrid.DataContext as RefData).DelTemplate(this.reftemplate.SelectedItem as DataBlock);
                    break;
                case 2:
                    (this.refDataGrid.DataContext as RefData).DelNotice(this.refNotice.SelectedItem as MyString);
                    break;
                default:
                    break;
            }
        }

        private void SearchData_Executed(object sender, object e)
        {
            this.searchdataBox.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void SearchDataRes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SearchViewData temp =(SearchViewData)(sender as ListBox).SelectedItem;
            if (temp.ShowText==null)
                return;
            int index = temp.WhichLine;
            if (index >= 0 && index < textGird.Items.Count - 1)
            {
                Const.SelectRowByIndex(textGird, index + 1);
                editBox.Focus();
            }
        }
    }
}
