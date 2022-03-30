namespace SmartVotingAPI.Models.DTO
{
    public class Voter
    {
        public string VoterId { get; set; } = null!;
        public int RidingId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public DateTime BirthDate { get; set; }
        public int Gender { get; set; }
        public int StreetNumber { get; set; }
        public string StreetName { get; set; } = null!;
        public string? UnitNumber { get; set; }
        public string City { get; set; } = null!;
        public int ProvinceId { get; set; }
        public string PostCode { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
