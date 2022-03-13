using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepOne
    {
        [Required]
        public bool IsCitizen { get; set; }

        [Required]
        public bool IsLegalAge { get; set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public DateOnly BirthDate { get; set; }

        [Required]
        [Range(1, 3)]
        public int Gender { get; set; }

        [Required]
        public int StreetNumber { get; set; }

        [Required]
        public string StreetName { get; set; }

        public string? UnitNumber { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        [Range(1, 13)]
        public int Province { get; set; }

        [Required]
        [StringLength(7)]
        public string PostCode { get; set; }
    }
}
