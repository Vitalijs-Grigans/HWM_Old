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
        public async Task ProcessDataAsync()
        {
            // Load JSON file into existing object representation
            IEnumerable<Follower> creatureList =
                await GenericExternalServices<IEnumerable<Follower>>.Instance.LoadJsonAsync
                (
                    $@"{_jsonFolder}\LGCreatures.json"
                );

            // Get creature raw efficiency for each characteristics
            IDictionary<string, double[]> maxEfficiency = GetAbsoluteEfficiency(creatureList);

            // Get creature efficiency against best value for each tier type
            GetRelativeEfficiency(creatureList, maxEfficiency);

            // Get creature overall and each characteristics rating 
            GetFinalScore(creatureList);

            // Apply ordering to collection
            IEnumerable<Follower> followers = creatureList.OrderByDescending(c => c.Efficiency.Overall)
                                                    .ThenBy(c => c.Tier)
                                                    .ThenBy(c => c.DisplayName)
                                                    .ToList();

            // Store creature data into JSON file
            await GenericExternalServices<IEnumerable<Follower>>.Instance.SaveJsonAsync
            (
                followers, 
                $@"{_jsonFolder}\LGCreatures_ext.json"
            );
        }

        // Method for applying bonus for strongest tier creatures 
        private double AssignTierModifier(double value, Rarity tier)
        {
            double result = default(double);

            // Apply bonus based on creature tier
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

        //Method 1 to calculate creature absolute efficiency for allsimple characteristics
        private double CalculateEfficiency(double absoluteStatValue, int leadership, Rarity tier)
        {
            double efficiency = absoluteStatValue / leadership;

            return AssignTierModifier(efficiency, tier);
        }

        //Method 2 to calculate creature absolute efficiency for all complex characteristics
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

        // Method for calculating creature raw efficiency for all characteristics
        private IDictionary<string, double[]> GetAbsoluteEfficiency(IEnumerable<Follower> creatures)
        {
            foreach (var follower in creatures)
            {
                double avgDamage = new double[]
                    {
                        follower.Characteristics.MinDamage,
                        follower.Characteristics.MaxDamage
                    }
                    .Average();

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
                        Damage =
                            CalculateEfficiency
                            (
                                avgDamage,
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
                                avgDamage,
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

            // Filter creatures by tier type
            IEnumerable<Follower> mythical = creatures.Where(c => c.Tier == Rarity.Mythical);
            IEnumerable<Follower> legendary = creatures.Where(c => c.Tier == Rarity.Legendary);
            IEnumerable<Follower> veryRare = creatures.Where(c => c.Tier == Rarity.VeryRare);
            IEnumerable<Follower> rare = creatures.Where(c => c.Tier == Rarity.Rare);
            IEnumerable<Follower> standard = creatures.Where(c => c.Tier == Rarity.Standard);

            // Extract best efficiency results for each characteristics and tier type
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
                    "Damage",
                    new double[]
                    {
                        mythical.Max(c => c.Efficiency.Absolute.Damage),
                        legendary.Max(c => c.Efficiency.Absolute.Damage),
                        veryRare.Max(c => c.Efficiency.Absolute.Damage),
                        rare.Max(c => c.Efficiency.Absolute.Damage),
                        standard.Max(c => c.Efficiency.Absolute.Damage),
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

        // Method for calculating creature overall and each characteristics rating 
        private void GetFinalScore(IEnumerable<Follower> creatures)
        {
            foreach (var follower in creatures)
            {
                IEnumerable<double> efficiencyStore = new List<double>()
                {
                    follower.Efficiency.Relative.Attack,
                    follower.Efficiency.Relative.Defence,
                    follower.Efficiency.Relative.Damage,
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
                follower.Efficiency.Damage = (int)Math.Round(follower.Efficiency.Relative.Damage);
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

        // Method for calculating creature efficiency based on best value and tier type
        private void GetRelativeEfficiency
        (
            IEnumerable<Follower> creatures,
            IDictionary<string, double[]> max
        )
        {
            foreach (var follower in creatures)
            {
                follower.Efficiency.Relative = new RelativeRating()
                {
                    Attack =
                        follower.Efficiency.Absolute.Attack / max["Attack"][(int)follower.Tier] * 100,
                    Defence =
                        follower.Efficiency.Absolute.Defence / max["Defence"][(int)follower.Tier] * 100,
                    Damage =
                        follower.Efficiency.Absolute.Damage / max["Damage"][(int)follower.Tier] * 100,
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
    }
}