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
            
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            IConfigurationRoot config = builder.Build();

            var parser = new LeaderGuildParser
            (
                config.GetSection("LeaderGuildEndpoint").Value,
                config.GetSection("ParseResultsFolder").Value,
                config.GetSection("CreatureImageFolder").Value
            );

            await parser.CollectData();
            await parser.ProcessData();
        }
    }
}
