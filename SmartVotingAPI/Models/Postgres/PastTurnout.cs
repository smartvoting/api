using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PastTurnout
    {
        public Guid RecordId { get; set; }
        public int ElectionId { get; set; }
        public int RidingId { get; set; }
        public int ValidVotes { get; set; }
        public int InvalidVotes { get; set; }
        public int TotalElectors { get; set; }
    }
}
