using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepSix : StepBase
    {
        [Required]
        public bool UserConfirmation { get; set; }
    }
}
