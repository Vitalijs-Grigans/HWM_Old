using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HWM.Parser.Entities.LeaderGuild;
using HWM.Parser.Helpers;
using HWM.Parser.Interfaces;

namespace HWM.Parser
{
    public partial class LeaderGuildParser : IParser
    { 
        private double AssignTierModifier(double value, Rarity tier)
        {
            double result = default(double);
            
            switch (tier)
            {
                case Rarity.Mythical:
                    result = value / 0.8;
                    break;

                case Rarity.Legendary:
                    result = value / 0.85;
                    break;

                case Rarity.VeryRare:
                    result = value / 0.9;
                    break;

                case Rarity.Rare:
                    result = value / 0.95;
                    break;

                case Rarity.Standard:
                    result = value;
                    break;

                default:
                    break;
            }

            return result;
        }
        
        private double CalculateEfficiency(double absoluteStatValue, int leadership, Rarity tier)
        {
            double efficiency = absoluteStatValue / leadership;

            return AssignTierModifier(efficiency, tier);
        }

        private double CalculateEfficiency
        (
            double baseStatValue,
            double extraStatValue,
            double coefficient,
            int leadership, 
            Rarity tier
        )
        {
            double efficiency =
                baseStatValue * (1 + extraStatValue * coefficient) / leadership;
            
            return AssignTierModifier(efficiency, tier);
        }
        
