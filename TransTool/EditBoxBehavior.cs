using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace TransTool
{
    public class TextBoxInputRegExBehaviour : Behavior<TextBox>
    {
        #region DependencyProperties

        public static readonly DependencyProperty MaxLineProperty =
            DependencyProperty.Register(
                nameof(MaxLine),
                typeof(int),
                typeof(TextBoxInputRegExBehaviour),
                new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register(
                nameof(MaxLength),
                typeof(int),
                typeof(TextBoxInputRegExBehaviour),
                new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty EmptyValueProperty =
            DependencyProperty.Register("EmptyValue", typeof(string), typeof(TextBoxInputRegExBehaviour), null);
        /// <summary>
        /// 最大行数
        /// </summary>
        public int MaxLine
        {
            get { return (int)GetValue(MaxLineProperty); }
            set
            {
                SetValue(MaxLineProperty, value);
            }
        }
        /// <summary>
        /// 单行最长长度[不包括换行符]
        /// </summary>
        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set
            {
                SetValue(MaxLengthProperty, value );
            }
        }
        public string EmptyValue
        {
            get { return (string)GetValue(EmptyValueProperty); }
            set { SetValue(EmptyValueProperty, value); }
        }
        #endregion

        private int MaxAllLength=0;
        private int[] LineLength;

        /// <summary>
        ///     Attach our behaviour. Add event handlers
        /// </summary>
        protected override void OnAttached()
        {
            base.OnAttached();
            MaxAllLength = this.MaxLine * this.MaxLength+Environment.NewLine.Length*(this.MaxLine);//整个文本框理论最大值【包括了换行符
            AssociatedObject.PreviewTextInput += PreviewTextInputHandler;
            AssociatedObject.PreviewKeyDown += PreviewKeyDownHandler;
            DataObject.AddPastingHandler(AssociatedObject, PastingHandler);
            LineLength = new int[MaxLine+1];//初始化最大行数
        }

        /// <summary>
        ///     Deattach our behaviour. remove event handlers
        /// </summary>
        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.PreviewTextInput -= PreviewTextInputHandler;
            AssociatedObject.PreviewKeyDown -= PreviewKeyDownHandler;
            DataObject.RemovePastingHandler(AssociatedObject, PastingHandler);
        }

        #region Event handlers [PRIVATE] --------------------------------------

        void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
        {
            //当前输入字符的位置[光标前一个字符的位置，从0开始计数]
            int indexinline = AssociatedObject.CaretIndex;
            //当前光标所在行
            int focusline=AssociatedObject.GetLineIndexFromCharacterIndex(indexinline);
            //即将输入字符长度
            int inputlength = e.Text.Length;
            //当前行该光标前一个字符相对于行首的长度
            indexinline -= AssociatedObject.GetCharacterIndexFromLineIndex(focusline);
            //光标前面的字符长度
            int frontlength=AssociatedObject.Text.Length-1- AssociatedObject.GetCharacterIndexFromLineIndex(focusline)-indexinline;
            //如果当前行长度加上即将输入字符长度小于限制的单行最大长度，不处理
            if (indexinline + inputlength <= MaxLength)
            {
                e.Handled = false;
                return;
            }
            else
            {
                //如果大于 说明尝试输入的时候已经到达了该行限制的最大长度
                //最佳情况：光标位于行末尾
                //如果光标在当前行末尾[行末尾-1。每往前一个字符+1]
                if (frontlength < 0)
                {
                    //如果当前行是最大行[focusline从0开始]，直接停止输入
                    if (focusline==MaxLine-1)
                    {
                        e.Handled = true;
                        return;
                    }
                    AssociatedObject.Text = AssociatedObject.Text.Insert(AssociatedObject.CaretIndex, Environment.NewLine);
                    AssociatedObject.CaretIndex = AssociatedObject.Text.Length;
                }
                //坑爹情况：光标不在末尾而位于文本中任一位置
                else
                {
                    //TODO:
                }
                e.Handled = false;
                return;
            }
            


            if (AssociatedObject.LineCount < this.MaxLine)
            {
                //如果当前行数比设定最大行数小，最多进行换行处理

                e.Handled = false ;
                return;
            }
            //获取当前光标前面换行符出现次数
            int count = Regex.Matches(AssociatedObject.Text.Substring(0, AssociatedObject.CaretIndex - 1), Environment.NewLine).Count;









            string text;
            int c = AssociatedObject.CaretIndex;
            AssociatedObject.Text = e.Text + "1";
            if (AssociatedObject.LineCount >= 4)
            {
                AssociatedObject.Text.Insert(this.AssociatedObject.CaretIndex, e.Text);

                //AssociatedObject.Text.
            }
            if (this.AssociatedObject.Text.Length < this.AssociatedObject.CaretIndex)
                text = this.AssociatedObject.Text;
            else
            {
                //  Remaining text after removing selected text.
                string remainingTextAfterRemoveSelection;

                text = TreatSelectedText(out remainingTextAfterRemoveSelection)
                    ? remainingTextAfterRemoveSelection.Insert(AssociatedObject.SelectionStart, e.Text)
                    : AssociatedObject.Text.Insert(this.AssociatedObject.CaretIndex, e.Text);
            }

            e.Handled = !ValidateText(text);
        }

        /// <summary>
        ///     PreviewKeyDown event handler
        /// </summary>
        void PreviewKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (string.IsNullOrEmpty(this.EmptyValue))
                return;

            string text = null;

            // Handle the Backspace key
            if (e.Key == Key.Back)
            {
                if (!this.TreatSelectedText(out text))
                {
                    if (AssociatedObject.SelectionStart > 0)
                        text = this.AssociatedObject.Text.Remove(AssociatedObject.SelectionStart - 1, 1);
                }
            }
            // Handle the Delete key
            else if (e.Key == Key.Delete)
            {
                // If text was selected, delete it
                if (!this.TreatSelectedText(out text) && this.AssociatedObject.Text.Length > AssociatedObject.SelectionStart)
                {
                    // Otherwise delete next symbol
                    text = this.AssociatedObject.Text.Remove(AssociatedObject.SelectionStart, 1);
                }
            }

            if (text == string.Empty)
            {
                this.AssociatedObject.Text = this.EmptyValue;
                if (e.Key == Key.Back)
                    AssociatedObject.SelectionStart++;
                e.Handled = true;
            }
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                string text = Convert.ToString(e.DataObject.GetData(DataFormats.Text));

                if (!ValidateText(text))
                    e.CancelCommand();
            }
            else
                e.CancelCommand();
        }
        #endregion Event handlers [PRIVATE] -----------------------------------

        #region Auxiliary methods [PRIVATE] -----------------------------------

        /// <summary>
        ///     Validate certain text by our regular expression and text length conditions
        /// </summary>
        /// <param name="text"> Text for validation </param>
        /// <returns> True - valid, False - invalid </returns>
        private bool ValidateText(string text)
        {
            return true;
        }

        /// <summary>
        ///     Handle text selection
        /// </summary>
        /// <returns>true if the character was successfully removed; otherwise, false. </returns>
        private bool TreatSelectedText(out string text)
        {
            text = null;
            if (AssociatedObject.SelectionLength <= 0)
                return false;

            var length = this.AssociatedObject.Text.Length;
            if (AssociatedObject.SelectionStart >= length)
                return true;

            if (AssociatedObject.SelectionStart + AssociatedObject.SelectionLength >= length)
                AssociatedObject.SelectionLength = length - AssociatedObject.SelectionStart;

            text = this.AssociatedObject.Text.Remove(AssociatedObject.SelectionStart, AssociatedObject.SelectionLength);
            return true;
        }
        #endregion Auxiliary methods [PRIVATE] --------------------------------
    }
}
