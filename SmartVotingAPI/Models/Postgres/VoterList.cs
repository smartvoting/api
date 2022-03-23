using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Postgres
{
    public partial class VoterList
    {
        public Guid VoterId { get; set; }
        public int RidingId { get; set; }
        public string FirstName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public string LastName { get; set; } = null!;
        public DateOnly BirthDate { get; set; }
        public int Gender { get; set; }
        public int StreetNumber { get; set; }
        public string StreetName { get; set; } = null!;
        public string UnitNumber { get; set; } = null!;
        public string City { get; set; } = null!;
        public int ProvinceId { get; set; }
        public string PostCode { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
    }
}
