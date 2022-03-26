using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepTwo : StepBase
    {
        [Required]
        [StringLength(12)]
        public string CardId { get; set; }
        
        [Required]
        public int CardPin { get; set; }

        [Required]
        public int SinDigits { get; set; }
    }
}
