namespace SmartVotingAPI.Models.DTO
{
    public class Office
    {
        public string Type { get; set; } = null!;
        public string? StreetNumber { get; set; }
        public string? StreetName { get; set; }
        public string? UnitNumber { get; set; }
        public string City { get; set; } = null!;
        public string? Province { get; set; }
        public string PostCode { get; set; } = null!;
        public string? PoBox { get; set; }
        public bool IsPublic { get; set; }
    }
}
