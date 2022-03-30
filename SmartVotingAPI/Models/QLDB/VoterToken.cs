namespace SmartVotingAPI.Models.QLDB
{
    public class VoterToken
    {
        public string VoterId { get; set; }
        public int RidingId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
