﻿using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepTwo
    {
        [Required]
        [StringLength(12)]
        public string CardId { get; set; }
        
        [Required]
        [StringLength(8)]
        public int CardPin { get; set; }

        [Required]
        [StringLength(3)]
        public int SinDigits { get; set; }
    }
}
