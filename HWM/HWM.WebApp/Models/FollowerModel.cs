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

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public IList<PoolModel> Pools { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public RatingModel? Efficiency { get; set; }

        public int? ActivePoolId { get; set; }
    }
}
