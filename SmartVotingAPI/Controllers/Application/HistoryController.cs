using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO.Elections;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/History")]
    [ApiController]
    public class HistoryController : BaseController
    {
        public HistoryController(PostgresDbContext _context) : base(_context) { }

        [HttpGet]
        [Route("Elections")]
        public async Task<ActionResult<IEnumerable<PastElection>>> GetElectionResults(int electionId)
        {
            var list = new object();

            if (electionId != 0)
            {
                list = await postgres.PastElections
                    .Where(a => a.ElectionId == electionId)
                    .Select(a => new PastElection
                    {
                        ElectionId = a.ElectionId,
                        ElectionYear = a.ElectionYear,
                        ElectionType = a.ElectionType,
                        ElectionDate = a.ElectionDate.ToShortDateString(),
                        ValidVotes = a.ValidVotes,
                        InvalidVotes = a.InvalidVotes,
                        EligibleVoters = a.TotalElectors
                    })
                    .ToArrayAsync();
            }
            else
            {
                list = await postgres.PastElections
                    .OrderBy(a => a.ElectionId)
                    .Select(a => new PastElection
                    {
                        ElectionId = a.ElectionId,
                        ElectionYear = a.ElectionYear,
                        ElectionType = a.ElectionType,
                        ElectionDate = a.ElectionDate.ToShortDateString(),
                        ValidVotes = a.ValidVotes,
                        InvalidVotes = a.InvalidVotes,
                        EligibleVoters = a.TotalElectors
                    })
                    .ToArrayAsync();
            }

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Ridings")]
        public async Task<ActionResult<IEnumerable<PastResult>>> GetRidingResults([Required] int ridingId, int electionId, int candidateId)
        {
            if (ridingId <= 0)
                return BadRequest(NewReturnMessage("A valid riding id number is required."));

            var list = new object();

            if (electionId != 0 && candidateId != 0)
            {
                list = await postgres.PastResults
                    .Where(a => a.RidingId == ridingId)
                    .Where(a => a.ElectionId == electionId)
                    .Where(a => a.CandidateId == candidateId)
                    .Select(a => new PastResult
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        CandidateId = a.CandidateId,
                        TotalVotes = a.TotalVotes,
                        WasElected = a.Elected
                    })
                    .ToArrayAsync();
            }
            else if (electionId != 0)
            {
                list = await postgres.PastResults
                    .Where(a => a.RidingId == ridingId)
                    .Where(a => a.ElectionId == electionId)
                    .Select(a => new PastResult
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        CandidateId = a.CandidateId,
                        TotalVotes = a.TotalVotes,
                        WasElected = a.Elected
                    })
                    .ToArrayAsync();
            }
            else if (candidateId != 0)
            {
                list = await postgres.PastResults
                    .Where(a => a.RidingId == ridingId)
                    .Where(a => a.CandidateId == candidateId)
                    .Select(a => new PastResult
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        CandidateId = a.CandidateId,
                        TotalVotes = a.TotalVotes,
                        WasElected = a.Elected
                    })
                    .ToArrayAsync();
            }
            else
            {
                list = await postgres.PastResults
                    .Where(a => a.RidingId == ridingId)
                    .Select(a => new PastResult
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        CandidateId = a.CandidateId,
                        TotalVotes = a.TotalVotes,
                        WasElected = a.Elected
                    })
                    .ToArrayAsync();
            }

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Turnout")]
        public async Task<ActionResult<IEnumerable<PastTurnout>>> GetTurnoutResults(int electionId, int ridingId)
        {
            var list = new object();

            if (electionId != 0 && ridingId != 0)
            {
                list = await postgres.PastTurnouts
                    .Where(a => a.ElectionId == electionId)
                    .Where(a => a.RidingId == ridingId)
                    .Select(a => new PastTurnout
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        ValidVotes = a.ValidVotes,
                        InvalidVotes = a.InvalidVotes,
                        EligibleVoters = a.TotalElectors
                    })
                    .ToArrayAsync();
            }
            else if (electionId != 0)
            {
                list = await postgres.PastTurnouts
                    .Where(a => a.ElectionId == electionId)
                    .Select(a => new PastTurnout
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        ValidVotes = a.ValidVotes,
                        InvalidVotes = a.InvalidVotes,
                        EligibleVoters = a.TotalElectors
                    })
                    .ToArrayAsync();
            }
            else if (ridingId != 0)
            {
                list = await postgres.PastTurnouts
                    .Where(a => a.RidingId == ridingId)
                    .Select(a => new PastTurnout
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        ValidVotes = a.ValidVotes,
                        InvalidVotes = a.InvalidVotes,
                        EligibleVoters = a.TotalElectors
                    })
                    .ToArrayAsync();
            }
            else
            {
                list = await postgres.PastTurnouts
                    .Select(a => new PastTurnout
                    {
                        ElectionId = a.ElectionId,
                        RidingId = a.RidingId,
                        ValidVotes = a.ValidVotes,
                        InvalidVotes = a.InvalidVotes,
                        EligibleVoters = a.TotalElectors
                    })
                    .ToArrayAsync();
            }

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Candidates")]
        public async Task<ActionResult<IEnumerable<PastCandidate>>> GetPastCandidate(int candidateId, int ridingId)
        {
            var list = new object();

            if (candidateId != 0 && ridingId != 0)
            {
                list = await postgres.PastCandidates
                    .Where(a => a.CandidateId == candidateId)
                    .Where(a => a.RidingId == ridingId)
                    .Select(a => new PastCandidate
                    {
                        CandidateId = a.CandidateId,
                        PartyId = a.PartyId,
                        RidingId = a.RidingId,
                        FirstName = a.FirstName,
                        LastName = a.LastName
                    })
                    .ToArrayAsync();
            }
            else if (candidateId != 0)
            {
                list = await postgres.PastCandidates
                    .Where(a => a.CandidateId == candidateId)
                    .Select(a => new PastCandidate
                    { 
                        CandidateId = a.CandidateId,
                        PartyId = a.PartyId,
                        RidingId = a.RidingId,
                        FirstName = a.FirstName,
                        LastName = a.LastName
                    })
                    .ToArrayAsync();
            }
            else if (ridingId != 0)
            {
                list = await postgres.PastCandidates
                    .Where(a => a.RidingId == ridingId)
                    .Select(a => new PastCandidate
                    {
                        CandidateId = a.CandidateId,
                        PartyId = a.PartyId,
                        RidingId = a.RidingId,
                        FirstName = a.FirstName,
                        LastName = a.LastName
                    })
                    .ToArrayAsync();
            }
            else
            {
                list = await postgres.PastCandidates
                    .Select(a => new PastCandidate
                    {
                        CandidateId = a.CandidateId,
                        PartyId = a.PartyId,
                        RidingId = a.RidingId,
                        FirstName = a.FirstName,
                        LastName = a.LastName
                    })
                    .ToArrayAsync();
            }

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpPost]
        [Route("Record")]
        public async Task<IActionResult> PostTransferResults()
        {
            return Ok();
        }
    }
}
