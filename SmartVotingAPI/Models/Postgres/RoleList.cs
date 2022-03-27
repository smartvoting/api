using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class RoleList
    {
        public int RoleId { get; set; }
        public string RoleTitle { get; set; } = null!;
        public string RoleGroup { get; set; } = null!;
        public string RoleCode { get; set; } = null!;
    }
}
