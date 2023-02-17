using System.Collections.Generic;

using HWM.Parser.Entities.Creature;
using HWM.Parser.Interfaces;

namespace HWM.Parser.Entities.LeaderGuild
{
    public class Follower : CreatureInfo, IIdentity
    {
        public Rarity Tier { get; set; }

        public string DisplayTier { get; set; }

        public int Leadership { get; set; }

        public IEnumerable<int> Owners { get; set; }

        public Rating Efficiency { get; set; }
    }
}
