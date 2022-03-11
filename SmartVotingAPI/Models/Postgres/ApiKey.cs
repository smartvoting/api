using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class ApiKey
    {
        public Guid KeyId { get; set; }
        public Guid AuthKey { get; set; }
        public string KeyName { get; set; } = null!;
        public bool IsProduction { get; set; }
        public bool IsDevelopment { get; set; }
        public bool IsActive { get; set; }
        public int KeyTtl { get; set; }
        public DateTime Created { get; set; }
    }
}
