using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using HWM.Parser.Entities.LeaderGuild;
using HWM.Parser.Helpers;
using HWM.Parser.Interfaces;

namespace HWM.Parser
{
    public partial class LeaderGuildParser : IParser
    { 
        private IDictionary<string, double> CalculateAbsoluteEfficiency(IList<Follower> creatures)
        {
            foreach (var follower in creatures)
            {
                follower.Efficiency = new Rating()
                {
                    Absolute = new AbsoluteRating()
                    {
                        Attack = (double)follower.Characteristics.Attack / follower.Leadership,
                        Defence = (double)follower.Characteristics.Defence / follower.Leadership,
                        MinDamage = (double)follower.Characteristics.MinDamage / follower.Leadership,
                        MaxDamage = (double)follower.Characteristics.MaxDamage / follower.Leadership,
                        HitPoints = (double)follower.Characteristics.HitPoints / follower.Leadership,
                        Movement = (double)follower.Characteristics.Movement / follower.Leadership,
                        Initiative = (double)follower.Characteristics.Initiative / follower.Leadership,
                        Offense =
                            (double)
                            ((follower.Characteristics.MinDamage + follower.Characteristics.MaxDamage) / 2 *
                            (1 + follower.Characteristics.Attack * 0.05) /
                            follower.Leadership),
                        Survivability =
                            (double)
                            (follower.Characteristics.HitPoints *
                            (1 + follower.Characteristics.Defence * 0.05) /
                            follower.Leadership),
                        Rush =
                            (double)
                            (follower.Characteristics.Movement *
                            (1 + follower.Characteristics.Initiative * 0.1) /
                            follower.Leadership)
                    }
                };
            }

            return new Dictionary<string, double>()
            {
                { "Attack", creatures.Max(c => c.Efficiency.Absolute.Attack) },
                { "Defence", creatures.Max(c => c.Efficiency.Absolute.Defence) },
                { "MinDamage", creatures.Max(c => c.Efficiency.Absolute.MinDamage) },
                { "MaxDamage", creatures.Max(c => c.Efficiency.Absolute.MaxDamage) },
                { "HitPoints", creatures.Max(c => c.Efficiency.Absolute.HitPoints) },
                { "Movement", creatures.Max(c => c.Efficiency.Absolute.Movement) },
                { "Initiative", creatures.Max(c => c.Efficiency.Absolute.Initiative) },
                { "Offense", creatures.Max(c => c.Efficiency.Absolute.Offense) },
                { "Survivability", creatures.Max(c => c.Efficiency.Absolute.Survivability) },
                { "Rush", creatures.Max(c => c.Efficiency.Absolute.Rush) }
            };
        }

        private void CalculateRelativeEfficiency(IList<Follower> creatures, IDictionary<string, double> max)
        {
            foreach (var follower in creatures)
            {
                follower.Efficiency.Relative = new RelativeRating()
                {
                    Attack = follower.Efficiency.Absolute.Attack / max["Attack"] * 100,
                    Defence = follower.Efficiency.Absolute.Defence / max["Defence"] * 100,
                    MinDamage = follower.Efficiency.Absolute.MinDamage / max["MinDamage"] * 100,
                    MaxDamage = follower.Efficiency.Absolute.MaxDamage / max["MaxDamage"] * 100,
                    HitPoints = follower.Efficiency.Absolute.HitPoints / max["HitPoints"] * 100,
                    Movement = follower.Efficiency.Absolute.Movement / max["Movement"] * 100,
                    Initiative = follower.Efficiency.Absolute.Initiative / max["Initiative"] * 100,
                    Offense = follower.Efficiency.Absolute.Offense / max["Offense"] * 100,
                    Survivability = follower.Efficiency.Absolute.Survivability / max["Survivability"] * 100,
                    Rush = follower.Efficiency.Absolute.Rush / max["Rush"] * 100
                };
            }
        }

        private void CalculateFinalScore(IList<Follower> creatures)
        {
            foreach (var follower in creatures)
            {
                IList<double> efficiencyStore = new List<double>()
                {
                    follower.Efficiency.Relative.Attack,
                    follower.Efficiency.Relative.Defence,
                    follower.Efficiency.Relative.MinDamage,
                    follower.Efficiency.Relative.MaxDamage,
                    follower.Efficiency.Relative.HitPoints,
                    follower.Efficiency.Relative.Movement,
                    follower.Efficiency.Relative.Initiative,
                    follower.Efficiency.Relative.Offense,
                    follower.Efficiency.Relative.Survivability,
                    follower.Efficiency.Relative.Rush
                };

                follower.Efficiency.Score = new ScoreRating()
                {
                    Attack = (int)Math.Round(follower.Efficiency.Relative.Attack),
                    Defence = (int)Math.Round(follower.Efficiency.Relative.Defence),
                    MinDamage = (int)Math.Round(follower.Efficiency.Relative.MinDamage),
                    MaxDamage = (int)Math.Round(follower.Efficiency.Relative.MaxDamage),
                    HitPoints = (int)Math.Round(follower.Efficiency.Relative.HitPoints),
                    Movement = (int)Math.Round(follower.Efficiency.Relative.Movement),
                    Initiative = (int)Math.Round(follower.Efficiency.Relative.Initiative),
                    Offense = (int)Math.Round(follower.Efficiency.Relative.Offense),
                    Survivability = (int)Math.Round(follower.Efficiency.Relative.Survivability),
                    Rush = (int)Math.Round(follower.Efficiency.Relative.Rush),
                    Overall = (int)Math.Round(efficiencyStore.Average())
                };
            }
        }

        public void ProcessData()
        {
            string json = File.ReadAllText($@"{_jsonFolder}\LGCreatures.json");
            List<Follower> creatureList = JsonSerializer.Deserialize<List<Follower>>(json);

            IDictionary<string, double> maxEfficiency = CalculateAbsoluteEfficiency(creatureList);
            CalculateRelativeEfficiency(creatureList, maxEfficiency);
            CalculateFinalScore(creatureList);

            FileStoreHelper.SaveJsonFile(creatureList, $@"{_jsonFolder}\LGCreatures_ext.json");
        }
    }
}
