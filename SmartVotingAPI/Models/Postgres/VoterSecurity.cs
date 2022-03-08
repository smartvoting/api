using System;
using System.Collections.Generic;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class VoterSecurity
    {
        public string CardId { get; set; } = null!;
        public Guid VoterId { get; set; }
        public int CardPin { get; set; }
        public string EmailPin { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public short Sin { get; set; }
        public int Tax10100 { get; set; }
        public int Tax12000 { get; set; }
        public int Tax12900 { get; set; }
        public int Tax14299 { get; set; }
        public int Tax15000 { get; set; }
        public int Tax23600 { get; set; }
        public int Tax24400 { get; set; }
        public int Tax26000 { get; set; }
        public int Tax31270 { get; set; }
        public int Tax58240 { get; set; }
    }
}
