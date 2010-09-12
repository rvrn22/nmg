using System;

namespace NMG.Core.TextFormatter
{
    public interface ITextFormatter
    {
        string FormatText(string text);
        string FormatSingular(string text);
        string FormatPlural(string text);
    }

    public abstract class AbstractTextFormatter : ITextFormatter
    {
        public virtual string FormatText(string text)
        {
            return text;
        }

        public string FormatSingular(string text)
        {
            return FormatText(text).MakeSingular();
        }

        public string FormatPlural(string text)
        {
            return FormatText(text).MakePlural();
        }
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
            return text.ToPascalCase();
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