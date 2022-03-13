using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepSix
    {
        [Required]
        public bool UserConfirmation { get; set; }
    }
}
