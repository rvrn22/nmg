//
//   SubSonic - http://subsonicproject.com
//
//   The contents of this file are subject to the New BSD
//   License (the "License"); you may not use this file
//   except in compliance with the License. You may obtain a copy of
//   the License at http://www.opensource.org/licenses/bsd-license.php
//
//   Software distributed under the License is distributed on an
//   "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
//   implied. See the License for the specific language governing
//   rights and limitations under the License.
//
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace NMG.Core.TextFormatter
{
    /// <summary>
    /// Summary for the Inflector class
    /// </summary>
    public static class Inflector
    {
        private static readonly List<InflectorRule> _plurals = new List<InflectorRule>();
        private static readonly List<InflectorRule> _singulars = new List<InflectorRule>();
        private static readonly List<string> _uncountables = new List<string>();

        private static bool _enableInflection;
        public static bool EnableInflection
        {
            get
            {
                return _enableInflection;
            }
            set
            {
                _enableInflection = value;
                if (value)
                    SetRules();
            }
        }

        /// <summary>
        /// Initializes the <see cref="Inflector"/> class.
        /// </summary>
        static Inflector()
        {
            if (!EnableInflection) return;
            SetRules();
        }

        private static void SetRules()
        {
            lock (_plurals)
            {
                lock (_singulars)
                {
                    _plurals.Clear();
                    _singulars.Clear();
                    _uncountables.Clear();
                    AddPluralRule("$", "s");
                    AddPluralRule("s$", "s");
                    AddPluralRule("(ax|test)is$", "$1es");
                    AddPluralRule("(octop|vir)us$", "$1i");
                    AddPluralRule("(alias|status)$", "$1es");
                    AddPluralRule("(bu)s$", "$1ses");
                    AddPluralRule("(buffal|tomat)o$", "$1oes");
                    AddPluralRule("([ti])um$", "$1a");
                    AddPluralRule("sis$", "ses");
                    AddPluralRule("(?:([^f])fe|([lr])f)$", "$1$2ves");
                    AddPluralRule("(hive)$", "$1s");
                    AddPluralRule("([^aeiouy]|qu)y$", "$1ies");
                    AddPluralRule("(x|ch|ss|sh)$", "$1es");
                    AddPluralRule("(matr|vert|ind)ix|ex$", "$1ices");
                    AddPluralRule("([m|l])ouse$", "$1ice");
                    AddPluralRule("^(ox)$", "$1en");
                    AddPluralRule("(quiz)$", "$1zes");

                    AddSingularRule("s$", String.Empty);
                    AddSingularRule("ss$", "ss");
                    AddSingularRule("(n)ews$", "$1ews");
                    AddSingularRule("([ti])a$", "$1um");
                    AddSingularRule("((a)naly|(b)a|(d)iagno|(p)arenthe|(p)rogno|(s)ynop|(t)he)ses$", "$1$2sis");
                    AddSingularRule("(^analy)ses$", "$1sis");
                    AddSingularRule("([^f])ves$", "$1fe");
                    AddSingularRule("(hive)s$", "$1");
                    AddSingularRule("(tive)s$", "$1");
                    AddSingularRule("([lr])ves$", "$1f");
                    AddSingularRule("([^aeiouy]|qu)ies$", "$1y");
                    AddSingularRule("(s)eries$", "$1eries");
                    AddSingularRule("(m)ovies$", "$1ovie");
                    AddSingularRule("(x|ch|ss|sh)es$", "$1");
                    AddSingularRule("([m|l])ice$", "$1ouse");
                    AddSingularRule("(bus)es$", "$1");
                    AddSingularRule("(o)es$", "$1");
                    AddSingularRule("(shoe)s$", "$1");
                    AddSingularRule("(cris|ax|test)es$", "$1is");
                    AddSingularRule("(octop|vir)i$", "$1us");
                    AddSingularRule("(alias|status)$", "$1");
                    AddSingularRule("(alias|status)es$", "$1");
                    AddSingularRule("^(ox)en", "$1");
                    AddSingularRule("(vert|ind)ices$", "$1ex");
                    AddSingularRule("(matr)ices$", "$1ix");
                    AddSingularRule("(quiz)zes$", "$1");

                    AddIrregularRule("person", "people");
                    AddIrregularRule("man", "men");
                    AddIrregularRule("child", "children");
                    AddIrregularRule("sex", "sexes");
                    AddIrregularRule("tax", "taxes");
                    AddIrregularRule("move", "moves");

                    AddUnknownCountRule("equipment");
                    AddUnknownCountRule("information");
                    AddUnknownCountRule("rice");
                    AddUnknownCountRule("money");
                    AddUnknownCountRule("species");
                    AddUnknownCountRule("series");
                    AddUnknownCountRule("fish");
                    AddUnknownCountRule("sheep");
                }
            }
        }

        /// <summary>
        /// Adds the irregular rule.
        /// </summary>
        /// <param name="singular">The singular.</param>
        /// <param name="plural">The plural.</param>
        private static void AddIrregularRule(string singular, string plural)
        {
            AddPluralRule(String.Concat("(", singular[0], ")", singular.Substring(1), "$"),
                          String.Concat("$1", plural.Substring(1)));
            AddSingularRule(String.Concat("(", plural[0], ")", plural.Substring(1), "$"),
                            String.Concat("$1", singular.Substring(1)));
        }

        /// <summary>
        /// Adds the unknown count rule.
        /// </summary>
        /// <param name="word">The word.</param>
        private static void AddUnknownCountRule(string word)
        {
            _uncountables.Add(word.ToLower());
        }

        /// <summary>
        /// Adds the plural rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="replacement">The replacement.</param>
        private static void AddPluralRule(string rule, string replacement)
        {
            _plurals.Add(new InflectorRule(rule, replacement));
        }

        /// <summary>
        /// Adds the singular rule.
        /// </summary>
        /// <param name="rule">The rule.</param>
        /// <param name="replacement">The replacement.</param>
        private static void AddSingularRule(string rule, string replacement)
        {
            _singulars.Add(new InflectorRule(rule, replacement));
        }

        /// <summary>
        /// Makes the plural.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string MakePlural(this string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            return SetRules(_plurals, word);
        }

        /// <summary>
        /// Makes the singular.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string MakeSingular(this string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            return SetRules(_singulars, word);
        }

        /// <summary>
        /// Applies the rules.
        /// </summary>
        /// <param name="rules">The rules.</param>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        private static string SetRules(IList<InflectorRule> rules, string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            string result = word;
            if (!_uncountables.Contains(word.ToLower()))
            {
                for (int i = rules.Count - 1; i >= 0; i--)
                {
                    string currentPass = rules[i].Apply(word);
                    if (currentPass != null)
                    {
                        result = currentPass;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Converts the string to title case.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string ToTitleCase(this string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            return Regex.Replace(ToHumanCase(AddUnderscores(word)), @"\b([a-z])",
                                 match => match.Captures[0].Value.ToUpper());
        }

        /// <summary>
        /// Converts the string to human case.
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
        /// <returns></returns>
        public static string ToHumanCase(this string lowercaseAndUnderscoredWord)
        {
            return MakeInitialCaps(Regex.Replace(lowercaseAndUnderscoredWord, @"_", " "));
        }

        /// <summary>
        /// Convert string to proper case
        /// </summary>
        /// <param name="sourceString">The source string.</param>
        /// <returns></returns>
        public static string ToProper(this string sourceString)
        {
            string propertyName = sourceString.ToPascalCase();
            return propertyName;
        }
        /// <summary>
        /// Converts the string to pascal case.
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
        /// <returns></returns>
        public static string ToPascalCase(this string lowercaseAndUnderscoredWord)
        {
            return ToPascalCase(lowercaseAndUnderscoredWord, true);
        }
        /// <summary>
        /// Converts text to pascal case...
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="removeUnderscores">if set to <c>true</c> [remove underscores].</param>
        /// <returns></returns>
        public static string ToPascalCase(this string text, bool removeUnderscores)
        {
            if (String.IsNullOrEmpty(text))
                return text;
            text = text.Replace("_", " ");
            string joinString = removeUnderscores ? String.Empty : "_";
            string[] words = text.Split(' ');
            if (words.Length > 1)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    words[i] = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(words[i].ToLowerInvariant());
                }
                return String.Join(joinString, words);
            }
            return MakeInitialCaps(words[0]);
        }
        /// <summary>
        /// Converts the string to camel case.
        /// </summary>
        /// <param name="lowercaseAndUnderscoredWord">The lowercase and underscored word.</param>
        /// <returns></returns>
        public static string ToCamelCase(this string lowercaseAndUnderscoredWord)
        {
            return MakeInitialLowerCase(ToPascalCase(lowercaseAndUnderscoredWord));
        }
        /// <summary>
        /// Adds the underscores.
        /// </summary>
        /// <param name="pascalCasedWord">The pascal cased word.</param>
        /// <returns></returns>
        public static string AddUnderscores(this string pascalCasedWord)
        {
            return
                Regex.Replace(
                    Regex.Replace(Regex.Replace(pascalCasedWord, @"([A-Z]+)([A-Z][a-z])", "$1_$2"), @"([a-z\d])([A-Z])",
                                  "$1_$2"), @"[-\s]", "_").ToLower();
        }

        /// <summary>
        /// Makes the initial caps.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string MakeInitialCaps(this string word)
        {
            var tmp = new char[word.Length];
            bool lastCharUpper = false;
            for (int i = 0; i < word.Length; i++)
            {
                if (i == 0)
                {
                    tmp[i] = char.ToUpperInvariant(word[i]);
                    lastCharUpper = true;
                }
                else if (char.IsUpper(word[i]))
                {
                    if (lastCharUpper)
                    {
                        tmp[i] = char.ToLowerInvariant(word[i]);
                    }
                    else
                    {
                        tmp[i] = word[i];
                        lastCharUpper = true;
                    }
                }
                else
                {
                    lastCharUpper = false;
                    tmp[i] = word[i];
                }
            }

            return new string(tmp);
        }

        /// <summary>
        /// Makes the initial lower case.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        public static string MakeInitialLowerCase(this string word)
        {
            if (string.IsNullOrEmpty(word)) return word;

            return String.Concat(word.Substring(0, 1).ToLower(), word.Substring(1));
        }

        /// <summary>
        /// Adds the ordinal suffix.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns></returns>
        //public static string AddOrdinalSuffix(this string number)
        //{
        //    if (number.IsStringNumeric())
        //    {
        //        int n = int.Parse(number);
        //        int nMod100 = n % 100;
        //        if (nMod100 >= 11 && nMod100 <= 13)
        //            return String.Concat(number, "th");
        //        switch (n % 10)
        //        {
        //            case 1:
        //                return String.Concat(number, "st");
        //            case 2:
        //                return String.Concat(number, "nd");
        //            case 3:
        //                return String.Concat(number, "rd");
        //            default:
        //                return String.Concat(number, "th");
        //        }
        //    }
        //    return number;
        //}
        /// <summary>
        /// Converts the underscores to dashes.
        /// </summary>
        /// <param name="underscoredWord">The underscored word.</param>
        /// <returns></returns>
        public static string ConvertUnderscoresToDashes(this string underscoredWord)
        {
            return underscoredWord.Replace('_', '-');
        }

        /// <summary>
        /// Checks if the first character of a string if capitalized.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public static bool IsUpperCase(this string word)
        {
            return (new Regex("^[A-Z][^\\s]*")).IsMatch(word);
        }

        #region Nested type: InflectorRule

        /// <summary>
        /// Summary for the InflectorRule class
        /// </summary>
        private class InflectorRule
        {
            /// <summary>
            ///
            /// </summary>
            public readonly Regex regex;

            /// <summary>
            ///
            /// </summary>
            public readonly string replacement;

            /// <summary>
            /// Initializes a new instance of the <see cref="InflectorRule"/> class.
            /// </summary>
            /// <param name="regexPattern">The regex pattern.</param>
            /// <param name="replacementText">The replacement text.</param>
            public InflectorRule(string regexPattern, string replacementText)
            {
                regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
                replacement = replacementText;
            }

            /// <summary>
            /// Applies the specified word.
            /// </summary>
            /// <param name="word">The word.</param>
            /// <returns></returns>
            public string Apply(string word)
            {
                if (!regex.IsMatch(word))
                    return null;

                string replace = regex.Replace(word, replacement);
                if (word == word.ToUpper())
                    replace = replace.ToUpper();

                return replace;
            }
        }

        #endregion
    }
}
