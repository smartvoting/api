namespace SmartVotingAPI.Models.QLDB
{
    public class BallotToken
    {
        public int CandidateId { get; set; }
        public int RidingId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
