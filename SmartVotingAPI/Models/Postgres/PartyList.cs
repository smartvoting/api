using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PartyList
    {
        public int PartyId { get; set; }
        public Guid? OfficeId { get; set; }
        public Guid? SocialId { get; set; }
        public string PartyName { get; set; } = null!;
        public string? PartyDomain { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FaxNumber { get; set; }
        public bool IsRegistered { get; set; }
        public string? DeregisterReason { get; set; }
        public DateTime Updated { get; set; }
    }
}
