using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PastElection
    {
        public int ElectionId { get; set; }
        public int ElectionYear { get; set; }
        public string ElectionType { get; set; } = null!;
        public DateOnly ElectionDate { get; set; }
        public int? ValidVotes { get; set; }
        public int? InvalidVotes { get; set; }
        public int? TotalElectors { get; set; }
    }
}