        private IDictionary<string, double[]> GetAbsoluteEfficiency(IEnumerable<Follower> creatures)
        {
            foreach (var follower in creatures)
            {
                follower.Efficiency = new Rating()
                {
                    Absolute = new AbsoluteRating()
                    {
                        Attack =
                            CalculateEfficiency
                            (
                                follower.Characteristics.Attack,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Defence =
                            CalculateEfficiency
                            (
                                follower.Characteristics.Defence,
                                follower.Leadership,
                                follower.Tier
                            ),
                        MinDamage =
                            CalculateEfficiency
                            (
                                follower.Characteristics.MinDamage,
                                follower.Leadership,
                                follower.Tier
                            ),
                        MaxDamage =
                            CalculateEfficiency
                            (
                                follower.Characteristics.MaxDamage,
                                follower.Leadership,
                                follower.Tier
                            ),
                        HitPoints =
                            CalculateEfficiency
                            (
                                follower.Characteristics.HitPoints,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Movement =
                            CalculateEfficiency
                            (
                                follower.Characteristics.Movement,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Initiative =
                            CalculateEfficiency
                            (
                                follower.Characteristics.Initiative,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Abilities =
                            CalculateEfficiency
                            (
                                follower.Characteristics.Abilities,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Offense =
                            CalculateEfficiency
                            (
                                new double[]
                                {
                                    follower.Characteristics.MinDamage,
                                    follower.Characteristics.MaxDamage
                                }.Average(),
                                follower.Characteristics.Attack,
                                0.05d,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Survivability =
                            CalculateEfficiency
                            (
                                follower.Characteristics.HitPoints,
                                follower.Characteristics.Defence,
                                0.05d,
                                follower.Leadership,
                                follower.Tier
                            ),
                        Rush =
                            CalculateEfficiency
                            (
                                follower.Characteristics.Movement,
                                follower.Characteristics.Initiative,
                                0.1d,
                                follower.Leadership,
                                follower.Tier
                            )
                    }
                };
            }

            IEnumerable<Follower> mythical = creatures.Where(c => c.Tier == Rarity.Mythical);
            IEnumerable<Follower> legendary = creatures.Where(c => c.Tier == Rarity.Legendary);
            IEnumerable<Follower> veryRare = creatures.Where(c => c.Tier == Rarity.VeryRare);
            IEnumerable<Follower> rare = creatures.Where(c => c.Tier == Rarity.Rare);
            IEnumerable<Follower> standard = creatures.Where(c => c.Tier == Rarity.Standard);

            return new Dictionary<string, double[]>()
            {
                { 
                    "Attack",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Attack),
                        legendary.Max(c => c.Efficiency.Absolute.Attack),
                        veryRare.Max(c => c.Efficiency.Absolute.Attack),
                        rare.Max(c => c.Efficiency.Absolute.Attack),
                        standard.Max(c => c.Efficiency.Absolute.Attack),
                    }
                },
                { 
                    "Defence",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Defence),
                        legendary.Max(c => c.Efficiency.Absolute.Defence),
                        veryRare.Max(c => c.Efficiency.Absolute.Defence),
                        rare.Max(c => c.Efficiency.Absolute.Defence),
                        standard.Max(c => c.Efficiency.Absolute.Defence),
                    }
                },
                {
                    "MinDamage",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.MinDamage),
                        legendary.Max(c => c.Efficiency.Absolute.MinDamage),
                        veryRare.Max(c => c.Efficiency.Absolute.MinDamage),
                        rare.Max(c => c.Efficiency.Absolute.MinDamage),
                        standard.Max(c => c.Efficiency.Absolute.MinDamage),
                    }
                },
                {
                    "MaxDamage",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.MaxDamage),
                        legendary.Max(c => c.Efficiency.Absolute.MaxDamage),
                        veryRare.Max(c => c.Efficiency.Absolute.MaxDamage),
                        rare.Max(c => c.Efficiency.Absolute.MaxDamage),
                        standard.Max(c => c.Efficiency.Absolute.MaxDamage),
                    }
                },
                {
                    "HitPoints",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.HitPoints),
                        legendary.Max(c => c.Efficiency.Absolute.HitPoints),
                        veryRare.Max(c => c.Efficiency.Absolute.HitPoints),
                        rare.Max(c => c.Efficiency.Absolute.HitPoints),
                        standard.Max(c => c.Efficiency.Absolute.HitPoints),
                    }
                },
                {
                    "Movement",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Movement),
                        legendary.Max(c => c.Efficiency.Absolute.Movement),
                        veryRare.Max(c => c.Efficiency.Absolute.Movement),
                        rare.Max(c => c.Efficiency.Absolute.Movement),
                        standard.Max(c => c.Efficiency.Absolute.Movement),
                    }
                },
                {
                    "Initiative",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Initiative),
                        legendary.Max(c => c.Efficiency.Absolute.Initiative),
                        veryRare.Max(c => c.Efficiency.Absolute.Initiative),
                        rare.Max(c => c.Efficiency.Absolute.Initiative),
                        standard.Max(c => c.Efficiency.Absolute.Initiative),
                    }
                },
                {
                    "Abilities",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Abilities),
                        legendary.Max(c => c.Efficiency.Absolute.Abilities),
                        veryRare.Max(c => c.Efficiency.Absolute.Abilities),
                        rare.Max(c => c.Efficiency.Absolute.Abilities),
                        standard.Max(c => c.Efficiency.Absolute.Abilities),
                    }
                },
                {
                    "Offense",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Offense),
                        legendary.Max(c => c.Efficiency.Absolute.Offense),
                        veryRare.Max(c => c.Efficiency.Absolute.Offense),
                        rare.Max(c => c.Efficiency.Absolute.Offense),
                        standard.Max(c => c.Efficiency.Absolute.Offense),
                    }
                },
                {
                    "Survivability",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Survivability),
                        legendary.Max(c => c.Efficiency.Absolute.Survivability),
                        veryRare.Max(c => c.Efficiency.Absolute.Survivability),
                        rare.Max(c => c.Efficiency.Absolute.Survivability),
                        standard.Max(c => c.Efficiency.Absolute.Survivability),
                    }
                },
                {
                    "Rush",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Rush),
                        legendary.Max(c => c.Efficiency.Absolute.Rush),
                        veryRare.Max(c => c.Efficiency.Absolute.Rush),
                        rare.Max(c => c.Efficiency.Absolute.Rush),
                        standard.Max(c => c.Efficiency.Absolute.Rush),
                    }
                }
            };
        }

        private void GetRelativeEfficiency(IEnumerable<Follower> creatures, IDictionary<string, double[]> max)
        {
            foreach (var follower in creatures)
            {
                follower.Efficiency.Relative = new RelativeRating()
                {
                    Attack =
                        follower.Efficiency.Absolute.Attack / max["Attack"][(int)follower.Tier] * 100,
                    Defence =
                        follower.Efficiency.Absolute.Defence / max["Defence"][(int)follower.Tier] * 100,
                    MinDamage =
                        follower.Efficiency.Absolute.MinDamage / max["MinDamage"][(int)follower.Tier] * 100,
                    MaxDamage =
                        follower.Efficiency.Absolute.MaxDamage / max["MaxDamage"][(int)follower.Tier] * 100,
                    HitPoints =
                        follower.Efficiency.Absolute.HitPoints / max["HitPoints"][(int)follower.Tier] * 100,
                    Movement =
                        follower.Efficiency.Absolute.Movement / max["Movement"][(int)follower.Tier] * 100,
                    Initiative =
                        follower.Efficiency.Absolute.Initiative / max["Initiative"][(int)follower.Tier] * 100,
                    Abilities =
                        follower.Efficiency.Absolute.Abilities / max["Abilities"][(int)follower.Tier] * 100,
                    Offense =
                        follower.Efficiency.Absolute.Offense / max["Offense"][(int)follower.Tier] * 100,
                    Survivability =
                        follower.Efficiency.Absolute.Survivability / max["Survivability"][(int)follower.Tier] * 100,
                    Rush =
                        follower.Efficiency.Absolute.Rush / max["Rush"][(int)follower.Tier] * 100
                };
            }
        }

        private void GetFinalScore(IEnumerable<Follower> creatures)
        {
            foreach (var follower in creatures)
            {
                IEnumerable<double> efficiencyStore = new List<double>()
                {
                    follower.Efficiency.Relative.Attack,
                    follower.Efficiency.Relative.Defence,
                    follower.Efficiency.Relative.MinDamage,
                    follower.Efficiency.Relative.MaxDamage,
                    follower.Efficiency.Relative.HitPoints,
                    follower.Efficiency.Relative.Movement,
                    follower.Efficiency.Relative.Initiative,
                    follower.Efficiency.Relative.Abilities,
                    follower.Efficiency.Relative.Offense,
                    follower.Efficiency.Relative.Survivability,
                    follower.Efficiency.Relative.Rush
                };

                follower.Efficiency.Attack = (int)Math.Round(follower.Efficiency.Relative.Attack);
                follower.Efficiency.Defence = (int)Math.Round(follower.Efficiency.Relative.Defence);
                follower.Efficiency.MinDamage = (int)Math.Round(follower.Efficiency.Relative.MinDamage);
                follower.Efficiency.MaxDamage = (int)Math.Round(follower.Efficiency.Relative.MaxDamage);
                follower.Efficiency.HitPoints = (int)Math.Round(follower.Efficiency.Relative.HitPoints);
                follower.Efficiency.Movement = (int)Math.Round(follower.Efficiency.Relative.Movement);
                follower.Efficiency.Initiative = (int)Math.Round(follower.Efficiency.Relative.Initiative);
                follower.Efficiency.Abilities = (int)Math.Round(follower.Efficiency.Relative.Abilities);
                follower.Efficiency.Offense = (int)Math.Round(follower.Efficiency.Relative.Offense);
                follower.Efficiency.Survivability = (int)Math.Round(follower.Efficiency.Relative.Survivability);
                follower.Efficiency.Rush = (int)Math.Round(follower.Efficiency.Relative.Rush);
                follower.Efficiency.Overall = (int)Math.Round(efficiencyStore.Average());
            }
        }

        public async Task ProcessDataAsync()
        {
            IEnumerable<Follower> creatureList =
                await ExternalServices.Instance.LoadJsonAsync($@"{_jsonFolder}\LGCreatures.json");

            IDictionary<string, double[]> maxEfficiency = GetAbsoluteEfficiency(creatureList);
            GetRelativeEfficiency(creatureList, maxEfficiency);
            GetFinalScore(creatureList);

            IEnumerable<Follower> followers = creatureList.OrderByDescending(c => c.Efficiency.Overall)
                                                    .ThenBy(c => c.Tier)
                                                    .ThenBy(c => c.DisplayName)
                                                    .ToList();

            await ExternalServices.Instance.SaveJsonAsync(followers, $@"{_jsonFolder}\LGCreatures_ext.json");
        }
    }
}
