using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class TaxEntry
    {
        [Required]
        [StringLength(5)]
        public int LineNumber { get; set; }

        [Required]
        public int LineValue { get; set; }
    }
}
