/*****************************************************************************************
 *     _________                      __       ____   ____     __  .__                   *
 *    /   _____/ _____ _____ ________/  |_     \   \ /   /____/  |_|__| ____    ____     *
 *    \_____  \ /     \\__  \\_  __ \   __\     \   Y   /  _ \   __\  |/    \  / ___\    *
 *    /        \  Y Y  \/ __ \|  | \/|  |        \     (  <_> )  | |  |   |  \/ /_/  >   *
 *   /_______  /__|_|  (____  /__|   |__|         \___/ \____/|__| |__|___|  /\___  /    *
 *           \/      \/     \/                                             \//_____/     *
 *****************************************************************************************
 *   Project Title: Smart Voting                                                         *
 *   Project Website: https://smartvoting.cc/                                            *
 *   API Url: https://api.smartvoting.cc/                                                *
 *   Project Source Code: https://github.com/smartvoting/                                *
 *****************************************************************************************
 *   Project License: GNU General Public License v3.0                                    *
 *   Project Authors: Stephen Davis, Michael Sirna, Matthew Campbell, Satabdi Sangma     *
 *   George Brown College - Computer Programmer Analyst (T127)                           *
 *   Capstone I & II - September 2021 to April 2022                                      *
 *****************************************************************************************/

using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using SmartVotingAPI.Models.Postgres;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Voters")]
    [ApiController]
    [Authorize(Policy = "ElectionOfficials")]
    public class VoterController : BaseController
    {
        public VoterController(PostgresDbContext _context, IOptions<AppSettings> _app) : base(_context, _app) { }

        [HttpPost]
        [Route("Check")]
        [AllowAnonymous]
        public async Task<IActionResult> CheckRegistration(VoterCheck data)
        {
            if (data == null)
                return BadRequest(NewReturnMessage("Voter information is required."));

            DateOnly dob = DateOnly.FromDateTime(data.BirthDate);

            var predicate = PredicateBuilder.New<VoterList>(true);
            predicate = string.IsNullOrEmpty(data.MiddleName) ? predicate.And(x => x.MiddleName.Equals(null)) : predicate.And(x => x.MiddleName.Equals(data.MiddleName));
            predicate = string.IsNullOrEmpty(data.UnitNumber) ? predicate.And(x => x.UnitNumber.Equals(null)) : predicate.And(x => x.UnitNumber.Equals(data.UnitNumber));

            var voterEntry = await postgres.VoterLists
                .Where(a => a.FirstName.Equals(data.FirstName))
                .Where(a => a.LastName.Equals(data.LastName))
                .Where(a => a.BirthDate.CompareTo(dob) == 0)
                .Where(a => a.Gender == data.Gender)
                .Where(a => a.StreetNumber == data.StreetNumber)
                .Where(a => a.StreetName.Equals(data.StreetName))
                .Where(a => a.City.Equals(data.City))
                .Where(a => a.ProvinceId == data.Province)
                .Where(a => a.PostCode.Equals(data.PostCode))
                .Where(predicate)
                .FirstOrDefaultAsync();

            if (voterEntry == null)
                return NoContent();

            return Ok();
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IActionResult> GetVoter(string? VoterID = "", string? FirstName = "", string? MiddleName = "", string? LastName = "", string? StreetName = "", string? City = "", string? PostCode = "")
        {
            bool nullCheck = string.IsNullOrEmpty(VoterID) && string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(MiddleName) && string.IsNullOrEmpty(LastName) && string.IsNullOrEmpty(StreetName) && string.IsNullOrEmpty(City) && string.IsNullOrEmpty(PostCode);

            if (nullCheck)
                return BadRequest(NewReturnMessage("At least one parameter is required to search for a voter."));

            var list = await postgres.VoterLists
                .Where(x => x.VoterId.ToString().ToLower().Contains(VoterID.ToLower()) &&
                    x.FirstName.ToLower().Contains(FirstName.ToLower()) &&
                    x.MiddleName.ToLower().Contains(MiddleName.ToLower()) &&
                    x.LastName.ToLower().Contains(LastName.ToLower()) &&
                    x.StreetName.ToLower().Contains(StreetName.ToLower()) &&
                    x.City.ToLower().Contains(City.ToLower()) &&
                    x.PostCode.ToLower().Contains(PostCode.ToLower())
                )
                .Select(z => new Voter
                {
                    VoterId = z.VoterId.ToString(),
                    RidingId = z.RidingId,
                    FirstName = z.FirstName,
                    MiddleName = z.MiddleName,
                    LastName = z.LastName,
                    BirthDate = DateTime.Parse(z.BirthDate.ToString()),
                    Gender = z.Gender,
                    StreetNumber = z.StreetNumber,
                    StreetName = z.StreetName,
                    UnitNumber = z.UnitNumber,
                    City = z.City,
                    ProvinceId = z.ProvinceId,
                    PostCode = z.PostCode,
                    EmailAddress = z.EmailAddress,
                    PhoneNumber = z.PhoneNumber
                })
                .ToArrayAsync();

            if (list.Length <= 0)
                return NoContent();

            return Ok(list);
        }

        [HttpPost]
        [Route("Generate")]
        [Authorize(Roles = "SA")]
        public async Task<IActionResult> GenerateVoters([Required] int NumberOfVoters)
        {
            if (NumberOfVoters <= 0)
                return BadRequest(NewReturnMessage("Number of voters must be greater than zero."));

            int counter = 0;
            while (counter < NumberOfVoters)
            {
                VoterList voter = NewVoter(counter);
                postgres.VoterLists.Add(voter);
                var voterSave = await postgres.SaveChangesAsync();

                Guid voterId = voter.VoterId;
                VoterSecurity security = NewVoterSecurity(counter, voterId);
                postgres.VoterSecurities.Add(security);
                var securitySave = await postgres.SaveChangesAsync();

                if (voterSave == 1 && securitySave == 1)
                    counter++;
                else
                    return BadRequest(NewReturnMessage("Something went wrong creating new voters."));
            }

            return Ok();
        }

        [HttpPut]
        [Route("Update")]
        [Authorize(Roles = "RA,RS")]
        public async Task<IActionResult> UpdateVoter([Required] Voter voter)
        {
            DateOnly dob = DateOnly.FromDateTime(voter.BirthDate);

            var record = await postgres.VoterLists
                .Where(a => a.FirstName.Equals(voter.FirstName))
                .Where(a => a.LastName.Equals(voter.LastName))
                .Where(a => a.BirthDate.CompareTo(dob) == 0)
                .Where(a => a.Gender == voter.Gender)
                .Where(a => a.StreetNumber == voter.StreetNumber)
                .Where(a => a.StreetName.Equals(voter.StreetName))
                .Where(a => a.City.Equals(voter.City))
                .Where(a => a.ProvinceId == voter.ProvinceId)
                .Where(a => a.PostCode.Equals(voter.PostCode))
                .FirstOrDefaultAsync();

            if (record == null)
                return NoContent();

            bool verifiedMiddleName = false;
            bool verifiedUnitNumber = false;

            if (!string.IsNullOrEmpty(record.MiddleName) && !string.IsNullOrEmpty(voter.MiddleName))
                verifiedMiddleName = record.MiddleName.Equals(voter.MiddleName);

            if (!string.IsNullOrEmpty(record.UnitNumber) && !string.IsNullOrEmpty(voter.UnitNumber))
                verifiedUnitNumber = record.UnitNumber.Equals(voter.UnitNumber);

            Guid id = record.VoterId;
            var entry = await postgres.VoterSecurities.FindAsync(id);

            if (entry == null)
                return NoContent();

            Random random = new Random();
            entry.CardId = GetCardId();
            entry.CardPin = random.Next(10000000, 99999999);

            postgres.VoterSecurities.Update(entry);
            int result = await postgres.SaveChangesAsync();

            if (result <= 0)
                return BadRequest(NewReturnMessage("Error updating voter card and pin number."));

            return Ok();
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize(Roles = "SA")]
        public async Task<IActionResult> DeleteVoterList()
        {
            PostgresDbContext context = new();
            int result = await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE voter_list, voter_security;");

            if (result != -1)
                return BadRequest();

            return Ok();
        }

        #region Helper Methods
        private VoterList NewVoter(int index)
        {
            bool isFemale = index % 2 == 0;
            bool hasMiddleName = index % 6 == 0;
            bool hasUnitNumber = index % 4 == 0;
            Random random = new();
            VoterList voter = new();

            voter.RidingId = GetRidingId(random.Next(0, 338));
            voter.FirstName = isFemale ? GetFemaleName(random.Next(0, 104)) : GetMaleName(random.Next(0, 100));
            voter.MiddleName = hasMiddleName ? GetLastName(random.Next(0, 151)) : null;
            voter.LastName = GetLastName(random.Next(0, 151));
            voter.BirthDate = GetBirthDate();

            int gender = index + 1;
            if (gender % 5 == 0)
                voter.Gender = 3;
            else if (gender % 2 == 0)
                voter.Gender = 1;
            else
                voter.Gender = 2;

            voter.StreetNumber = random.Next(1, 10001);
            voter.StreetName = GetStreetName(random.Next(0, 100));
            voter.UnitNumber = hasUnitNumber ? GetUnitNumber() : null;
            voter.ProvinceId = random.Next(1, 14);
            voter.City = GetCityName(random.Next(0, 137));
            voter.PostCode = GetPostCode();
            voter.EmailAddress = GetEmailAddress(random.Next(0, 5));
            voter.PhoneNumber = GetPhoneNumber();

            return voter;
        }
        private VoterSecurity NewVoterSecurity(int index, Guid voterId)
        {
            Random random = new Random();
            VoterSecurity security = new();

            security.VoterId = voterId;
            security.CardId = GetCardId();
            security.CardPin = random.Next(10000000, 99999999);
            security.Sin = random.Next(100, 1000);
            security.Tax10100 = random.Next(1, 1000001);
            security.Tax12000 = random.Next(0, 2500001);
            security.Tax12900 = random.Next(0, 1000001);
            security.Tax14299 = (index % 12 == 0) ? random.Next(100000, 50000001) : 0;
            security.Tax15000 = security.Tax10100 + security.Tax12000 + security.Tax12900 + security.Tax14299;
            security.Tax23600 = Convert.ToInt32(Math.Floor(security.Tax15000 * 0.3));
            security.Tax24400 = (index % 14 == 0) ? Convert.ToInt32(Math.Floor(security.Tax15000 * 0.1)) : 0;
            security.Tax26000 = (security.Tax15000 > security.Tax14299) ? security.Tax15000 - security.Tax14299 : 0;
            security.Tax31220 = (index % 21 == 0) ? 3000 : 0;
            security.Tax58240 = Convert.ToInt32(Math.Floor(security.Tax15000 * 0.15));

            return security;
        }
        #endregion
    }
}
