namespace SmartVotingAPI.Models.DTO
{
    public class Person
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public Office? Office { get; set; }
        public SocialMedia? SocialMedia { get; set; }
    }
}
