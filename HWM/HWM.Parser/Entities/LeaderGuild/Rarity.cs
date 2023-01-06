using System.ComponentModel;

namespace HWM.Parser.Entities.LeaderGuild
{
    public enum Rarity
    {
        [Description("Пусто")]
        None = -1,
        
        [Description("Мифический")]
        Mythical,

        [Description("Легендарный")]
        Legendary,

        [Description("Очень редкий")]
        VeryRare,

        [Description("Редкий")]
        Rare,

        [Description("Обычный")]
        Standard
    }
}
