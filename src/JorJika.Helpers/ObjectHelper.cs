using JorJika.Helpers.Attributes;
using JorJika.Helpers.Rows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace JorJika.Helpers
{
    public static class ObjectHelper
    {
        #region Object Copy

        public static void CopyTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            if (destination == null)
                destination = Activator.CreateInstance<TDestination>();

            var destinationProperties = destination.GetType().GetProperties();

            var sourceProperties = source.GetType().GetProperties().Where(sp => destinationProperties.Select(dp => dp.Name).Contains(sp.Name));

            foreach (var sourceProperty in sourceProperties)
            {
                var destinationProperty = destinationProperties.First(dp => dp.Name == sourceProperty.Name);

                if (sourceProperty.PropertyType.IsValueType != destinationProperty.PropertyType.IsValueType) continue;

                if (destinationProperty.PropertyType == sourceProperty.PropertyType)
                {
                    destinationProperty.SetValue(destination, sourceProperty.GetValue(source));
                }
                else
                {
                    if (sourceProperty.PropertyType.IsValueType && destinationProperty.PropertyType.IsValueType)
                    {
                        try
                        {
                            var destValue = Convert.ChangeType(sourceProperty.GetValue(source), destinationProperty.PropertyType);
                            destinationProperty.SetValue(destination, destValue);
                        }
                        catch { }
                    }
                    else if (typeof(IEnumerable).IsAssignableFrom(sourceProperty.PropertyType) && sourceProperty.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(destinationProperty.PropertyType) && destinationProperty.PropertyType != typeof(string))
                    {
                        List<object> sourceObjectList = (sourceProperty.GetValue(source) as IEnumerable<object>).ToList();
                        dynamic destinationObjectList = destinationProperty.GetValue(destination);

                        Type destinationItemType = destinationProperty.PropertyType.GetGenericArguments().FirstOrDefault();
                        var destinationListType = typeof(List<>).MakeGenericType(destinationItemType);

                        if ((sourceObjectList?.Any() ?? false))
                        {
                            if (destinationObjectList == null)
                                destinationObjectList = Activator.CreateInstance(destinationListType);

                            var arr = destinationObjectList.ToArray();

                            for (int i = 0; i < sourceObjectList.Count(); i++)
                            {
                                var sourceListItem = sourceObjectList[i];

                                if (arr.Length <= i)
                                {
                                    var destinationObjectItem = Activator.CreateInstance(destinationItemType);
                                    sourceListItem.CopyTo(destinationObjectItem);
                                    destinationListType.GetMethod("Add").Invoke(destinationObjectList, new[] { destinationObjectItem });
                                }
                                else
                                {
                                    var destItem = destinationObjectList[i];
                                    sourceListItem.CopyTo((object)destItem);
                                }
                            }

                            for (int i = sourceObjectList.Count(); i < arr.Length; i++)
                            {
                                destinationListType.GetMethod("RemoveAt").Invoke(destinationObjectList, new[] { (object)sourceObjectList.Count() });
                            }
                        }
                    }
                    else
                    {
                        var sourceObjProperty = sourceProperty.GetValue(source);
                        var destObjProperty = destinationProperty.GetValue(destination);
                        sourceObjProperty.CopyTo(destObjProperty);
                    }
                }
            }
        }

        #endregion

        #region Track Changes

        public static string GetIdentifier(this object obj)
        {
            if (obj == null) return null;
            var identifier = (from prop in obj.GetType().GetProperties()
                              let logIdentifier = prop.GetCustomAttributes(typeof(IdAttribute), true)?.FirstOrDefault() as IdAttribute ?? null
                              let displayNameAttr = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)?.FirstOrDefault() as DisplayNameAttribute ?? null
                              where logIdentifier != null
                              select new { prop.Name, displayNameAttr?.DisplayName }).ToDictionary(arg => arg.Name, arg => arg.DisplayName ?? arg.Name)?.FirstOrDefault();

            var identifierValue = obj?.GetType()?.GetProperty(identifier?.Key ?? "")?.GetValue(obj);

            return identifierValue?.ToString();
        }

        public static List<ChangeBaseRow> TrackChanges<TRow>(TRow oldRow, TRow newRow, string userId = null, string user = null, Type rowItemType = null)
        {
            var changeList = new List<ChangeBaseRow>();

            var oldRowProperties = (from prop in (typeof(TRow).FullName == "System.Object" && rowItemType != null ? rowItemType.GetProperties() : typeof(TRow).GetProperties())
                                    let changeSensitiveAttr = prop.GetCustomAttributes(typeof(ChangeSensitiveAttribute), true)?.FirstOrDefault() as ChangeSensitiveAttribute ?? null
                                    let displayNameAttr = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)?.FirstOrDefault() as DisplayNameAttribute ?? null
                                    where (changeSensitiveAttr != null)
                                    select new { prop.Name, PropertyTypeName = prop.PropertyType.FullName, DisplayName = displayNameAttr?.DisplayName ?? prop.Name, PropertyType = prop.PropertyType, prop });

            var identifier = (from prop in (typeof(TRow).FullName == "System.Object" && rowItemType != null ? rowItemType.GetProperties() : typeof(TRow).GetProperties())
                              let idAttr = prop.GetCustomAttributes(typeof(IdAttribute), true)?.FirstOrDefault() as IdAttribute ?? null
                              let displayNameAttr = prop.GetCustomAttributes(typeof(DisplayNameAttribute), true)?.FirstOrDefault() as DisplayNameAttribute ?? null
                              where idAttr != null
                              select new { prop.Name, DisplayName = displayNameAttr?.DisplayName ?? prop.Name }).ToDictionary(arg => arg.Name, arg => arg.DisplayName ?? arg.Name)?.FirstOrDefault();

            var identifierValue = oldRow?.GetType()?.GetProperty(identifier?.Key ?? "")?.GetValue(oldRow);

            var rootName = (typeof(TRow).FullName == "System.Object" && rowItemType != null ? rowItemType.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? rowItemType.Name :
                                                                                             typeof(TRow).GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? typeof(TRow).Name);


            foreach (var oldRowProperty in oldRowProperties)
            {
                var oldRowProp = oldRow?.GetType()?.GetProperty(oldRowProperty.Name);
                var oldRowPropertyValue = oldRowProp?.GetValue(oldRow);

                var newRowProp = newRow?.GetType()?.GetProperty(oldRowProperty.Name);
                var newRowPropertyValue = newRowProp?.GetValue(newRow);

                if (oldRowProp.PropertyType.IsGenericType && oldRowProp.PropertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>) && oldRowProp.PropertyType != typeof(string))
                {
                    var oldRowPropertyList = (oldRowPropertyValue as IEnumerable)?.OfType<dynamic>()?.ToDictionary(x => x.Key, y => y.Value);
                    var newRowPropertyList = (newRowPropertyValue as IEnumerable)?.OfType<dynamic>()?.ToDictionary(x => x.Key, y => y.Value);

                    Type rItemType = oldRowProp.PropertyType.GetGenericArguments()[1];

                    IEnumerable<ChangeBaseRow> enumerableChangesList = TrackDictionaryChanges(oldRowProperty.DisplayName, oldRowPropertyList, newRowPropertyList, rItemType);
                    changeList.AddRange(enumerableChangesList);
                    continue;
                }

                if (typeof(IEnumerable).IsAssignableFrom(oldRowProp.PropertyType) && oldRowProp.PropertyType != typeof(string))
                {
                    List<object> oldRowPropertyList = (oldRowPropertyValue as IEnumerable<object>)?.ToList();
                    List<object> newRowPropertyList = (newRowPropertyValue as IEnumerable<object>)?.ToList();

                    Type rItemType = oldRowProp.PropertyType.GetGenericArguments().FirstOrDefault();

                    if (oldRowPropertyList == null)
                    {
                        var listType = typeof(List<>).MakeGenericType(rItemType);
                        oldRowPropertyList = Activator.CreateInstance(listType) as List<object>;
                    }

                    if (newRowPropertyList == null)
                    {
                        var listType = typeof(List<>).MakeGenericType(rItemType);
                        newRowPropertyList = Activator.CreateInstance(listType) as List<object>;
                    }

                    IEnumerable<ChangeBaseRow> enumerableChangesList = TrackListChanges(oldRowProperty.DisplayName, oldRowPropertyList, newRowPropertyList, rItemType);
                    changeList.AddRange(enumerableChangesList);
                    continue;
                }

                if (oldRowPropertyValue == null && newRowPropertyValue == null) continue;

                FormatProperty(ref oldRowPropertyValue, ref newRowPropertyValue, oldRowProperty.prop);

                if (oldRowPropertyValue.ToString() == newRowPropertyValue.ToString()) continue;

                var logRow = new ChangeBaseRow();
                logRow.UserId = userId;
                logRow.User = user;
                logRow.Location = rootName;
                logRow.Identifier = identifierValue != null ? ((identifier?.Value ?? "Identifier") + $": {identifierValue}") : "New";
                logRow.FieldName = oldRowProperty.DisplayName;
                logRow.OldValue = oldRowPropertyValue?.ToString() ?? "";
                logRow.NewValue = newRowPropertyValue?.ToString() ?? "";
                changeList.Add(logRow);
            }

            return changeList;
        }

        public static List<ChangeBaseRow> TrackListChanges(string location, IEnumerable<object> oldRows, IEnumerable<object> newRows, Type rowItemType)
        {
            var result = new List<ChangeBaseRow>();

            if (oldRows == null) oldRows = new List<object>();
            if (newRows == null) newRows = new List<object>();

            foreach (var oldRow in oldRows)
            {
                var newRow = newRows.FirstOrDefault(row => row.GetIdentifier() != null && row.GetIdentifier() == oldRow.GetIdentifier());
                var deleting = false;

                var blankNewRow = Activator.CreateInstance(rowItemType);

                deleting = newRow == null;

                result.AddRange(TrackSingleChange(location, deleting ? "Remove" : "Change", oldRow, deleting ? blankNewRow : newRow, rowItemType).AsEnumerable());
            }

            foreach (var newRow in newRows?.Where(row => row.GetIdentifier() == null || !oldRows.Any(o => o.GetIdentifier() != null && o.GetIdentifier() == row.GetIdentifier())))
            {
                if (newRow != null)
                {
                    var blankOldRow = Activator.CreateInstance(rowItemType);
                    result.AddRange(TrackSingleChange(location, "Add", blankOldRow, newRow, rowItemType).AsEnumerable());
                }
            }

            return result;
        }

        public static List<ChangeBaseRow> TrackDictionaryChanges(string location, IDictionary<object, object> oldRows, IDictionary<object, object> newRows, Type rowItemType)
        {
            var result = new List<ChangeBaseRow>();

            if (oldRows == null && newRows == null) return result;

            if (oldRows == null) oldRows = new Dictionary<object, object>();
            if (newRows == null) newRows = new Dictionary<object, object>();

            foreach (var oldRow in oldRows)
            {
                var newRow = newRows.ContainsKey(oldRow.Key) ? newRows[oldRow.Key] : null;
                var deleting = false;

                object blankNewRow = null;
                if (rowItemType == typeof(string))
                    blankNewRow = string.Empty;
                else
                    blankNewRow = Activator.CreateInstance(rowItemType);

                deleting = newRow == null;

                result.AddRange(TrackSingleChange($"{location}=>{oldRow.Key}", deleting ? "Remove" : "Change", oldRow.Value, deleting ? blankNewRow : newRow, rowItemType).AsEnumerable());
            }

            foreach (var newRow in newRows?.Where(row => !oldRows.Any(o => o.Key.ToString() == row.Key.ToString())))
            {
                if (newRow.Value != null)
                {
                    object blankOldRow = null;
                    if (rowItemType == typeof(string))
                        blankOldRow = string.Empty;
                    else
                        blankOldRow = Activator.CreateInstance(rowItemType);

                    result.AddRange(TrackSingleChange($"{location}=>{newRow.Key}", "Add", blankOldRow, newRow.Value, rowItemType).AsEnumerable());
                }
            }

            return result;
        }

        public static List<ChangeBaseRow> TrackSingleChange<TRow>(string location, string action, TRow oldRow, TRow newRow, Type rowItemType)
        {
            var result = new List<ChangeBaseRow>();
            if (newRow == null) return result;

            List<ChangeBaseRow> dataToLog = new List<ChangeBaseRow>();

            if (rowItemType == typeof(string))
            {
                if ((oldRow?.ToString() ?? "") != (newRow?.ToString() ?? ""))
                    dataToLog.Add(new ChangeBaseRow()
                    {
                        FieldName = "Value",
                        OldValue = oldRow?.ToString() ?? "",
                        NewValue = newRow?.ToString() ?? "",
                    });
            }
            else
                dataToLog = TrackChanges(oldRow, newRow, rowItemType: rowItemType);

            foreach (var logRow in dataToLog)
            {
                logRow.Location = location;
                logRow.Action = action;
                result.Add(logRow);
            }

            return result;
        }

        #endregion

        #region Kakha

        #region properties

        private static readonly Type formatAttributeType = typeof(FormatAttribute);
        private static readonly Type idPropertyAttributeType = typeof(IdAttribute);

        #endregion

        #region comparison method

        public static bool AreEqual<T>(T sourceType, T targetType) where T : class
        {
            if (sourceType is null ^ targetType is null)
                return false;

            if (sourceType is null)
                return true;

            foreach (var property in sourceType.GetType().GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(ChangeSensitiveAttribute))).ToList())
            {
                if (property is null)
                    throw new ArgumentNullException($"{nameof(property)} is Null in Type {nameof(sourceType)}");

                if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                {
                    var sourceDictionary = (property?.GetValue(sourceType) as IDictionary);
                    var targetDictionary = (property?.GetValue(targetType) as IDictionary);

                    if (!DictionariesAreEqual(sourceDictionary, targetDictionary))
                        return false;
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    var sourceCollection = property?.GetValue(sourceType) as ICollection;
                    var targetCollection = property?.GetValue(targetType) as ICollection;

                    if (!SequencesAreEqual(sourceCollection, targetCollection))
                        return false;
                }
                else
                {
                    if (!PropertyValuesAreEqual(sourceType, targetType, property))
                        return false;
                }
            }

            return true;
        }

        #endregion

        #region Helpers

        private static bool SequencesAreEqual(ICollection sourceCollection, ICollection targetCollection)
        {
            if (sourceCollection is null ^ targetCollection is null)
                return false;

            if (sourceCollection?.Count != targetCollection?.Count)
                return false;

            if (sourceCollection is null)
                return true;

            foreach (var sourceItem in sourceCollection)
            {
                var sourceKeyProperty = (sourceItem).GetType().GetProperties()
                    .Where(prop => Attribute.IsDefined(prop, idPropertyAttributeType))
                    .FirstOrDefault()
                    ?? throw new ArgumentException("IdAttribute Not Found in SourceItem");

                bool matchIsFound = false;
                foreach (var targetItem in targetCollection)
                {
                    if (PropertyValuesAreEqual(sourceItem, targetItem, sourceKeyProperty))
                    {
                        var sourceProperties = (sourceItem).GetType().GetProperties()
                            .Where(prop => prop.Name != sourceKeyProperty.Name && Attribute.IsDefined(prop, typeof(ChangeSensitiveAttribute)))
                            .ToList();

                        foreach (var property in sourceProperties)
                        {
                            if (!PropertyValuesAreEqual(sourceItem, targetItem, property))
                                return false;
                        }

                        matchIsFound = true;
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (!matchIsFound)
                    return false;
                else
                    continue;
            }

            return true;
        }

        private static bool DictionariesAreEqual(IDictionary sourceDictionary, IDictionary targetDictionary)
        {
            if (sourceDictionary is null && targetDictionary is null)
                return true;

            if (sourceDictionary is null || targetDictionary is null)
                return false;

            if (sourceDictionary.Count != targetDictionary.Count)
                return false;

            foreach (var sourceDictionaryKey in sourceDictionary.Keys)
                if ((sourceDictionary[sourceDictionaryKey]?.ToString() ?? string.Empty) != (targetDictionary[sourceDictionaryKey]?.ToString() ?? string.Empty))
                    return false;

            return true;
        }

        private static bool PropertyValuesAreEqual(object sourceType, object targetType, PropertyInfo property)
        {
            var sourcePropertyValue = property?.GetValue(sourceType);
            var targetPropertyValue = property?.GetValue(targetType);

            if (sourcePropertyValue == null && targetPropertyValue == null)
                return true;

            if (sourcePropertyValue != null && targetPropertyValue == null)
                return false;

            if (sourcePropertyValue == null && targetPropertyValue != null)
                return false;

            FormatProperty(ref sourcePropertyValue, ref targetPropertyValue, property);

            if ((!sourcePropertyValue?.Equals(targetPropertyValue)) ?? targetPropertyValue != null)
                return false;

            return true;
        }

        private static void FormatProperty(ref object sourcePropertyValue, ref object targetPropertyValue, PropertyInfo property)
        {
            if (Attribute.IsDefined(property, formatAttributeType))
            {
                var dateAttribute = property.GetCustomAttributes(true)
                    .Where(attr => attr.GetType() == formatAttributeType)
                    .FirstOrDefault();

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Date)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? (Convert.ToDateTime(sourcePropertyValue)).ToString("yyyy-MM-dd") : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? (Convert.ToDateTime(targetPropertyValue)).ToString("yyyy-MM-dd") : string.Empty;
                    return;
                }

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.DateTime)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? (Convert.ToDateTime(sourcePropertyValue)).ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? (Convert.ToDateTime(targetPropertyValue)).ToString("yyyy-MM-dd HH:mm:ss") : string.Empty;
                    return;
                }

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Decimal)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? Math.Round(Convert.ToDecimal(sourcePropertyValue), 0).ToString() : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? Math.Round(Convert.ToDecimal(targetPropertyValue), 0).ToString() : string.Empty;
                    return;
                }

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Decimal2)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? Math.Round(Convert.ToDecimal(sourcePropertyValue), 2).ToString() : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? Math.Round(Convert.ToDecimal(targetPropertyValue), 2).ToString() : string.Empty;
                    return;
                }

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Decimal4)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? Math.Round(Convert.ToDecimal(sourcePropertyValue), 4).ToString() : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? Math.Round(Convert.ToDecimal(targetPropertyValue), 4).ToString() : string.Empty;
                    return;
                }

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Decimal6)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? Math.Round(Convert.ToDecimal(sourcePropertyValue), 6).ToString() : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? Math.Round(Convert.ToDecimal(targetPropertyValue), 6).ToString() : string.Empty;
                    return;
                }

                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Decimal8)
                {
                    sourcePropertyValue = sourcePropertyValue != null ? Math.Round(Convert.ToDecimal(sourcePropertyValue), 8).ToString() : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? Math.Round(Convert.ToDecimal(targetPropertyValue), 8).ToString() : string.Empty;
                    return;
                }


                if ((dateAttribute as FormatAttribute)?.Type == FormatType.Custom)
                {
                    var stringFormat = (dateAttribute as FormatAttribute)?.Format;
                    if (!string.IsNullOrWhiteSpace(stringFormat))
                    {
                        sourcePropertyValue = sourcePropertyValue != null ? string.Format(stringFormat, sourcePropertyValue).ToString() : string.Empty;
                        targetPropertyValue = targetPropertyValue != null ? string.Format(stringFormat, targetPropertyValue).ToString() : string.Empty;
                    }

                    return;
                }

                if (property.PropertyType == typeof(string))
                {
                    sourcePropertyValue = sourcePropertyValue != null ? sourcePropertyValue.ToString().ToLower().Trim() : string.Empty;
                    targetPropertyValue = targetPropertyValue != null ? targetPropertyValue.ToString().ToLower().Trim().ToString() : string.Empty;
                    return;
                }
            }
            else
            {
                sourcePropertyValue = sourcePropertyValue?.ToString() ?? string.Empty;
                targetPropertyValue = targetPropertyValue?.ToString() ?? string.Empty;
            }
        }

        #endregion

        #endregion
    }
}
