namespace SmartVotingAPI.Models.DTO.Elections
{
    public class Election
    {
        public int ElectionYear { get; set; }
        public string ElectionType { get; set; }
        public DateTime ElectionDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Notes { get; set; }
    }
}
