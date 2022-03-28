namespace SmartVotingAPI.Models.DTO.Elections
{
    public class PastElection
    {
        public int ElectionId { get; set; }
        public int ElectionYear { get; set; }
        public string ElectionType { get; set; }
        public string ElectionDate { get; set; }
        public int? ValidVotes { get; set; }
        public int? InvalidVotes { get; set; }
        public int? EligibleVoters { get; set; }
    }
}
