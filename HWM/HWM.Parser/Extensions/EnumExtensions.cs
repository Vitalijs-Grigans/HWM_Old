using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace HWM.Parser.Extensions
{
    public static class EnumExtensions
    {
        private static readonly ConcurrentDictionary<string, string> DisplayNameCache =
            new ConcurrentDictionary<string, string>();

        public static string GetDisplayName(this Enum value)
        {
            string key = $"{value.GetType().FullName}.{value}";

            var displayName = DisplayNameCache.GetOrAdd(key, x =>
            {
                var name = (DescriptionAttribute[])value
                    .GetType()
                    .GetTypeInfo()
                    .GetField(value.ToString())
                    .GetCustomAttributes(typeof(DescriptionAttribute), false);

                return name.Length > 0 ? name.First().Description : value.ToString();
            });

            return displayName;
        }
    }
}
