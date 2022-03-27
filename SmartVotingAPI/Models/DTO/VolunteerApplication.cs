using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO
{
    public class VolunteerApplication
    {
        [Required]
        public int PartyId { get; set; }

        [Required]
        public int RidingId { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string PhoneNumber { get; set; } = null!;

        [Required]
        public string EmailAddress { get; set; } = null!;

        [Required]
        public bool LegalResident { get; set; }

        [Required]
        public bool PastVolunteer { get; set; }

        [Required]
        public bool PartyMember { get; set; }
    }
}
