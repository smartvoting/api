using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class OfficeList
    {
        public Guid OfficeId { get; set; }
        public int TypeId { get; set; }
        public string? StreetNumber { get; set; }
        public string? StreetName { get; set; }
        public string? UnitNumber { get; set; }
        public string City { get; set; } = null!;
        public int ProvinceId { get; set; }
        public string PostCode { get; set; } = null!;
        public string? PoBox { get; set; }
        public bool IsPublic { get; set; }
        public DateTime Updated { get; set; }
    }
}
