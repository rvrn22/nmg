using System.Globalization;
using System.Threading;

namespace NMG.Core.Util
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

        public static string GetPreferenceFormattedText(this string text, ApplicationPreferences applicationPreferences, bool pluralize)
        {
            if (applicationPreferences.FieldNamingConvention.Equals(FieldNamingConvention.SameAsDatabase))
                return text;
            string formattedText = text.Replace('_', ' ');
            formattedText = formattedText.MakeTitleCase();
            formattedText = formattedText.Replace(" ", "");

            if (applicationPreferences.FieldNamingConvention.Equals(FieldNamingConvention.Prefixed))
                return applicationPreferences.Prefix + formattedText;

            return applicationPreferences.FieldNamingConvention.Equals(FieldNamingConvention.CamelCase)
                       ? formattedText.MakeFirstCharLowerCase()
                       : formattedText;
        }

        public static string GetPreferenceFormattedText(this string text, ApplicationPreferences applicationPreferences)
        {
            return GetPreferenceFormattedText(text, applicationPreferences, false);
        }

        public static string ReplaceAt(this string text, int index, char charToUse)
        {
            var buffer = text.ToCharArray();
            buffer[index] = charToUse;
            return new string(buffer);
        }

        public static string MakeFirstCharLowerCase(this string text)
        {
            char lower = char.ToLower(text[0]);
            text = text.Remove(0, 1);
            text = lower + text;
            return text;
        }

        public static string MakeFirstCharUpperCase(this string text)
        {
            char upper = char.ToUpper(text[0]);
            text = text.Remove(0, 1);
            text = upper + text;
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

        /// <summary>
        /// Wrap word in double quotes, then backticks (`)
        /// for NHibernate to interpret
        /// without backticks
        ///
        /// This is for table / column / object
        /// names that contain spaces or that use
        /// db-specific keywords
        /// so NHibernate will behave correctly
        ///
        /// Reference:
        /// https://groups.google.com/g/nhusers/c/-46QXkkXVV0
        /// https://sdesmedt.wordpress.com/2006/09/04/nhibernate-part-4-mapping-techniques-for-aggregation-one-to-many-mapping/
        /// and from Hibernate:
        /// https://stackoverflow.com/questions/50783644/add-backticks-to-column-names-in-hibernate
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToStringLiteral(this string input)
        {
            if (input == null) return null;
            return string.Format("\"`{0}`\"", input);
        }
    }
}