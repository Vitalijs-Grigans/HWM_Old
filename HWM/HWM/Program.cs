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
            // Apply and build configuration for instance
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Load settings to named object from appsettings.json
            var settings = config.GetRequiredSection("Settings").Get<Settings>();

            // Initialize parser session
            var parser = new LeaderGuildParser
            (
                new Dictionary<string, string>()
                {
                    { "LeaderGuildEndpoint", settings.LeaderGuildEndpoint },
                    { "CharacterProgressEndpoint", settings.CharacterProgressEndpoint },
                    { "CreatureOwnersList", string.Join(",", settings.CreatureOwnersList) },
                    { "ParseResultsFolder", settings.ParseResultsFolder },
                    { "CreatureImageFolder", settings.CreatureImageFolder },
                }
            );

            // Verify character leader guild lvl
            await parser.EnsureOwnersStatusAsync();

            // Execute data fetching part
            await parser.CollectDataAsync();

            // Execute calculation part
            await parser.ProcessDataAsync();
        }
    }
}
