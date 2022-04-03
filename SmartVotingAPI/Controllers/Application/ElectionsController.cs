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

using Amazon.QLDB.Driver;
using Amazon.QLDB.Driver.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO.Elections;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.QLDB;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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
        [Authorize(Roles = "EM,SA")]
        public async Task<IActionResult> TransferElection([Required] int ElectionID)
        {
            int numRidings = 338;
            bool isValid = await postgres.ElectionTokens.Where(e => e.ElectionId.Equals(ElectionID)).FirstOrDefaultAsync() != null;

            if (!isValid)
                return BadRequest(NewReturnMessage("Election with the provided ID does not exist."));

            var ridingList = await postgres.RidingLists.Where(r => r.RidingId > 100).OrderBy(r => r.RidingId).ToArrayAsync();
            var candidateList = await postgres.People.Where(z => z.RoleId.Equals(5)).OrderBy(z => z.PersonId).ToArrayAsync();
            int userClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals(ClaimTypes.UserData)).Value.ToString());
            Queue<Models.Postgres.PastResult[]> electionResults = new();

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
                    ridingResults[i].Elected = false;
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

                int highestVotes = -1;
                int ridingWinner = -1;

                for (int i = 0; i < ridingResults.Length; i++)
                {
                    var result = ridingResults[i];
                    if ((highestVotes.Equals(-1) && ridingWinner.Equals(-1)) || (highestVotes < result.TotalVotes))
                    {
                        highestVotes = result.TotalVotes;
                        ridingWinner = i;
                    }
                }

                ridingResults[ridingWinner].Elected = true;

                if (!ElectionID.Equals(-1))
                {
                    foreach (var element in ridingResults)
                        postgres.PastResults.Add(element);

                    try
                    {
                        int r = await postgres.SaveChangesAsync();
                        Console.WriteLine($"{r} rows transferred from qldb to postgres.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                electionResults.Enqueue(ridingResults);
                index++;
            }
            
            if (!ElectionID.Equals(-1))
            {
                foreach (var element in candidateList)
                {
                    Models.Postgres.PastCandidate temp = new Models.Postgres.PastCandidate
                    {
                        CandidateId = element.PersonId,
                        PartyId = element.PartyId,
                        RidingId = element.RidingId,
                        FirstName = element.FirstName,
                        LastName = element.LastName
                    };
                    postgres.PastCandidates.Add(temp);
                }

                try
                {
                    int r = await postgres.SaveChangesAsync();
                    Console.WriteLine($"{r} rows transferred from people to past candidates.");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            bool sendStatus = await SendReport(electionResults, ridingList, candidateList, null, userClaim);

            if (index.Equals(numRidings) && !sendStatus)
                return Ok(NewReturnMessage("Failed to send report but transfer to PostgreSQL successful."));

            return Ok(electionResults);
        }

        [HttpGet]
        [Route("GenerateReport")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateReport([Required] int ElectionID)
        {
            var election = await postgres.PastElections.Where(x => x.ElectionId.Equals(ElectionID)).FirstOrDefaultAsync();

            if (election == null)
                return BadRequest(NewReturnMessage("Election with the provided ID does not exist."));

            Queue<Models.Postgres.PastResult[]> electionResults = new();
            var ridingList = await postgres.RidingLists.Where(r => r.RidingId > 100).OrderBy(r => r.RidingId).ToArrayAsync();
            int numRidings = ridingList.Length;
            var candidateList = await postgres.People.Where(z => z.RoleId.Equals(5)).OrderBy(z => z.PersonId).ToArrayAsync();
            var pastCandidates = await postgres.PastCandidates.OrderBy(x => x.CandidateId).ToArrayAsync();
            var rawResults = await postgres.PastResults.Where(x => x.ElectionId.Equals(ElectionID)).OrderBy(x => x.RidingId).OrderBy(x => x.CandidateId).ToArrayAsync();

            for (int i = 0; i < numRidings; i++)
            {
                ArrayList temp = new();
                int currentRiding = ridingList[i].RidingId;
                int counter = 0;
                foreach (var riding in rawResults)
                {
                    if (riding.RidingId.Equals(currentRiding))
                    {
                        temp.Add(riding);
                        counter++;
                    }
                }
                Models.Postgres.PastResult[] results = new Models.Postgres.PastResult[counter];
                int nav = 0;
                foreach (var element in temp)
                {
                    results[nav] = (Models.Postgres.PastResult)element;
                    nav++;
                }
                electionResults.Enqueue(results);
            }

            bool sendStatus = await SendReport(electionResults, ridingList, candidateList, pastCandidates);

            if (!sendStatus)
                return Ok(NewReturnMessage("Failed to send report election report."));

            return Ok(electionResults);
        }

        #region Helper Methods
        private async Task<bool> SendReport(Queue<Models.Postgres.PastResult[]> electionResults, RidingList[] ridings, Person[] candidates = null, Models.Postgres.PastCandidate[]? pastCandidates = null, int? userClaim = -1)
        {
            string body = "<h1>Smart Voting CC - Election Report</h1><hr/>";
            foreach (var riding in electionResults)
            {
                if (riding.Length.Equals(0))
                    continue;
                string name = ridings.Where(x => x.RidingId.Equals(riding[0].RidingId)).Select(x => x.RidingName).First();
                string header = $"<h2>Results: Riding #{riding[0].RidingId} - {name}</h2>";
                body += header;
                body += "<table><tr><th>Candidate ID</th><th>Candidate Name</th><th>Total Votes</th><th>Riding Won (Elected)</th></tr>";
                foreach (var candidate in riding)
                {
                    string? candidateName = null;
                    if (pastCandidates == null)
                        candidateName = candidates.Where(x => x.PersonId.Equals(candidate.CandidateId)).Select(x => x.FirstName + " " + x.LastName).First();

                    if (string.IsNullOrEmpty(candidateName) && pastCandidates != null)
                        candidateName = pastCandidates.Where(x => x.CandidateId.Equals(candidate.CandidateId)).Select(x => x.FirstName + " " + x.LastName).First();

                    body += GetEmailLine(candidate, candidateName);
                }
                body += "</table><hr/>";
            }
            DateTime ts = DateTime.Now;
            body += $"<p>Report Timestamp: {ts:dddd, dd MMMM yyyy} at {ts:HH:mm:ss}</p>";

            if (!userClaim.Equals(-1))
            {
                Person? person = await postgres.People.FindAsync(userClaim);
                if (person != null)
                    body += $"<p>Report Generated By: {person.FirstName} {person.LastName} - User ID #{userClaim}</p>";
            }

            bool result = await SendEmailSES("Election Administrator", "smartvoting@skdprojects.net", "Election Results Report", body);

            return result;
        }

        private string GetEmailLine(Models.Postgres.PastResult result, string? name)
        {
            string elected = result.Elected ? "Yes" : "No";

            if (string.IsNullOrEmpty(name))
                return $"<tr><td>{result.CandidateId}</td><td>Name Not Found</td><td>{result.TotalVotes}</td><td>{elected}</td></tr>";
            else
                return $"<tr><td>{result.CandidateId}</td><td>{name}</td><td>{result.TotalVotes}</td><td>{elected}</td></tr>";
        }
        #endregion
    }
}
