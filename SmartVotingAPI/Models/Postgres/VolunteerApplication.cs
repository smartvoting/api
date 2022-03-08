using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class VolunteerApplication
    {
        public int ApplicationId { get; set; }
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
        public bool IsApproved { get; set; }
        public DateTime Submitted { get; set; }
        public DateTime Updated { get; set; }
    }
}
