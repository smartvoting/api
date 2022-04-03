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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/History")]
    [ApiController]
    public class HistoryController : BaseController
    {
        public HistoryController(PostgresDbContext _context, IOptions<AppSettings> _app) : base(_context, _app) { }

        [HttpGet]
        [Route("Elections")]
        public async Task<ActionResult<IEnumerable<Models.DTO.Elections.PastElection>>> GetElectionResults(int ElectionID)
        {
            bool searchElectionId = ElectionID > 0;

            var predicate = PredicateBuilder.New<Models.Postgres.PastElection>(true);
            predicate = searchElectionId ? predicate.And(x => x.ElectionId.Equals(ElectionID)) : predicate.And(x => x.ElectionId > 0);

            var list = await postgres.PastElections
                .Where(predicate)
                .Select(a => new Models.DTO.Elections.PastElection
                {
                    ElectionId = a.ElectionId,
                    ElectionYear = a.ElectionYear,
                    ElectionType = a.ElectionType,
                    ElectionDate = a.ElectionDate.ToShortDateString(),
                    ValidVotes = a.ValidVotes,
                    InvalidVotes = a.InvalidVotes,
                    EligibleVoters = a.TotalElectors
                })
                .OrderBy(z => z.ElectionId)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Ridings")]
        public async Task<ActionResult<IEnumerable<Models.DTO.Elections.PastResult>>> GetRidingResults(int ElectionID, int RidingID, int CandidateID)
        {
            bool filterElectionId = ElectionID > 0;
            bool filterRidingId = RidingID > 0;
            bool filterCandidateId = CandidateID > 0;

            var predicate = PredicateBuilder.New<Models.Postgres.PastResult>(true);

            if (filterElectionId)
                predicate = predicate.And(x => x.ElectionId.Equals(ElectionID));
            
            if (filterRidingId)
                predicate = predicate.And(x => x.RidingId.Equals(RidingID));

            if (filterCandidateId)
                predicate = predicate.And(x => x.CandidateId.Equals(CandidateID));

            if (!filterRidingId && !filterElectionId && !filterCandidateId)
                predicate = predicate.And(x => x.RidingId > 0);
                
            var list = await postgres.PastResults
                .Where(predicate)
                .Select(a => new Models.DTO.Elections.PastResult
                {
                    ElectionId = a.ElectionId,
                    RidingId = a.RidingId,
                    CandidateId = a.CandidateId,
                    TotalVotes = a.TotalVotes,
                    WasElected = a.Elected
                })
                .OrderBy(z => z.RidingId)
                .OrderBy(z => z.CandidateId)
                .OrderBy(z => z.ElectionId)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Turnout")]
        public async Task<ActionResult<IEnumerable<Models.DTO.Elections.PastTurnout>>> GetTurnoutResults(int ElectionID, int RidingID)
        {
            bool filterElectionId = ElectionID > 0;
            bool filterRidingId = RidingID > 0;

            var predicate = PredicateBuilder.New<Models.Postgres.PastTurnout>(true);

            if (filterElectionId)
                predicate = predicate.And(x => x.ElectionId.Equals(ElectionID));

            if (filterRidingId)
                predicate = predicate.And(x => x.RidingId.Equals(RidingID));

            if (!filterElectionId && !filterRidingId)
                predicate = predicate.And(x => x.ElectionId > 0).And(x => x.RidingId > 0);

            var list = await postgres.PastTurnouts
                .Where(predicate)
                .Select(a => new Models.DTO.Elections.PastTurnout
                {
                    ElectionId = a.ElectionId,
                    RidingId = a.RidingId,
                    ValidVotes = a.ValidVotes,
                    InvalidVotes = a.InvalidVotes,
                    EligibleVoters = a.TotalElectors
                })
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Candidates")]
        public async Task<ActionResult<IEnumerable<Models.DTO.Elections.PastCandidate>>> GetPastCandidate(int CandidateID, int RidingID)
        {
            bool filterCandidateId = CandidateID > 0;
            bool filterRidingId = RidingID > 0;

            var predicate = PredicateBuilder.New<Models.Postgres.PastCandidate>(true);

            if (filterCandidateId)
                predicate = predicate.And(x => x.CandidateId.Equals(CandidateID));

            if (filterRidingId)
                predicate = predicate.And(x => x.RidingId.Equals(RidingID));

            if (!filterRidingId && !filterCandidateId)
                predicate = predicate.And(x => x.RidingId > 0).And(x => x.CandidateId > 0);

            var list = await postgres.PastCandidates
                .Where(predicate)
                .Select(a => new Models.DTO.Elections.PastCandidate
                {
                    CandidateId = a.CandidateId,
                    PartyId = a.PartyId,
                    RidingId = a.RidingId,
                    FirstName = a.FirstName,
                    LastName = a.LastName
                })
                .OrderBy(z => z.CandidateId)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        //[HttpPost]
        //[Route("Record")]
        //public async Task<IActionResult> PostTransferResults()
        //{
        //    //RidingList[] ridings = await postgres.RidingLists.OrderBy(a => a.RidingId).ToArrayAsync();
        //    //int length = ridings.Length;
        //    //var ballots = new object();

        //    //IAsyncQldbDriver driver = AsyncQldbDriver.Builder()
        //    //    .WithLedger(appSettings.Value.Vote.LedgerID)
        //    //    .WithSerializer(new ObjectSerializer())
        //    //    .Build();

        //    //var read = await driver.Execute(async txn =>
        //    //{
        //    //    IQuery<VoteToken> query = txn.Query<VoteToken>("SELECT * FROM Ballots WHERE RidingId = ?", 35027);
        //    //    var result = await txn.Execute(query);
        //    //    return await result.ToArrayAsync();
        //    //});

        //    //Models.Postgres.PastResult result = new();

        //    //Console.WriteLine(read.Length);

        //    ////await foreach (var i in read)
        //    ////    Console.WriteLine(i.ToString());

        //    return Ok();
        //}
    }
}
