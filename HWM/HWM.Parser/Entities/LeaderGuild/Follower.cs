using HWM.Parser.Entities.Creature;
using HWM.Parser.Interfaces;

namespace HWM.Parser.Entities.LeaderGuild
{
    public class Follower : CreatureInfo, IIdentity
    {
        public string Tier { get; set; }

        public int Leadership { get; set; }

        public Rating Efficiency { get; set; }
    }
}
