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
            _endpointLG = config["LeaderGuildEndpoint"];
            _endpointCP = config["CharacterProgressEndpoint"];
            _ownerIds = config["CreatureOwnersList"].Split(',').Select(int.Parse).ToList();
            _jsonFolder = config["ParseResultsFolder"];
            _imageFolder = config["CreatureImageFolder"];
        }

        // Asynchronous method to fetch required data from external service
        public async Task CollectDataAsync()
        {
            // Obtain existing creature info from local JSON file
            IEnumerable<Follower> cachedCreatures =
                await GenericExternalServices<IEnumerable<Follower>>.Instance.LoadJsonAsync
                (
                    $@"{_jsonFolder}\LGCreatures.json"
                );

            // Obtain follower names for each corresponding owner
            IDictionary<int, string[,]> ownersCollection =
                await CreateOwnersCollection(_ownerIds, _endpointLG);

            // Obtain basic creature data containers
            HtmlDocument htmlDoc =
                await ExternalServices.Instance.GetHtmlAsync($@"{_endpointLG}/leader.php");
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

                if (existingFollower == null)
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

                follower.Pools = SetPools
                (
                    ownersCollection,
                    displayName,
                    follower.Characteristics.HitPoints,
                    follower.Leadership
                );

                creatureList.Add(follower);

                DisplayProcessStatus(follower.Id, quarter);
            }

            // Store creature data into JSON file
            await GenericExternalServices<IEnumerable<Follower>>.Instance.SaveJsonAsync
            (
                creatureList,
                $@"{_jsonFolder}\LGCreatures.json"
            );
        }

        // Async method to check/update owners guild values
        public async Task EnsureOwnersStatusAsync()
        {
            IList<Owner> owners = new List<Owner>();
            
            foreach (var id in _ownerIds)
            {
                // Obtain owner guild dataset
                HtmlDocument htmlDoc =
                    await ExternalServices.Instance.GetHtmlAsync($@"{_endpointCP}/{id}");

                int leaderGuildLvl = ConvertToNumber
                (
                    htmlDoc.DocumentNode.SelectSingleNode
                    (
                        "(//table[@class='report']//tr)[last()]//b"
                    )
                    .InnerText
                );

                var owner = new Owner()
                {
                    Id = id,
                    LeaderGuildLvl = leaderGuildLvl
                };

                owners.Add(owner);
            }

            // Add low lvl player guild info (should be removed in future)
            owners.Add(new Owner() { Id = 7719041, LeaderGuildLvl = 0 });

            _owners = owners;
        }

        private string _endpointLG;
        private string _endpointCP;
        private IEnumerable<int> _ownerIds;
        private string _jsonFolder;
        private string _imageFolder;

        private IEnumerable<Owner> _owners;

        // Async method to obtain followers for each owner
        private async Task<IDictionary<int, string[,]>> CreateOwnersCollection
            (IEnumerable<int> ownerIds, string endpoint)
        {
            var collection = new Dictionary<int, string[,]>();

            string commonXPath =
                "//div[contains(@style, 'display:flex;flex-wrap: wrap;')]//div[@class='cre_mon_parent']";
            string nameXPath = "/a";
            string countXPath = "/div[@class='cre_amount']";

            int lowPlayerId = 7719041;

            // Load JSON file for low lvl player into standard object representation
            var localFollowers =
                JsonConvert.DeserializeObject<string[,]>
                (
                    File.ReadAllText($@"{_jsonFolder}\{lowPlayerId}.json")
                );

            foreach (int ownerId in ownerIds)
            {
                HtmlDocument ownerDoc =
                    await ExternalServices.Instance.GetHtmlAsync($@"{endpoint}/collection/{ownerId}");

                var nameArray = 
                    ownerDoc.DocumentNode.SelectNodes($"{commonXPath}{nameXPath}")
                                         .Select(n => n.Attributes["title"]?.Value)
                                         .ToArray();

                var countArray =
                    ownerDoc.DocumentNode.SelectNodes($"{commonXPath}{countXPath}")
                                         .Select(c => c.InnerText)
                                         .ToArray();

                collection.Add
                (
                    ownerId,
                    DataTypeConvertor<string>.ConvertTo2DArray(nameArray, countArray)
                );
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

        // Method to assign owners and extra info for every follower (if exists)
        private IList<Pool> SetPools
        (
            IDictionary<int, string[,]> collections,
            string follower,
            int hitPoints,
            int followersLeadership
        )
        {
            IList<Pool> poolsList = new List<Pool>();

            foreach (var collection in collections)
            {
                int ownerlvlLG = _owners.FirstOrDefault(o => o.Id == collection.Key).LeaderGuildLvl;
                int allowedCount =
                    (int)Math.Floor
                    (
                        0.4 * ((ownerlvlLG < 10) ? (10000 + ownerlvlLG * 1000) : 20000) / followersLeadership
                    );
                
                for (var i = 0; i < collection.Value.GetLength(0); i++)
                {
                    if (collection.Value[i, 0] == follower)
                    {
                        int inStock = ConvertToNumber(collection.Value[i, 1]);
                        allowedCount = (inStock < allowedCount) ? inStock : allowedCount;

                        var pool = new Pool()
                        {
                            OwnerId = collection.Key,
                            InStock = inStock,
                            AllowedCount = allowedCount,
                            AllowedHP = allowedCount * hitPoints
                        };

                        poolsList.Add(pool);
                    }
                }
            }

            return poolsList;
        }
    }
}
