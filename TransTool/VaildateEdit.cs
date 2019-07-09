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
        Regex CNMarkRule = new Regex("[\u0391-\uFFE5]+");
        StringBuilder sberror = new StringBuilder();
        bool FindError = false;
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string[] checkstr=Regex.Split(value as string, Environment.NewLine, RegexOptions.IgnoreCase);
            sberror.Clear();
            FindError = false;
            int count = 0;
            if (checkstr.Length > Const.MaxLineNum)
            {
                FindError = true;
                sberror.AppendLine($"行数量不能超过{Const.MaxLineNum}(行{checkstr.Length})");
            }
            foreach(string cs in checkstr)
            {
                count++;
                if (cs.Length > Const.MaxLineLength)
                {
                    FindError = true;
                    sberror.AppendLine($"单行长度不能超过{Const.MaxLineLength}(行{count},长度{cs.Length})");
                }
                int matchcount = CNMarkRule.Matches(cs).Count;
                if (matchcount > 0)
                {
                    FindError = true;
                    sberror.AppendLine($"在行{count}中出现了{matchcount}个全角字符");
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
