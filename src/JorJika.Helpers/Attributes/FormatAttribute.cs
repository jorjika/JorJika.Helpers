using System;

namespace JorJika.Helpers.Attributes
{
    public class FormatAttribute : Attribute
    {
        public string Format { get; }
        public FormatType Type { get; }

        public FormatAttribute(FormatType type, string format = null)
        {
            Format = format;
            Type = type;
        }
    }

    public enum FormatType
    {
        DateTime,
        Date,
        Decimal,
        Decimal2,
        Decimal4,
        Decimal6,
        Decimal8,
        Custom
    }
}
