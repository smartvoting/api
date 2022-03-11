using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class Person
    {
        public int PersonId { get; set; }
        public int RoleId { get; set; }
        public Guid OfficeId { get; set; }
        public int RidingId { get; set; }
        public Guid? SocialId { get; set; }
        public int PartyId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public bool AccountActive { get; set; }
        public string PwdHash { get; set; } = null!;
        public DateTime Updated { get; set; }
    }
}
