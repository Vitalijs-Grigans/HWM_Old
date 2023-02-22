using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using HtmlAgilityPack;
using Newtonsoft.Json;

using HWM.Parser.Helpers;
using HWM.Parser.Interfaces;
using HWM.Parser.Entities.Creature;
using HWM.Parser.Entities.LeaderGuild;
using HWM.Parser.Extensions;

namespace HWM.Parser
{
    [SupportedOSPlatform("windows")]
    public partial class LeaderGuildParser : IParser
    {
        public LeaderGuildParser(IDictionary<string, string> config)
        {
            _endpoint = config["LeaderGuildEndpoint"];
            _owners = config["CreatureOwnersList"].Split(',');
            _forceUpdate = config["CreatureForceUpdate"] == "1";
            _jsonFolder = config["ParseResultsFolder"];
            _imageFolder = config["CreatureImageFolder"];
        }

        // Asynchronous method to fetch required data from external service
        public async Task CollectDataAsync()
        {
            // Obtain existing creature info from local JSON file
            IEnumerable<Follower> cachedCreatures =
                await ExternalServices.Instance.LoadJsonAsync($@"{_jsonFolder}\LGCreatures.json");

            // Obtain follower names for each corresponding owner
            IDictionary<int, IList<string>> ownersCollection =
                await CreateOwnersCollection(_owners, _endpoint);

            // Obtain basic creature data containers
            HtmlDocument htmlDoc =
                await ExternalServices.Instance.GetHtmlAsync($@"{_endpoint}/leader.php");
            HtmlNodeCollection creatureNodeCollection =
                htmlDoc.DocumentNode.SelectNodes("//div[@class='fcont']//div[@class='ccont']");

            int quarter = creatureNodeCollection.Count / 4;

            // Iterate over collection to gain extra creature data
            IList<Follower> creatureList = new List<Follower>();
            int id = default;

            foreach (HtmlNode creatureNode in creatureNodeCollection)
            {
                Follower follower = default;

                int leadership = ConvertToNumber
                    (
                        creatureNode.Descendants("a")
                                    .FirstOrDefault(d => 
                                        d.Attributes.Contains("style") &&
                                        d.Attributes["style"].Value.StartsWith("color:"))
                                    .InnerText
                    );

                // Anchor element of creature
                var imageAnchor = 
                    creatureNode.Descendants("a").FirstOrDefault(d => 
                        d.Attributes.Contains("title"));

                // Url to creature details
                string pageUrl = imageAnchor.Attributes["href"]?.Value;

                // Creature name used for rendering
                string displayName = imageAnchor.Attributes["title"]?.Value;

                // Creature name used by system
                string name = pageUrl.Split('=').LastOrDefault();
                
                // Find existing creature if such exists
                Follower existingFollower = cachedCreatures.FirstOrDefault(c => c.Name == name);

                if (existingFollower == null || _forceUpdate)
                {
                    // Url to creature image
                    string imageUrl =
                        imageAnchor.Descendants("img")
                                   .FirstOrDefault(d =>
                                   d.Attributes.Contains("class") &&
                                   d.Attributes["class"].Value == "cre_mon_image1")
                                   .Attributes["src"]
                                   ?.Value;

                    // Download image
                    await ExternalServices.Instance.DownloadImageAsync(imageUrl, $@"{_imageFolder}\{name}.png");

                    // Get creature rank to enumerated value based on color background represented by hex code
                    Rarity tier = 
                        GetFollowerRarity
                        (
                            creatureNode.Attributes["style"]?.Value.Split(':').LastOrDefault()
                        );

                    // Save creature info and characteristics and add to collection
                    follower = new Follower()
                    {
                        Id = id++,
                        Url = pageUrl,
                        Name = name,
                        DisplayName = displayName,
                        Tier = tier,
                        DisplayTier = tier.GetDisplayName(),
                        Leadership = leadership,
                        Characteristics = await RetrieveCharacteristics(pageUrl)
                    };
                }

                else
                {
                    // Copy already cached creature
                    follower = existingFollower;

                    // Update when leadership changes
                    if (follower.Leadership != leadership)
                    {
                        follower.Leadership = leadership;
                    }
                }

                follower.Owners = SetOwners(ownersCollection, displayName);
                creatureList.Add(follower);

                DisplayProcessStatus(follower.Id, quarter);
            }

            // Store creature data into JSON file
            await ExternalServices.Instance.SaveJsonAsync(creatureList, $@"{_jsonFolder}\LGCreatures.json");
        }

        private string _endpoint;
        private IEnumerable<string> _owners;
        private bool _forceUpdate;
        private string _jsonFolder;
        private string _imageFolder;

