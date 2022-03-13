using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepFour
    {
        [Required]
        [StringLength(8)]
        public int EmailPin { get; set; }
    }
}
