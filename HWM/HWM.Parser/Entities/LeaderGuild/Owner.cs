using HWM.Parser.Interfaces;

namespace HWM.Parser.Entities.LeaderGuild
{
    public class Owner : IIdentity
    {
        public int Id { get; set; }

        public int LeaderGuildLvl { get; set; }
    }
}
