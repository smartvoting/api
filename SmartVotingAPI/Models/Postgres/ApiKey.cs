using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class ApiKey
    {
        public Guid Key { get; set; }
        public string Name { get; set; } = null!;
        public bool Status { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
    }
}
