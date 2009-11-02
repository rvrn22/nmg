namespace NMG.Core
{
    public class Preferences
    {
        public Preferences()
        {
            FieldNamingConvention = FieldNamingConvention.SameAsDatabase;
            Prefix = string.Empty;
        }

        public Preferences(FieldNamingConvention fieldNamingConvention, string prefix)
        {
            FieldNamingConvention = fieldNamingConvention;
            Prefix = prefix;
        }

        public FieldNamingConvention FieldNamingConvention { get; private set; }
        public string Prefix { get; private set; }

    }
}