using System;

namespace NMG.Core.TextFormatter
{
    public interface ITextFormatter
    {
        string FormatText(string text);
    }

    public class UnformattedTextFormatter : ITextFormatter
    {
        public string FormatText(string text)
        {
            return text;
        }
    }

    public class CamelCaseTextFormatter : ITextFormatter
    {
        public string FormatText(string text)
        {
            return text.ToCamelCase();
        }
    }

    public class PascalCaseTextFormatter : ITextFormatter
    {
        public string FormatText(string text)
        {
            return text.ToPascalCase();
        }
    }

    public class PrefixedTextFormatter : ITextFormatter
    {
        public PrefixedTextFormatter(string prefix)
        {
            Prefix = prefix;
        }

        private string Prefix { get; set; }

        public string FormatText(string text)
        {
            return Prefix + text;
        }
    }

    public static class TextFormatterFactory
    {
        public static ITextFormatter GetTextFormatter(ApplicationPreferences applicationPreferences)
        {
            switch(applicationPreferences.FieldNamingConvention)
            {
                case FieldNamingConvention.SameAsDatabase:
                    return new UnformattedTextFormatter();
                case FieldNamingConvention.CamelCase:
                    return new CamelCaseTextFormatter();
                case FieldNamingConvention.PascalCase:
                    return new PascalCaseTextFormatter();
                case FieldNamingConvention.Prefixed:
                    return new PrefixedTextFormatter(applicationPreferences.Prefix);
                default:
                    throw new Exception("Invalid or unsupported field naming convention.");
            }
        }
    }
}