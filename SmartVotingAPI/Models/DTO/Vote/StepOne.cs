using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepOne : StepBase
    {
        [Required]
        public bool IsCitizen { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [Range(1, 3)]
        public int Gender { get; set; }

        [Required]
        public int StreetNumber { get; set; }

        [Required]
        public string StreetName { get; set; } = null!;

        public string? UnitNumber { get; set; }

        [Required]
        public string City { get; set; } = null!;

        [Required]
        [Range(1, 13)]
        public int Province { get; set; }

        [Required]
        [StringLength(7)]
        public string PostCode { get; set; } = null!;
    }
}
