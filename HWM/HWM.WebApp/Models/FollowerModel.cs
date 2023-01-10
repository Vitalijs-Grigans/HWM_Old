namespace HWM.WebApp.Models
{
    public class FollowerModel
    {
        public int Id { get; set; }

        public string? Url { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public int Tier { get; set; }

        public string? DisplayTier { get; set; }

        public int Leadership { get; set; }

        public RatingModel? Efficiency { get; set; }
    }
}
