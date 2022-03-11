using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class RidingList
    {
        public int RidingId { get; set; }
        public Guid? OfficeId { get; set; }
        public string RidingName { get; set; } = null!;
        public string? RidingEmail { get; set; }
        public string? RidingPhone { get; set; }
        public string? RidingFax { get; set; }
    }
}
