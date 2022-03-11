using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class OfficeType
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = null!;
    }
}
