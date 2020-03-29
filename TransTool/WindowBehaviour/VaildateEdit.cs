using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TransTool
{
    public class ValidationEdit:ValidationRule
    {

        StringBuilder sberror = new StringBuilder();
        bool FindError = false;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            sberror.Clear();
            FindError = false;
            if (value == null)
            {
                FindError = true;
                sberror.AppendLine($"文本块不能为空！");
            }
            else
            {
                string valueregular = (Regex.Replace((value as string), "\n", "\r\n", RegexOptions.IgnoreCase)).Replace("\n\n", "\n");
                string[] checkstr = Regex.Split(value as string, Environment.NewLine, RegexOptions.IgnoreCase);
                int count = 0;
                if (checkstr.Length > Const.MaxLineNum)
                {
                    FindError = true;
                    sberror.AppendLine($"行数量不能超过{Const.MaxLineNum}(行{checkstr.Length})");
                }
                foreach (string cs in checkstr)
                {
                    count++;
                    if (cs.Length > Const.MaxLineLength)
                    {
                        FindError = true;
                        sberror.AppendLine($"单行长度不能超过{Const.MaxLineLength}(行{count},长度{cs.Length})");
                    }
                    int matchcount = Const.CNMarkRule.Matches(cs).Count;
                    if (matchcount > 0)
                    {
                        FindError = true;
                        sberror.AppendLine($"在行{count}中出现了{matchcount}个全角字符");
                    }

                }
            }
            if (FindError)
            {
                return new ValidationResult(false, sberror.ToString());
            }
            return new ValidationResult(true,"");
        }
    }
}
