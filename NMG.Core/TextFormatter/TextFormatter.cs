using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NMG.Core.TextFormatter
{
    public interface ITextFormatter
    {
        string FormatText(string text);
        string FormatSingular(string text);
        string FormatPlural(string text);
        
        IList<string> PrefixRemovalList { get; set; }
    }

    public abstract class AbstractTextFormatter : ITextFormatter
    {
        public virtual string FormatText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var result = RemovePrefix(text);

            // Cannot have class or property with not allowed chars
            result = result
                .Replace("%", "Porcentaje") //Means Percentage in spanish
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u");

            // Split by capitals to preserve pascal/camelcasing in original text value
            // Preserves TLAs. See http://stackoverflow.com/a/1098039
            result = Regex.Replace(result, "((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", " $1").Trim();

            // Omit any chars except letters and numbers in class or properties.
            result = result.Replace(" ", "_");
            result = Regex.Replace(result, "[^a-zA-Z0-9_]", String.Empty); //And Underscore

            if (result.Length != 0 && char.IsNumber(result.ToCharArray(0, 1)[0]))
            {
                // Cannot start class or property with a number
                result = "_" + result;
            }

            return result;
        }

        public string FormatSingular(string text)
        {
            return FormatText(text).MakeSingular();
        }

        public string FormatPlural(string text)
        {
            return FormatText(text).MakePlural();
        }

        private string RemovePrefix(string original)
        {
            if (PrefixRemovalList == null || PrefixRemovalList.Count == 0 || string.IsNullOrEmpty(original))
                return original;

            // Strip out the first matching prefix
            foreach (var prefix in PrefixRemovalList)
            {
                if (original.ToLower().StartsWith(prefix.ToLower()))
                {
                    return original.Remove(0, prefix.Length);
                }
            }

            return original;
        }

        public IList<string> PrefixRemovalList { get; set; }
    }   

    public class UnformattedTextFormatter : AbstractTextFormatter { }

    public class CamelCaseTextFormatter : AbstractTextFormatter
    {
        public override string FormatText(string text)
        {
            return base.FormatText(text).ToCamelCase();
        }
    }

    public class PascalCaseTextFormatter : AbstractTextFormatter
    {
        public override string FormatText(string text)
        {
            return base.FormatText(text).ToPascalCase();
        }
    }

    public class PrefixedTextFormatter : AbstractTextFormatter
    {
        public PrefixedTextFormatter(string prefix)
        {
            Prefix = prefix;
        }

        private string Prefix { get; set; }

        public override string FormatText(string text)
        {
            return Prefix + base.FormatText(text);
        }
    }

    public static class TextFormatterFactory
    {
        public static ITextFormatter GetTextFormatter(ApplicationPreferences applicationPreferences)
        {
            ITextFormatter formatter;
            switch(applicationPreferences.FieldNamingConvention)
            {
                case FieldNamingConvention.SameAsDatabase:
                    formatter = new UnformattedTextFormatter();
                    break;
                case FieldNamingConvention.CamelCase:
                    formatter = new CamelCaseTextFormatter();
                    break;
                case FieldNamingConvention.PascalCase:
                    formatter = new PascalCaseTextFormatter();
                    break;
                case FieldNamingConvention.Prefixed:
                    formatter = new PrefixedTextFormatter(applicationPreferences.Prefix);
                    break;
                default:
                    throw new Exception("Invalid or unsupported field naming convention.");
            }

            formatter.PrefixRemovalList = applicationPreferences.FieldPrefixRemovalList;

            return formatter;
        }
    }
}