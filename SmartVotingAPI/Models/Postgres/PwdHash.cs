using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PwdHash
    {
        public int AccountId { get; set; }
        public string PwdHash1 { get; set; } = null!;
    }
}
