using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Linq
{
    public static class ListHelper
    {
        public static bool In<T>(this IEnumerable<T> list, params T[] param)
        {
            return list?.Any(r => param?.Contains(r) ?? false) ?? false;
        }

        public static bool In<T>(this T obj, params T[] param)
        {
            return param?.Contains(obj) ?? false;
        }

        public static List<T> Clone<T>(this List<T> original)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(ms, original);
                ms.Seek(0, SeekOrigin.Begin);
                return (List<T>)binaryFormatter.Deserialize(ms);
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            var props = TypeDescriptor.GetProperties(typeof(T));
            var table = new DataTable();

            for (var i = 0; i < props.Count; i++)
            {
                var prop = props[i];
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            var values = new object[props.Count];
            foreach (var item in list)
            {
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }

                table.Rows.Add(values);
            }
            return table;
        }

        public static DataTable ToDataTable<T>(this T[] list, string fieldName = "Value")
        {
            var table = new DataTable();

            table.Columns.Add(fieldName, typeof(T));

            var values = new object[1];
            foreach (var item in list)
            {
                values[0] = item;
                table.Rows.Add(values);
            }

            return table;
        }
    }
}
