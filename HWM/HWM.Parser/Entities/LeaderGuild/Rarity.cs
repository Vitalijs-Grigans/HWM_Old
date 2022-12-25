using System.ComponentModel;

namespace HWM.Parser.Entities.LeaderGuild
{
    public enum Rarity
    {
        [Description("None")]
        None = -1,
        
        [Description("Mythical")]
        Mythical,

        [Description("Legendary")]
        Legendary,

        [Description("Very Rare")]
        VeryRare,

        [Description("Rare")]
        Rare,

        [Description("Standard")]
        Standard
    }
}
