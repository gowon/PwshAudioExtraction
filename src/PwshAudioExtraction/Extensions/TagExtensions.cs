namespace PwshAudioExtraction.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using TagLib;

    public static class TagExtensions
    {
        public static void SetProperties(this Tag tag, IEnumerable<KeyValuePair<string, object>> values)
        {
            foreach (var pair in values)
            {
                SetProperty(tag, pair.Key, pair.Value);
            }
        }

        public static void SetProperty(this Tag tag, string key, object value)
        {
            var propertyInfo = tag.GetType().GetProperty(key, BindingFlags.Public | BindingFlags.Instance) ??
                               throw new InvalidOperationException($"Cannot find a setter for property '{key}'.");
            propertyInfo.SetValue(tag, value);
        }
    }
}