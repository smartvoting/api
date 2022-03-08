using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PartyStaff
    {
        public Guid EntryId { get; set; }
        public int UserId { get; set; }
        public int PartyId { get; set; }
        public int RidingId { get; set; }
        public int RoleId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
