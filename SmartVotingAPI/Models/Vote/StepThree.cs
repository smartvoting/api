using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepThree
    {
        [Required]
        public TaxEntry LineOne { get; set; }

        [Required]
        public TaxEntry LineTwo { get; set; }

        [Required]
        public TaxEntry LineThree { get; set; }
    }
}
