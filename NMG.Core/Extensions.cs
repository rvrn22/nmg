using System.Globalization;
using System.Threading;

namespace NMG.Core
{
    public static class Extensions
    {
        public static string GetFormattedText(this string text)
        {
            string formattedText = text.Replace('_', ' ');
            formattedText = formattedText.MakeTitleCase();
            formattedText = formattedText.Replace(" ", "");
            return formattedText;
        }

        public static string MakeFirstCharLowerCase(this string text)
        {
            char lower = char.ToLower(text[0]);
            text = text.Remove(0, 1);
            text = lower + text;
            return text;
        }
        
        public static string MakeTitleCase(this string text)
        {
            text = text.Trim();
            text = text.ToLower();
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;
            return textInfo.ToTitleCase(text);
        }
    }
}