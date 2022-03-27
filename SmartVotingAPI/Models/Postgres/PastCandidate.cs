using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class PastCandidate
    {
        public int CandidateId { get; set; }
        public int PartyId { get; set; }
        public int RidingId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }
}
