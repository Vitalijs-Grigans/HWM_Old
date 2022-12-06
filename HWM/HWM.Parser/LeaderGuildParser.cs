using HtmlAgilityPack;

using HWM.Parser.Interfaces;
using HWM.Parser.Entities;

using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.IO;
using HWM.Parser.Mappers;

namespace HWM.Parser
{
    public class LeaderGuildParser : IParser
    {
        public void CollectData()
        {
            string webpage = "https://daily.heroeswm.ru/leader/leader.php";
            var htmlDoc = new HtmlDocument();

            using (WebClient client = new WebClient())
            {
                string html = client.DownloadString(webpage);

                htmlDoc.LoadHtml(html);
            }

            HtmlNode body = htmlDoc.DocumentNode.SelectSingleNode("//body");
            HtmlNodeCollection creatureNodes = body.SelectNodes("//div[@class='cre_mon_parent']/a");

            IList<CreatureEntity> creatureList = new List<CreatureEntity>();

            int id = 0;

            foreach (var anchor in creatureNodes)
            {
                var creatureDoc = new HtmlDocument();

                HtmlNode backgroundStyleNode = anchor.ParentNode.ParentNode.ParentNode;
                string backgroundStyle = backgroundStyleNode.Attributes["style"].Value;
                string[] backgroundArr = backgroundStyle.Split(':');
                string background = backgroundArr[backgroundArr.Length - 1];

                Console.WriteLine("Background: " + background);

                string url = anchor.Attributes["href"].Value;

                Console.WriteLine("Url: " + url);

                using (WebClient client = new WebClient())
                {
                    string html = client.DownloadString(url);

                    creatureDoc.LoadHtml(html);
                }

                HtmlNode creatureBody = creatureDoc.DocumentNode.SelectSingleNode("//body");

                // Mapping for Russian text
                var nameParts = url.Split('=');
                string name = CreatureNameMapper.Map(nameParts[nameParts.Length - 1]);

                HtmlNodeCollection creatureStats = creatureBody.SelectNodes("//div[@class='scroll_content_half']//div");
                int attack = int.Parse(creatureStats[0].InnerText);

                int shots;
                int.TryParse(creatureStats[1].InnerText, out shots);

                int defence = int.Parse(creatureStats[2].InnerText);

                int mana;
                int.TryParse(creatureStats[3].InnerText, out mana);

                string[] damageParts = creatureStats[4].InnerText.Split('-');
                int minDamage = int.Parse(damageParts[0]);
                int maxDamage = int.Parse(damageParts[damageParts.Length - 1]);

                int range;
                int.TryParse(creatureStats[5].InnerText, out range);

                int hitPoints = int.Parse(creatureStats[6].InnerText);
                int initiative = int.Parse(creatureStats[7].InnerText);
                int movement = int.Parse(creatureStats[8].InnerText);
                int leadership = int.Parse(creatureStats[9].InnerText.Replace(",", string.Empty));

                var creature = new CreatureEntity()
                {
                    Id = id++,
                    Background = background,
                    Url = url,
                    Name = name,
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

                Console.WriteLine("Processed " + id + " creatures...");
                Console.WriteLine();
            }

            var json = JsonConvert.SerializeObject(creatureList, Formatting.Indented);

            File.WriteAllText(@"D:\Database\HWM.Leader\02-12-2022\LGCreatures.json", json);
        }
    }
}
