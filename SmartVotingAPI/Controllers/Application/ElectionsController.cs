using Amazon.QLDB.Driver;
using Amazon.QLDB.Driver.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO.Elections;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.QLDB;
using System.Collections;
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

        [HttpDelete]
        [Route("Transfer")]
        //[Authorize(Roles = "EM")]
        [AllowAnonymous]
        public async Task<IActionResult> TransferElection([Required] int ElectionID)
        {
            bool isValid = await postgres.ElectionTokens.Where(e => e.ElectionId.Equals(ElectionID)).FirstOrDefaultAsync() != null;

            if (!isValid)
                return BadRequest(NewReturnMessage("Election with the provided ID does not exist."));

            var ridingList = await postgres.RidingLists.Where(r => r.RidingId > 100).OrderBy(r => r.RidingId).ToArrayAsync();
            var candidateList = await postgres.People.Where(z => z.RoleId.Equals(5)).OrderBy(z => z.PersonId).ToArrayAsync();

            //Models.Postgres.PastResult[] results = new Models.Postgres.PastResult[ridingList.Length];
            var electionResults = new ArrayList();

            IAsyncQldbDriver driver = AsyncQldbDriver.Builder()
                .WithLedger(appSettings.Value.Vote.LedgerID)
                .WithSerializer(new ObjectSerializer())
                .Build();

            int index = 0;
            foreach (var riding in ridingList)
            {
                var ballots = await driver.Execute(async txn =>
                {
                    IQuery<BallotToken> query = txn.Query<BallotToken>("SELECT * FROM Ballots WHERE RidingId = ?", riding.RidingId);
                    var temp = await txn.Execute(query);
                    return await temp.ToArrayAsync();
                });

                ballots = ballots.OrderBy(x => x.CandidateId).ToArray();
                var candidates = candidateList.Where(x => x.RidingId.Equals(riding.RidingId)).ToArray();
                int numCandidates = candidates.Length;

                Models.Postgres.PastResult[] ridingResults = new Models.Postgres.PastResult[numCandidates];

                for (int i = 0; i < numCandidates; i++)
                {
                    ridingResults[i] = new();
                    ridingResults[i].ElectionId = ElectionID;
                    ridingResults[i].RidingId = riding.RidingId;
                    ridingResults[i].CandidateId = candidates[i].PersonId;
                    ridingResults[i].TotalVotes = 0;
                }

                Console.WriteLine();

                foreach (var ballot in ballots)
                {
                    int counter = 0;
                    while (true)
                    {
                        if (ballot.CandidateId.Equals(ridingResults[counter].CandidateId))
                            ridingResults[counter].TotalVotes++;

                        if (counter < numCandidates)
                            counter++;

                        if (counter == numCandidates)
                            break;
                    }
                }

                if (!ElectionID.Equals(-1))
                {
                    foreach (var element in ridingResults)
                        postgres.PastResults.Add(element);
                    try
                    {
                        await postgres.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                electionResults.Add(ridingResults);
                index++;
            }

            bool sendStatus = await SendReport(electionResults);

            if (index.Equals(338) && !sendStatus)
                return Ok(NewReturnMessage("Failed to send report but transfer to PostgreSQL successful."));

            return Ok(electionResults);
        }

        [HttpGet]
        [Route("GenerateReport")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateReport(int ElectionID)
        {
            return Ok();
        }

        #region Helper Methods
        private async Task<bool> SendReport(ArrayList list)
        {
            return true;
        }

        private string GetEmailLine(Models.Postgres.PastResult result)
        {
            return "";
        }
        #endregion
    }
}
