using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepFive : StepBase
    {
        [Required]
        public int CandidateId { get; set; }

        [Required]
        public int RidingId { get; set; }
    }
}
