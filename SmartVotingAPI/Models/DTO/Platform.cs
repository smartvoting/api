namespace SmartVotingAPI.Models.DTO
{
    public class Platform
    {
        public int TopicId { get; set; }
        public int PartyId { get; set; }
        public string TopicTitle { get; set; }
        public string TopicBody { get; set; }
        public string DateModified { get; set; }
    }
}