        // Async method to obtain followers for each owner
        private async Task<IDictionary<int, IList<string>>> CreateOwnersCollection
            (IEnumerable<string> ownerIds, string endpoint)
        {
            IDictionary<int, IList<string>> collection = new Dictionary<int, IList<string>>();

            int lowPlayerId = 7719041;

            // Load JSON file for low lvl player into standard object representation
            var localFollowers =
                JsonConvert.DeserializeObject<IList<string>>
                (
                    File.ReadAllText($@"{_jsonFolder}\{lowPlayerId}.json")
                );

            foreach (var ownerId in ownerIds)
            {
                HtmlDocument ownerDoc =
                    await ExternalServices.Instance.GetHtmlAsync($@"{endpoint}/collection/{ownerId}");

                var followers = ownerDoc.DocumentNode.SelectNodes
                (
                    "//div[contains(@style, 'display:flex;flex-wrap: wrap;')]//div[@class='cre_mon_parent']/a"
                )
                .Select(n => n.Attributes["title"]?.Value)
                .ToList();

                collection.Add(ConvertToNumber(ownerId), followers);
            }

            // Add local collection for players whose lvl < 5
            collection.Add(lowPlayerId, localFollowers);

            return collection;
        }

        private async Task<CreatureStats> RetrieveCharacteristics(string url)
        {
            // Obtain creature detailed characteristics
            HtmlDocument htmlDoc = await ExternalServices.Instance.GetHtmlAsync(url);

            // Find creature stats and abilities respectively
            HtmlNodeCollection creatureStats =
                htmlDoc.DocumentNode.SelectNodes("//div[@class='scroll_content_half']//div");
            HtmlNodeCollection creatureSkills =
                htmlDoc.DocumentNode.SelectNodes("//div[@class='army_info_skills']/span");

            // Divide creature damage section into min and max parts
            string[] damageParts = creatureStats[4].InnerText.Split('-');

            // Compose characteristics object
            var characteristics = new CreatureStats()
            {
                Attack = ConvertToNumber(creatureStats[0].InnerText),
                Shots = ConvertToNumber(creatureStats[1].InnerText, nullable: true),
                Defence = ConvertToNumber(creatureStats[2].InnerText),
                Mana = ConvertToNumber(creatureStats[3].InnerText, nullable: true),
                MinDamage = ConvertToNumber(damageParts.FirstOrDefault()),
                MaxDamage = ConvertToNumber(damageParts.LastOrDefault()),
                Range = ConvertToNumber(creatureStats[5].InnerText, nullable: true),
                HitPoints = ConvertToNumber(creatureStats[6].InnerText),
                Initiative = ConvertToNumber(creatureStats[7].InnerText),
                Movement = ConvertToNumber(creatureStats[8].InnerText),
                Abilities = creatureSkills?.Count ?? 0,
            };

            return characteristics;
        }

        // Utility method to parse string into numeric value
        private int ConvertToNumber(string input, bool nullable = false)
        {
            int returnVal = default;

            if (nullable)
            {
                int.TryParse(input, out returnVal);
            }

            else
            {
                returnVal = int.Parse(input);
            }

            return returnVal;
        }

        // Method to check parse progress
        private void DisplayProcessStatus(int value, int limit)
        {
            if (value == limit)
            {
                Console.WriteLine("Parse progress: 25%");
            }

            if (value == limit * 2)
            {
                Console.WriteLine("Parse progress: 50%");
            }

            if (value == limit * 3)
            {
                Console.WriteLine("Parse progress: 75%");
            }
        }

        // Method to convert hex color rarity to enumerated 
        private Rarity GetFollowerRarity(string colorHex)
        {
            Rarity rarity = default;

            switch (colorHex.ToLower())
            {
                case "#fdd7a7":
                    rarity = Rarity.Mythical;
                    break;
                case "#f4e5b0":
                    rarity = Rarity.Legendary;
                    break;
                case "#9cb6d4":
                    rarity = Rarity.VeryRare;
                    break;
                case "#bea798":
                    rarity = Rarity.Rare;
                    break;
                case "#bfbfbf":
                    rarity = Rarity.Standard;
                    break;
                default:
                    rarity = Rarity.None;
                    break;
            }

            return rarity;
        }

        // Method to assign owners for every follower (if exists)
        private IList<int> SetOwners
        (
            IDictionary<int, IList<string>> collections,
            string follower
        )
        {
            IList<int> ownersList = new List<int>();

            foreach (var collection in collections)
            {
                if (collection.Value.Contains(follower))
                {
                    ownersList.Add(collection.Key);
                }
            }

            return ownersList;
        }
    }
}
