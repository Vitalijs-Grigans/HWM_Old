using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;

using HWM.Parser;

namespace HWM
{
    [SupportedOSPlatform("windows")]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var settings = config.GetRequiredSection("Settings").Get<Settings>();

            var parser = new LeaderGuildParser
            (
                new Dictionary<string, string>()
                {
                    { "LeaderGuildEndpoint", settings.LeaderGuildEndpoint },
                    { "ParseResultsFolder", settings.ParseResultsFolder },
                    { "CreatureImageFolder", settings.CreatureImageFolder },
                }
            );

            await parser.CollectDataAsync();
            await parser.ProcessDataAsync();
        }
    }
}
