namespace SmartVotingAPI.Models.DTO.Elections
{
    public class PastCandidate
    {
        public int CandidateId { get; set; }
        public int PartyId { get; set; }
        public int RidingId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
