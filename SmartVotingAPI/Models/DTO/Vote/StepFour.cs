﻿using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepFour : StepBase
    {
        [Required]
        public int EmailPin { get; set; }
    }
}