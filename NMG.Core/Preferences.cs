namespace NMG.Core
{
    public class Preferences
    {
        private readonly string prefix;

        public Preferences()
        {
            FieldNamingConvention = FieldNamingConvention.SameAsDatabase;
            prefix = string.Empty;
        }

        public Preferences(FieldNamingConvention fieldNamingConvention, string prefix)
        {
            FieldNamingConvention = fieldNamingConvention;
            this.prefix = prefix;
        }

        public FieldNamingConvention FieldNamingConvention { get; private set; }

        public string Prefix
        {
            get { return prefix; }
        }
    }

    public enum FieldNamingConvention
    {
        SameAsDatabase,
        CamelCase,
        Prefixed
    }
}