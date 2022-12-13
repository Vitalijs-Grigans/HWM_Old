using HWM.Parser.Interfaces;

namespace HWM.Parser.Entities.Creature
{
    public class CreatureInfo : IIdentity
    {
        public int Id { get; set; }

        public string Url { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public CreatureStats Characteristics { get; set; }
    }
}
