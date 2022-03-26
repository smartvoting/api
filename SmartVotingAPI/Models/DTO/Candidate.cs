namespace SmartVotingAPI.Models.DTO
{
    public class Candidate
    {
        public int CandidateId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public int PartyId { get; set; }
        public string PartyName { get; set; } = null!;
        public int RidingId { get; set; }
        public string RidingName { get; set; } = null!;
    }
}
