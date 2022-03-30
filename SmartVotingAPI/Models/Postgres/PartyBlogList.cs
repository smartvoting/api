namespace SmartVotingAPI.Models.Postgres
{
    public class PartyBlogList
    {
        public Guid PostId { get; set; }
        public int PartyId { get; set; }
        public string PostName { get; set; }
    }
}
