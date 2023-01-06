using System.Runtime.Serialization;

namespace HWM.Parser.Entities.LeaderGuild
{
    [DataContract]
    public class Rating
    {
        public AbsoluteRating Absolute { get; set; }

        public RelativeRating Relative { get; set; }

        [DataMember]
        public int Attack { get; set; }

        [DataMember]
        public int Defence { get; set; }

        [DataMember]
        public int MinDamage { get; set; }

        [DataMember]
        public int MaxDamage { get; set; }

        [DataMember]
        public int HitPoints { get; set; }

        [DataMember]
        public int Movement { get; set; }

        [DataMember]
        public int Initiative { get; set; }

        [DataMember]
        public int Abilities { get; set; }

        [DataMember]
        public int Offense { get; set; }

        [DataMember]
        public int Survivability { get; set; }

        [DataMember]
        public int Rush { get; set; }

        [DataMember]
        public int Overall { get; set; }
    }
}
