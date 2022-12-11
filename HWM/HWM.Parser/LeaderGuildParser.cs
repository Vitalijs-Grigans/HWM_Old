using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using HtmlAgilityPack;
using Newtonsoft.Json;

using HWM.Parser.Interfaces;
using HWM.Parser.Entities;
using HWM.Parser.Mappers;

namespace HWM.Parser
{
    public class LeaderGuildParser : IParser
    {
        private string _endpoint;
        private string _jsonFolder;
        private string _imageFolder;

        private HtmlDocument GetLocalHtml(string url)
        {
            var doc = new HtmlDocument();

            using (WebClient client = new WebClient())
            {
                string html = client.DownloadString(url);

                doc.LoadHtml(html);
            }

            return doc;
        }

        private int ConvertToNumber(string input, bool nullable = false) 
        {
            int returnVal;

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

        private void DownloadFileFromUrl(string url, string fileName)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile(new Uri(url), fileName);
            }
        }

        public LeaderGuildParser() { }
        
        public LeaderGuildParser(string endpoint, string jsonFolder, string imageFolder)
        {
            _endpoint = endpoint;
            _jsonFolder = jsonFolder;
            _imageFolder = imageFolder;
        }
        
        public void CollectData()
        {
            HtmlDocument htmlDoc = GetLocalHtml(_endpoint);

            HtmlNode body = htmlDoc.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection creatureNodes = 
                body.SelectNodes("//div[@class='fcont']//div[@class='cre_mon_parent']/a");

            IList<CreatureEntity> creatureList = new List<CreatureEntity>();

            int id = 0;

            foreach (var anchor in creatureNodes)
            {
                string backgroundStyle =
                    anchor.ParentNode.ParentNode.ParentNode.Attributes["style"].Value;
                string background = backgroundStyle.Split(':').LastOrDefault();

                string url = anchor.Attributes["href"].Value;

                Console.WriteLine($"Background: {background}");
                Console.WriteLine($"Url: {url}");

                HtmlDocument creatureDoc = GetLocalHtml(url);
                HtmlNode creatureBody = creatureDoc.DocumentNode.SelectSingleNode("//body");

                string name = url.Split('=').LastOrDefault();
                string displayName = CreatureMapper.Map(name);

                HtmlNodeCollection creatureStats = 
                    creatureBody.SelectNodes("//div[@class='scroll_content_half']//div");

                int attack = ConvertToNumber(creatureStats[0].InnerText);
                int shots = ConvertToNumber(creatureStats[1].InnerText, nullable: true);
                int defence = ConvertToNumber(creatureStats[2].InnerText);
                int mana = ConvertToNumber(creatureStats[3].InnerText, nullable: true);

                string[] damageParts = creatureStats[4].InnerText.Split('-');
                int minDamage = ConvertToNumber(damageParts.FirstOrDefault());
                int maxDamage = ConvertToNumber(damageParts.LastOrDefault());

                int range = ConvertToNumber(creatureStats[5].InnerText, nullable: true);
                int hitPoints = ConvertToNumber(creatureStats[6].InnerText);
                int initiative = ConvertToNumber(creatureStats[7].InnerText);
                int movement = ConvertToNumber(creatureStats[8].InnerText);
                int leadership =
                    ConvertToNumber(creatureStats[9].InnerText.Replace(",", string.Empty));

                var creature = new CreatureEntity()
                {
                    Id = id++,
                    Background = background,
                    Url = url,
                    Name = name,
                    DisplayName = displayName,
                    Attack = attack,
                    Shots = shots,
                    Defence = defence,
                    Mana = mana,
                    MinDamage = minDamage,
                    MaxDamage = maxDamage,
                    Range = range,
                    HitPoints = hitPoints,
                    Initiative = initiative,
                    Movement = movement,
                    Leadership = leadership
                };

                creatureList.Add(creature);

                string imageUrl = anchor.ChildNodes.LastOrDefault().Attributes["src"].Value;
                string file = $"{name}.png";
                string cachedImage = Directory.GetFiles(_imageFolder, file).FirstOrDefault();

                if (cachedImage != null) 
                {
                    Console.WriteLine($"{file} is cached already");
                }
                
                else
                {
                    DownloadFileFromUrl(imageUrl, $@"{_imageFolder}\{file}");
                    Console.WriteLine($"{file} has been downloaded");
                }

                Console.WriteLine("Processed " + id + " creatures...");
                Console.WriteLine();
            }

            var json = JsonConvert.SerializeObject(creatureList, Formatting.Indented);
            File.WriteAllText($@"{_jsonFolder}\LGCreatures.json", json);
        }
    }
}
