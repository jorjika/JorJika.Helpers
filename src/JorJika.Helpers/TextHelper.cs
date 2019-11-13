using System;
using System.Linq;
using System.Text;

namespace JorJika.Helpers
{
    public static class TextHelper
    {
        /// <summary>
        /// Sylfaen ტექსტის Latin-ში გადაყვანა
        /// </summary>
        /// <param name="text">ტექსტი</param>
        /// <param name="correctVersion">თუ გადასცემთ True-ს ჭ - გადაყავს tch-ში და ა.შ., თუ False - ჭ გადაყავს W -ში.</param>
        /// <param name="fullName">სახელის თარგმნა</param>
        /// <returns></returns>
        public static string SylfaenToLatin(string text, bool correctVersion = false, bool fullName = false)
        {
            var result = "";

            var sylfaenSymbols = new[] { "ქ", "წ", "ე", "რ", "ტ", "ყ", "უ", "ი", "ო", "პ", "ა", "ს", "დ", "ფ", "გ", "ჰ", "ჯ", "კ", "ლ", "ზ", "ხ", "ც", "ვ", "ბ", "ნ", "მ", "ჭ", "ღ", "თ", "შ", "ჟ", "ძ", "ჩ" };

            var latinSymbols = correctVersion ?
                new[] { "q", "ts", "e", "r", "t", "k", "u", "i", "o", "p", "a", "s", "d", "p", "g", "h", "j", "k", "l", "z", "kh", "ts", "v", "b", "n", "m", "tch", "g", "t", "sh", "zh", "dz", "ch" } :
                new[] { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p", "a", "s", "d", "f", "g", "h", "j", "k", "l", "z", "x", "c", "v", "b", "n", "m", "W", "R", "T", "S", "J", "Z", "C" };

            var sb = new StringBuilder();

            foreach (var symbol in text.ToCharArray())
            {
                var sylfaenIndex = Array.IndexOf(sylfaenSymbols, symbol.ToString());
                if (sylfaenIndex != -1)
                    sb.Append(latinSymbols[sylfaenIndex]);
                else
                    sb.Append(symbol.ToString());
            }

            if (fullName)
            {
                var str = sb.ToString();
                var spl = str.Replace("  ", "").Replace("  ", " ").Trim().Split(' ');

                var fullNameBuilder = new StringBuilder();

                foreach (var item in spl)
                {
                    if (item.Length > 0)
                        fullNameBuilder.Append(item.First().ToString().ToUpper());

                    if (item.Length > 1)
                        fullNameBuilder.Append(item.Substring(1));

                    if (item.Length > 0)
                        fullNameBuilder.Append(' ');
                }

                result = fullNameBuilder.ToString().Trim();
                //if (spl.Length == 2)
                //{
                //    var firstName = spl[0].Trim();
                //    firstName = firstName.First().ToString().ToUpper() + firstName.Substring(1);
                //    var lastName = spl[1].Trim();
                //    lastName = lastName.First().ToString().ToUpper() + lastName.Substring(1);
                //    result = firstName + " " + lastName;
                //}
                //else
                //{
                //    result = str.First().ToString().ToUpper() + str.Substring(1);
                //}
            }
            else
                result = sb.ToString();

            return result;
        }
    }
}
