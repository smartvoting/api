namespace SmartVotingAPI.Models.DTO.Elections
{
    public class PastResult
    {
        public int ElectionId { get; set; }
        public int RidingId { get; set; }
        public int CandidateId { get; set; }
        public int TotalVotes { get; set; }
        public bool WasElected { get; set; }
    }
}
