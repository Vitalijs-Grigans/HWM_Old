using System.Runtime.Serialization;

namespace HWM.Parser.Entities.LeaderGuild
{
    [DataContract]
    public class Rating
    {
        public AbsoluteRating Absolute { get; set; }

        public RelativeRating Relative { get; set; }

        [DataMember]
        public ScoreRating Score { get; set; }
    }
}
