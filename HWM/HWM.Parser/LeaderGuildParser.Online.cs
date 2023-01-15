using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

using HtmlAgilityPack;

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
        private string _endpoint;
        private IList<string> _owners;
        private string _jsonFolder;
        private string _imageFolder;

        // Method to convert hex color rarity to enumerated 
        private static Rarity GetFollowerRarity(string colorHex)
        {
            Rarity rarity = default(Rarity);

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

        // Utility method to parse string data
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
        
        public LeaderGuildParser(IDictionary<string, string> config)
        {
            _endpoint = config["LeaderGuildEndpoint"];
            _owners = config["CreatureOwnersList"].Split(',');
            _jsonFolder = config["ParseResultsFolder"];
            _imageFolder = config["CreatureImageFolder"];
        }
        
        // Asynchronous method to fetch required data from external service
        public async Task CollectDataAsync()
        {
            // Obtain creature names for each owner
            IDictionary<int, IList<string>> ownerCollection = new Dictionary<int, IList<string>>();

            foreach (var owner in _owners)
            {
                HtmlDocument ownerDoc =
                    await ExternalServices.Instance.GetHtmlAsync($@"{_endpoint}/collection/{owner}");
                HtmlNode ownerBody = ownerDoc.DocumentNode.SelectSingleNode("//body");
                var collection = ownerBody.SelectNodes
                (
                    "//div[contains(@style, 'display:flex;flex-wrap: wrap;')]//div[@class='cre_mon_parent']/a"
                )
                .Select(n => n.Attributes["title"]?.Value)
                .ToList();
                
                ownerCollection.Add(ConvertToNumber(owner), collection);
            }
            
            // Obtain creature links to external site by traversing DOM tree
            HtmlDocument htmlDoc =
                await ExternalServices.Instance.GetHtmlAsync($@"{_endpoint}/leader.php");
            HtmlNode body = htmlDoc.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection creatureNodes = 
                body.SelectNodes("//div[@class='fcont']//div[@class='cre_mon_parent']/a");

            IList<Follower> creatureList = new List<Follower>();
            int id = default;

            foreach (var anchor in creatureNodes)
            {
                Console.Clear();
                
                // Creature color background represented by hex code
                string backgroundStyle =
                    anchor.ParentNode.ParentNode.ParentNode.Attributes["style"]?.Value;

                // Url to creature details
                string url = anchor.Attributes["href"]?.Value;

                // Creature name used by system and display respectively
                string name = url.Split('=').LastOrDefault();
                string displayName = anchor.Attributes["title"]?.Value;

                // Url to creature image
                string imageUrl = anchor.ChildNodes.LastOrDefault().Attributes["src"]?.Value;

                // Find creature image (if exists), otherwise - download it
                string file = $"{name}.png";
                string cachedImage = Directory.GetFiles(_imageFolder, file).FirstOrDefault();

                if (cachedImage == null)
                {
                    await ExternalServices.Instance.DownloadImageAsync(imageUrl, $@"{_imageFolder}\{file}");
                }

                // Obtain creature characteristics by traversing DOM tree
                HtmlDocument creatureDoc = await ExternalServices.Instance.GetHtmlAsync(url);
                HtmlNode creatureBody = creatureDoc.DocumentNode.SelectSingleNode("//body");

                // Find creature stats and abilities respectively
                HtmlNodeCollection creatureStats = 
                    creatureBody.SelectNodes("//div[@class='scroll_content_half']//div");
                HtmlNodeCollection creatureSkills =
                    creatureBody.SelectNodes("//div[@class='army_info_skills']/span");

                // Divide creature damage section into min and max parts
                string[] damageParts = creatureStats[4].InnerText.Split('-');

                // Get creature rank to enumerated value
                Rarity tier = GetFollowerRarity(backgroundStyle.Split(':').LastOrDefault());

                // Save creature info and characteristics and add to collection
                var follower = new Follower()
                {
                    Id = id++,
                    Url = url,
                    Name = name,
                    DisplayName = displayName,
                    Characteristics = new CreatureStats()
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
                    },
                    Tier = tier,
                    DisplayTier = tier.GetDisplayName(),
                    Leadership = ConvertToNumber(creatureStats[9].InnerText.Replace(",", string.Empty)),
                    Owners = SetOwners(ownerCollection, displayName)
                };

                creatureList.Add(follower);

                // Display parse progress info
                Console.WriteLine($"Processed {id} out of {creatureNodes.Count}");
            }

            // Store creature data into JSON file
            await ExternalServices.Instance.SaveJsonAsync(creatureList, $@"{_jsonFolder}\LGCreatures.json");
        }
    }
}
