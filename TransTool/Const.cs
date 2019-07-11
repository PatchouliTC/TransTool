using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TransTool
{
    class Const
    {
        public static int MaxLineLength = 40;
        public static int MaxLineNum = 4;
        public static Regex Chinese = new Regex("[\u4e00-\u9fbb]+$");
        public static Regex Japanese = new Regex("[\u0800-\u4e00]+$");

        public static Regex TextBlock = new Regex("\\-{3,}");
        public static Regex ChoiceBlock = new Regex("\\+{3,}");
        public static Regex LocationBlock = new Regex("\\#{3,}");

        public static Regex LocationMap=new Regex("(?<=(?:\\:\\s))[0-9]+");
        public static Regex LocationEvent = new Regex("(?<=(?:T\\s))[0-9]+");
        public static Regex LocationPage = new Regex("(?<=(?:E\\s))[0-9]+");

        private const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        private const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        private const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);

        /// <summary>
        /// 将字符转换成简体中文
        /// </summary>
        /// <param name="source">输入要转换的字符串</param>
        /// <returns>转换完成后的字符串</returns>
        public static string ToSimplified(string source)
        {
            String target = new String(' ', source.Length);
            int ret = LCMapString(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, source, source.Length, target, source.Length);
            return target;
        }
    }
}
