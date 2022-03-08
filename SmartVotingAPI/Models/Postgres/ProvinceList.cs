using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class ProvinceList
    {
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; } = null!;
    }
}
