using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PartyList
    {
        public int PartyId { get; set; }
        public string PartyName { get; set; } = null!;
        public string? PartyDomain { get; set; }
        public bool IsRegistered { get; set; }
        public string? DeregisterReason { get; set; }
        public string? HeadOffice { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FaxNumber { get; set; }
        public string? TwitterId { get; set; }
        public string? InstagramId { get; set; }
        public string? FacebookId { get; set; }
        public string? YoutubeId { get; set; }
        public string? SnapchatId { get; set; }
        public string? FlickrId { get; set; }
        public DateTime? Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}
