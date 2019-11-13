using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

namespace JorJika.Helpers
{
    public static class StringHelper
    {
        public static string ToAlphaNumericOnly(this string input)
        {
            if (input == null) return null;
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            return rgx.Replace(input, "");
        }

        public static string ToAlphaOnly(this string input)
        {
            if (input == null) return null;
            Regex rgx = new Regex("[^a-zA-Z]");
            return rgx.Replace(input, "");
        }
        public static string ToSylfaenOnly(this string input)
        {
            if (input == null) return null;
            Regex rgx = new Regex("[^ა-ჰ]");
            return rgx.Replace(input, "");
        }

        public static string ToNumericOnly(this string input)
        {
            if (input == null) return null;
            Regex rgx = new Regex("[^0-9]");
            return rgx.Replace(input, "");
        }

        public static string ToCustomOnly(this string input, string allowedChars = "a-zA-Z")
        {
            if (input == null) return null;
            Regex rgx = new Regex("[^" + allowedChars + "]");
            return rgx.Replace(input, "");
        }

        public static bool IsURLData(this string input)
        {
            Regex regex = new Regex(@"^data:(.*)?;base64\,(.+)");
            Match match = regex.Match(input);
            return match.Success && match.Groups.Count == 3;
        }

        public static string URLDataToBase64(this string input)
        {
            Regex regex = new Regex(@"^data:(.*)?;base64\,(.+)");
            Match match = regex.Match(input);
            if (match.Success && match.Groups.Count == 3)
            {
                var result = match.Groups[2].Value;
                return string.IsNullOrWhiteSpace(result) ? null : result;
            }
            else
                return null;
        }
        public static byte[] URLDataToBytes(this string input)
        {
            var base64Data = URLDataToBase64(input);
            return string.IsNullOrWhiteSpace(base64Data) ? null : Convert.FromBase64String(base64Data);
        }
    }
}
