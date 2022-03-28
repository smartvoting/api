namespace SmartVotingAPI.Models.DTO.Elections
{
    public class PastTurnout
    {
        public int ElectionId { get; set; }
        public int RidingId { get; set; }
        public int ValidVotes { get; set; }
        public int InvalidVotes { get; set; }
        public int EligibleVoters { get; set; }
    }
}
