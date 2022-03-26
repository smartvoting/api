namespace SmartVotingAPI.Models.Postgres
{
    public class FutureElection
    {
        public Guid ElectionId { get; set; }
        public Guid AuthKey { get; set; }
        public DateOnly ElectionDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
