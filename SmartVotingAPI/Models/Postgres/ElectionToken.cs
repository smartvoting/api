namespace SmartVotingAPI.Models.Postgres
{
    public class ElectionToken
    {
        public Guid PublicId { get; set; }
        public Guid SecretId { get; set; }
        public bool IsActive { get; set; }
        public int ElectionId { get; set; }
        public DateOnly ElectionDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Note { get; set; }
    }
}
