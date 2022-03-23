using System.Collections;

namespace SmartVotingAPI.Models.DTO
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
        public Coordinates? Centroid { get; set; }
        public ArrayList? Outline { get; set; }
    }
}
