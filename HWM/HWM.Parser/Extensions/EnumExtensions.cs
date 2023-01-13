using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace HWM.Parser.Extensions
{
    public static class EnumExtensions
    {
        // Store thread-safe named cache collection
        private static readonly ConcurrentDictionary<string, string> DisplayNameCache =
            new ConcurrentDictionary<string, string>();

        // Convert enum value name to represantable string using Description attribute
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
