﻿using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepFour : StepBase
    {
        [Required]
        [StringLength(8)]
        public int EmailPin { get; set; }
    }
}
