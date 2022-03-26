using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PastResult
    {
        public Guid EntryId { get; set; }
        public int ElectionId { get; set; }
        public int RidingId { get; set; }
        public int CandidateId { get; set; }
        public int TotalVotes { get; set; }
        public bool Elected { get; set; }
    }
}
