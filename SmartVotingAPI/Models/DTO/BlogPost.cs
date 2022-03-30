namespace SmartVotingAPI.Models.DTO
{
    public class BlogPost
    {
        public string PostId { get; set; }
        public int PartyId { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Posted { get; set; }
        public DateTime Modified { get; set; }
        public int AuthorId { get; set; }
    }
}
