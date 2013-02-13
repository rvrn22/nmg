using System;
using System.Collections.Generic;
using System.Linq;

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
            return RemovePrefix(text);
        }

        public string FormatSingular(string text)
        {
            return FormatText(RemovePrefix(text)).MakeSingular();
        }

        public string FormatPlural(string text)
        {
            return FormatText(RemovePrefix(text)).MakePlural();
        }

        protected string RemovePrefix(string original)
        {
            if (PrefixRemovalList == null || PrefixRemovalList.Count == 0)
                return original;

            // Strip out the first matching prefix
            foreach (var prefix in PrefixRemovalList)
            {
                if (original.StartsWith(prefix))
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
            return text.ToCamelCase();
        }
    }

    public class PascalCaseTextFormatter : AbstractTextFormatter
    {
        public override string FormatText(string text)
        {
            return RemovePrefix(text).ToPascalCase();
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
            return Prefix + RemovePrefix(text);
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