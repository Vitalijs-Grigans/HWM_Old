using System.Collections.Generic;

namespace HWM
{
    public class Settings
    {
        public string LeaderGuildEndpoint { get; set; }

        public string CharacterProgressEndpoint { get; set; }

        public IList<string> CreatureOwnersList { get; set; }

        public string ParseResultsFolder { get; set; }

        public string CreatureImageFolder { get; set; }
    }
}
