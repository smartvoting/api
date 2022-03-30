using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    [Route("v1/Candidates")]
    [ApiController]
    public class CandidatesController : BaseController
    {
        public CandidatesController(PostgresDbContext _context, IOptions<AppSettings> _app) : base(_context, _app) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Candidate>>> GetCandidates(int? CandidateID = 0)
        {
            bool searchCandidateId = CandidateID > 0;

            var predicate = PredicateBuilder.New<Models.Postgres.Person>(true);
            predicate = searchCandidateId ? predicate.And(x => x.PersonId.Equals(CandidateID)) : predicate.And(x => x.RoleId.Equals(5));

            var list = await postgres.People
                .Where(predicate)
                .Join(postgres.PartyLists, x => x.PartyId, p => p.PartyId, (x, p) => new { x, p })
                .Join(postgres.RidingLists, xp => xp.x.RidingId, r => r.RidingId, (xp, r) => new { xp, r })
                .Select(z => new Candidate
                {
                    CandidateId = z.xp.x.PersonId,
                    FirstName = z.xp.x.FirstName,
                    LastName = z.xp.x.LastName,
                    PartyId = z.xp.x.PartyId,
                    PartyName = z.xp.p.PartyName,
                    RidingId = z.xp.x.RidingId,
                    RidingName = z.r.RidingName
                })
                .OrderBy(z => z.CandidateId)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<ActionResult<IEnumerable<Candidate>>> SearchCandidates(string? Name = "", int? RidingID = 0)
        {
            bool searchName = !string.IsNullOrEmpty(Name);
            bool searchRidingID = RidingID > 0;

            var predicate = PredicateBuilder.New<Models.Postgres.Person>(true);

            if (searchName)
                predicate = predicate.And(x => x.FirstName.ToLower().Contains(Name.ToLower())).Or(x => x.LastName.ToLower().Contains(Name.ToLower()));
            else if (searchRidingID)
                predicate = predicate.Or(x => x.RidingId.Equals(RidingID));
            else
                return BadRequest(NewReturnMessage("A first name, last name, riding id or all are required to perform a search."));

            var list = await postgres.People
                .Where(x => x.RoleId.Equals(5))
                .Where(predicate)
                .Join(postgres.PartyLists, x => x.PartyId, p => p.PartyId, (x, p) => new { x, p })
                .Join(postgres.RidingLists, xp => xp.x.RidingId, r => r.RidingId, (xp, r) => new { xp, r })
                .Select(z => new Candidate
                {
                    CandidateId = z.xp.x.PersonId,
                    FirstName = z.xp.x.FirstName,
                    LastName = z.xp.x.LastName,
                    PartyId = z.xp.x.PartyId,
                    PartyName = z.xp.p.PartyName,
                    RidingId = z.xp.x.RidingId,
                    RidingName = z.r.RidingName
                })
                .OrderBy(z => z.CandidateId)
                .ToArrayAsync();

            if (list.Length == 0)
                return NoContent();

            return Ok(list);
        }

        [HttpPost]
        [Route("Generate")]
        [Authorize(Roles = "SA")]
        [Authorize(Policy = "ElectionOfficials")]
        public async Task<IActionResult> GenerateCandidates([Required] int NumberOfRidings)
        {
            if (NumberOfRidings <= 0)
                return BadRequest(NewReturnMessage("Number of ridings must be greater than zero."));

            if (NumberOfRidings > 338)
                return BadRequest(NewReturnMessage("Number of ridings can not exceed 338."));

            int candidateId = 500000;
            int counter = 0;
            while (counter < NumberOfRidings)
            {
                int partyCounter = 0;
                int ridingId = GetRidingId(counter);
                while (partyCounter < 6)
                {
                    Models.Postgres.Person candidate = NewCandidate(counter, candidateId, partyCounter, ridingId);
                    string username = candidate.FirstName + "." + candidate.LastName;
                    SocialMediaList social = NewSocial(counter, username);

                    postgres.SocialMediaLists.Add(social);
                    var socialSave = await postgres.SaveChangesAsync();

                    candidate.SocialId = social.SocialId;

                    postgres.People.Add(candidate);
                    var candidateSave = await postgres.SaveChangesAsync();

                    candidateId += 29;

                    if (socialSave == 1 && candidateSave == 1)
                        partyCounter++;
                    else
                        return BadRequest(NewReturnMessage("Something went wrong creating new candidates."));
                }
                counter++;
            }

            return Ok();
        }

        [HttpPut]
        [Route("ToggleAccount")]
        [Authorize(Roles = "SA")]
        [Authorize(Policy = "ElectionOfficials")]
        public async Task<IActionResult> ToggleAccount([Required] int CandidateID)
        {
            if (CandidateID <= 0)
                return BadRequest(NewReturnMessage("A valid candidate id number is required."));

            var person = await postgres.People.FindAsync(CandidateID);

            if (person == null)
                return NoContent();

            person.AccountActive = !person.AccountActive;
            postgres.People.Update(person);
            int result = await postgres.SaveChangesAsync();

            if (result <= 0)
                return NoContent();

            return Ok(NewReturnMessage("Candidate account status updated."));
        }

        [HttpDelete]
        [Route("Delete")]
        [Authorize(Roles = "SA")]
        [Authorize(Policy = "ElectionOfficials")]
        public async Task<IActionResult> DeleteCandidate(int CandidateID)
        {
            if (CandidateID > 0)
            {
                var person = await postgres.People.FindAsync(CandidateID);
                postgres.SocialMediaLists.Remove(postgres.SocialMediaLists.Find(person.SocialId));
                postgres.People.Remove(person);
            } else
            {
                postgres.People.RemoveRange(postgres.People.Where(x => x.RoleId == 5));
                postgres.SocialMediaLists.RemoveRange(postgres.SocialMediaLists.Where(x => x.TypeId == 2));
            }

            int result = await postgres.SaveChangesAsync();
            result /= 2;

            if (result <= 0)
                return NoContent();

            string msg = $"{result} candidate records deleted.";

            return Ok(NewReturnMessage(msg));
        }

        #region Helper Methods
        private Models.Postgres.Person NewCandidate(int index, int candidateId, int partyIndex, int ridingId)
        {
            int roleId = 5;
            int[] partyIds = { 3, 10, 15, 16, 24, 30 };
            bool isFemale = index % 2 == 0;
            Random random = new();
            Models.Postgres.Person candidate = new();

            candidate.PersonId = candidateId;
            candidate.RoleId = roleId;
            candidate.RidingId = ridingId;
            candidate.PartyId = partyIds[partyIndex];
            candidate.FirstName = isFemale ? GetFemaleName(random.Next(0, 104)) : GetMaleName(random.Next(0, 100));
            candidate.LastName = GetLastName(random.Next(0, 151));
            candidate.EmailAddress = GetEmailAddress(random.Next(0, 5));
            candidate.PhoneNumber = GetPhoneNumber();
            candidate.AccountActive = true;

            return candidate;
        }
        private SocialMediaList NewSocial(int index, string username)
        {
            SocialMediaList social = new();
            social.TwitterId = username;
            social.InstagramId = username;
            social.FacebookId = username;
            social.YoutubeId = (index % 4 == 0) ? username : null;
            social.SnapchatId = (index % 13 == 0) ? username : null;
            social.FlickrId = (index % 17 == 0) ? username : null;
            social.TiktokId = (index % 23 == 0) ? username : null;
            social.TypeId = 2;
            return social;
        }
        #endregion
    }
}
