using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using Newtonsoft.Json;

using HWM.Parser.Entities.LeaderGuild;

namespace HWM.Parser.Helpers
{
    [SupportedOSPlatform("windows")]
    public sealed class GenericExternalServices<T>
    {
        // Singleton object implementation
        private static readonly Lazy<GenericExternalServices<T>> requestHelper =
            new Lazy<GenericExternalServices<T>>(() => new GenericExternalServices<T>());

        private GenericExternalServices() { }

        public static GenericExternalServices<T> Instance
        {
            get
            {
                return requestHelper.Value;
            }
        }

        // Asynchronous method to load JSON from file
        public async Task<T> LoadJsonAsync(string path)
        {
            string json = await File.ReadAllTextAsync(path);

            return JsonConvert.DeserializeObject<T>(json);
        }

        // Asynchronous method to store JSON in file
        public async Task SaveJsonAsync(T data, string path)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            await File.WriteAllTextAsync(path, json);
        }
    }
}
