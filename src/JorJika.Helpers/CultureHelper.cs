using System;
using System.Globalization;

namespace JorJika.Helpers
{
    public static class CultureHelper
    {
        public static void StandardCulture(string cultureName = "ka-GE")
        {
            var culture = StandardCultureGet(cultureName);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static CultureInfo StandardCultureGet(string cultureName = "ka-GE")
        {
            var culture = CultureInfo.CreateSpecificCulture(cultureName);

            culture.DateTimeFormat.ShortDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.LongDatePattern = "yyyy-MM-dd";
            culture.DateTimeFormat.ShortTimePattern = "HH:mm:ss";
            culture.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            culture.NumberFormat.NumberDecimalSeparator = ".";
            culture.NumberFormat.CurrencyDecimalSeparator = ".";
            culture.NumberFormat.PercentDecimalSeparator = ".";

            return culture;
        }
    }
}
