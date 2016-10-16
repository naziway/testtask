using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Heathmill.WpfUtilities
{
    public static class StringExtensions
    {
        public static string TrimStartsWith(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return s1;
            return s1.StartsWith(s2) ? s1.Substring(s2.Length) : s1;
        }

        public static string TrimStartsWithIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return s1;
            return s1.StartsWithIgnoreCase(s2) ? s1.Substring(s2.Length) : s1;
        }

        public static string TrimEndsWith(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return s1;
            return s1.EndsWith(s2) ? s1.Substring(0, s1.Length - s2.Length) : s1;
        }

        public static string TrimEndsWithIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return s1;
            return s1.EndsWithIgnoreCase(s2) ? s1.Substring(0, s1.Length - s2.Length) : s1;
        }

        public static bool StartsWithIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return false;
            if (s1.Length < s2.Length) return false;
            return IgnoreCaseCompare(s1.Substring(0, s2.Length), s2);
        }

        public static bool EndsWithIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return false;
            if (s1.Length < s2.Length) return false;
            return IgnoreCaseCompare(s1.Right(s2.Length), s2);
        }

        public static bool LeftIgnoreCaseCompare(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return (s1 == s2);
            int i = Math.Min(s1.Length, s2.Length);
            return IgnoreCaseCompare(s1.Substring(0, i), s2.Substring(0, i));
        }

        public static bool RightIgnoreCaseCompare(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                return (s1 == s2);
            int i = Math.Min(s1.Length, s2.Length);
            return IgnoreCaseCompare(Right(s1, i), Right(s2, i));
        }

        public static string Right(this string s, int i)
        {
            if (i < 0) throw new ArgumentException("Negative length");
            if (string.IsNullOrEmpty(s)) return s;
            return s.Substring(Math.Max(s.Length - i, 0));
        }

        public static bool StringCompareWithNullsEqual(this object x1, object x2)
        {
            if (x1 == null || x2 == null) return (x1 == x2);
            return CompareWithNullsEqual(x1.ToString(), x2.ToString());
        }

        public static bool CompareWithNullsEqual(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return (s1 == s2);
            return String.Compare(s1, s2) == 0;
        }

        public static bool StringCompareWithNullsEqualIgnoreCase(this object x1, object x2)
        {
            if (x1 == null || x2 == null) return (x1 == x2);
            return CompareWithNullsEqual(x1.ToString(), x2.ToString());
        }

        public static bool CompareWithNullsEqualIgnoreCase(this string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return (s1 == s2);
            return IgnoreCaseCompare(s1, s2);
        }

        public static bool IgnoreCaseCompare(this string s1, string s2)
        {
            return string.Compare(s1, s2, true) == 0;
        }

        public static string BreakByCase(this string original)
        {
            // remove all spaces, we only want the ones we add
            original = original.Replace(" ", string.Empty);
            StringBuilder sb = new StringBuilder();

            foreach (Char c in original)
            {
                if (char.IsUpper(c)) sb.Append(" ");
                sb.Append(c);
            }

            return sb.ToString().Trim();
        }

        public static string Add3DigitCommaGroups(this string number)
        {
            if (string.IsNullOrWhiteSpace(number)) return string.Empty;
            string s = number.Trim();
            if (s.StartsWith(".")) return s;
            string[] atoms = s.Split('.');
            s = atoms[0];
            var i = Int64.Parse(s);
            atoms[0] = i.ToString("N0", CultureInfo.InvariantCulture);
            return string.Join(".", atoms);
        }

        public static string UnQuote(this string s)
        {
            if (s.Length < 2) return s;
            return (s[0] == '"' && s[s.Length - 1] == '"') ? s.Substring(1, s.Length - 2) : s;
        }

        public static string TrimQuotes(this string s)
        {
            string r = s;
            char q = '"';
            while (r.Length > 0)
            {
                int end = r.Length - 1;
                if (r[end] == q)
                {
                    r = r.Substring(0, end).Trim();
                }
                else if (r[0] == q)
                {
                    r = r.Substring(1);
                }
                else break;
            }
            return r;
        }

        public static string TrimNonBreaking(this string s)
        {
            if (s == null) throw new NullReferenceException("Can't trim null string");
            const char nbs = '\u00A0';
            const char nbs1 = '\uFFFD';
            string r = s.Trim();
            while (r.Length > 0)
            {
                int end = r.Length - 1;
                if (r[end] == nbs || r[end] == nbs1)
                {
                    r = r.Substring(0, end).Trim();
                }
                else if (r[0] == nbs || r[0] == nbs1)
                {
                    r = r.Substring(1).Trim();
                }
                else break;
            }
            return r;
        }

        public static string EncodePasswordToBase64(this string password, string salt)
        {
            return EncodePasswordToBase64(password + salt);
        }

        public static string EncodePasswordToBase64(this string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }

        public static string CurrencyCodeToSymbol(this string code)
        {
            if (code == null) throw new ArgumentNullException("code");
            switch (code.ToUpper())
            {
                case "AUD":
                    return "A$";
                case "CAD":
                    return "C$";
                case "CZK":
                    return "Kč";
                case "DKK":
                    return "kr";
                case "EUR":
                    return "€";
                case "GBP":
                    return "£";
                case "HKD":
                    return "HK$";
                case "ILS":
                    return "₪";
                case "JPY":
                    return "¥";
                case "KRW":
                    return "₩";
                case "NOK":
                    return "kr";
                case "SGD":
                    return "S$";
                case "USD":
                    return "$";
                case "SEK":
                    return "kr";
                default:
                    return code;
            }
        }

        public static T ToEnum<T>(this string s)
        {
            return (T) Enum.Parse(typeof (T), s);
        }

        public static T ToEnumIgnoreCase<T>(this string s)
        {
            return (T) Enum.Parse(typeof (T), s, true);
        }

        public static int ParseOrZero(this string s)
        {
            int i;
            bool ok = int.TryParse(s, out i);
            return ok ? i : 0;
        }
    }
}
