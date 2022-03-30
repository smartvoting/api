using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO.Elections;
using SmartVotingAPI.Models.Postgres;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Elections")]
    [ApiController]
    [Authorize(Policy = "ElectionOfficials")]
    public class ElectionsController : BaseController
    {
        public ElectionsController(PostgresDbContext _context, IOptions<AppSettings> _app) : base(_context, _app) { }

        [HttpGet]
        [Route("Token")]
        [Authorize(Roles = "SA")]
        public async Task<IActionResult> GetToken([Required] int ElectionID)
        {
            if (ElectionID <= 0)
                return BadRequest(NewReturnMessage("Election ID must be greater than zero."));

            var list = await postgres.ElectionTokens
                .Where(x => x.ElectionId == ElectionID)
                .Select(x => new
                {
                    ApiKey = x.PublicId.ToString(),
                    IsActive = x.IsActive,
                    ElectionId = x.ElectionId,
                    ElectionDate = x.ElectionDate.ToLongDateString(),
                    StartTime = x.StartTime.ToLongTimeString(),
                    EndTime = x.EndTime.ToLongTimeString()
                })
                .ToArrayAsync();

            if (list.Length == 0)
                return NoContent();

            return Ok(list);
        }

        [HttpPost]
        [Route("Create")]
        [Authorize(Roles = "SA")]
        public async Task<IActionResult> CreateElection(Election election)
        {
            if (election == null)
                return BadRequest();

            DateOnly date = DateOnly.FromDateTime(election.ElectionDate);
            TimeOnly start = TimeOnly.FromDateTime(election.StartTime);
            TimeOnly end = TimeOnly.FromDateTime(election.EndTime);

            ElectionToken token = new();
            Models.Postgres.PastElection past = new();
            past.ElectionYear = election.ElectionYear;
            past.ElectionType = election.ElectionType;
            postgres.PastElections.Add(past);
            int pastResult = await postgres.SaveChangesAsync();

            token.ElectionId = past.ElectionId;
            token.ElectionDate = date;
            token.StartTime = start;
            token.EndTime = end;
            token.Note = election.Notes;
            postgres.ElectionTokens.Add(token);
            int tokenResult = await postgres.SaveChangesAsync();

            if (pastResult != 1 || tokenResult != 1)
                return BadRequest(NewReturnMessage("Something went wrong creating a new election."));

            return Ok();
        }

        //[HttpPut]
        //[Route("Modify")]
        //[Authorize(Roles = "SA")]
        //public async Task<IActionResult> ModifyElection([Required] int ElectionID, bool? IsActive, DateTime? ElectionDate)
        //{
        //    return Ok();
        //}

        [HttpDelete]
        [Route("Close")]
        [Authorize(Roles = "SA")]
        public async Task<IActionResult> CloseElection([Required] int ElectionID)
        {
            if (ElectionID <= 0)
                return BadRequest(NewReturnMessage("Election ID must be greater than zero."));

            var election = await postgres.ElectionTokens.Where(x => x.ElectionId == ElectionID).FirstOrDefaultAsync();

            if (election == null)
                return NoContent();

            election.IsActive = false;
            postgres.ElectionTokens.Update(election);
            int electionResult = await postgres.SaveChangesAsync();

            if (electionResult != 1)
                return BadRequest(NewReturnMessage("Failed to deactivate the election."));

            Models.Postgres.PastElection past = new();
            past.ElectionId = election.ElectionId;
            past.ElectionYear = election.ElectionDate.Year;
            past.ElectionType = "Federal";
            past.ElectionDate = election.ElectionDate;

            postgres.PastElections.Add(past);
            int pastResult = await postgres.SaveChangesAsync();

            if (pastResult != 1)
                return BadRequest(NewReturnMessage("Failed to add election to past elections list."));

            return Ok();
        }
    }
}
