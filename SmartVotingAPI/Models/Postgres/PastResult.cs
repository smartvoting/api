﻿namespace SmartVotingAPI.Models.Postgres
{
    public partial class PastResult
    {
        public Guid RecordId { get; set; }
        public int ElectionId { get; set; }
        public int RidingId { get; set; }
        public int CandidateId { get; set; }
        public int TotalVotes { get; set; }
        public bool Elected { get; set; }
    }
}
