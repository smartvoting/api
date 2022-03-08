using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class VolunteerApplication
    {
        public int ApplicationId { get; set; }
        public int PartyId { get; set; }
        public int RidingId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public bool LegalResident { get; set; }
        public bool PastVolunteer { get; set; }
        public bool PartyMember { get; set; }
        public bool IsApproved { get; set; }
        public DateTime Submitted { get; set; }
        public DateTime Updated { get; set; }
    }
}
