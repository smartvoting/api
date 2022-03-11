namespace SmartVotingAPI.Models.ReactObjects
{
    public class Riding
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Fax { get; set; }
        public Office? Office { get; set; }
        public Person[]? Candidates { get; set; }
    }
}
