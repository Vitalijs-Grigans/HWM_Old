using System.IO;

using Microsoft.Extensions.Configuration;

using HWM.Parser;

namespace HWM
{
    class Program
    {
        static void Main(string[] args)
        {
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

            parser.CollectData();
        }
    }
}
