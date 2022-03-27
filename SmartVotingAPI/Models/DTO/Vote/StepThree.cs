﻿using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepThree : StepBase
    {
        [Required]
        public TaxEntry LineOne { get; set; }

        [Required]
        public TaxEntry LineTwo { get; set; }

        [Required]
        public TaxEntry LineThree { get; set; }
    }
}