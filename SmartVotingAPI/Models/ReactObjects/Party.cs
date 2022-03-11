namespace SmartVotingAPI.Models.ReactObjects
{
    public class Party
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Domain { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FaxNumber { get; set; }
        public bool IsRegistered { get; set; }
        public string? DeregisterReason { get; set; }
        public DateTime Updated { get; set; }
        public Office? Office { get; set; }
        public SocialMedia? SocialMedia { get; set; }
        public Person? PartyLeader { get; set; }
        public Person[]? Candidates { get; set; }
    }
}
