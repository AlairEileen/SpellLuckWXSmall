using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tools.Strings
{
    public class StringTool
    {
        //public static string[][] GetCNPinyinSimple(string text)
        //{
        //    char[] chars = text.ToArray();
        //    string[][] stringArray = new string[chars.Length][];
        //    for (int i = 0; i < chars.Length; i++)
        //    {

        //        stringArray[i] = Pinyin4net.PinyinHelper.ToHanyuPinyinStringArray(chars[i], new Pinyin4net.Format.HanyuPinyinOutputFormat() { CaseType = Pinyin4net.Format.HanyuPinyinCaseType.UPPERCASE, ToneType = Pinyin4net.Format.HanyuPinyinToneType.WITHOUT_TONE });

        //    }
        //    return stringArray;
        //}

        //public static string IsLetter(string text,out bool isLetter)
        //{
        //    byte[] ZW = System.Text.Encoding.Default.GetBytes(text);
        //    //如果是字母，则直接返回
        //    if (ZW.Length == 1)
        //    {
        //        isLetter = true;
        //        return text.ToUpper();
        //    }
        //    else
        //    {
        //        isLetter = false;
        //    }
        //    return null;
        //}
    }
}
