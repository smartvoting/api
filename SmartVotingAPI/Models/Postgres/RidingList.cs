using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class RidingList
    {
        public int RidingId { get; set; }
        public int? EmployeeId { get; set; }
        public int ProvinceId { get; set; }
        public string RidingName { get; set; } = null!;
        public string? OfficeAddress { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FaxNumber { get; set; }
        public DateTime Updated { get; set; }
    }
}
