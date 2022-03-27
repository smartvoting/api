namespace SmartVotingAPI.Models.DTO.Vote
{
    public class VoteToken
    {
        public int CandidateId { get; set; }
        public int RidingId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
