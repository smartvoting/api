using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepFive : StepBase
    {
        [Required]
        [StringLength(6)]
        public int CandidateId { get; set; }

        [Required]
        [StringLength(5)]
        public int RidingId { get; set; }
    }
}
