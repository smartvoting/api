using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class TaxEntry
    {
        [Required]
        public int LineNumber { get; set; }

        [Required]
        public int LineValue { get; set; }
    }
}
