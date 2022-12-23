using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using HWM.Parser.Entities.LeaderGuild;

namespace HWM.Parser.Helpers
{
    public static class FileStoreHelper
    {
        public static void SaveJsonFile(IList<Follower> data, string path)
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            File.WriteAllText(path, json);
        }
    }
}
