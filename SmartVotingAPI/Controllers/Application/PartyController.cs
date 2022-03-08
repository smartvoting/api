using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Dynamo;
using SmartVotingAPI.Models.Postgres;
using System.Text.Json;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Party")]
    [ApiController]
    public class PartyController : BaseController
    {
        public PartyController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult<IEnumerable<PartyList>>> GetPartyList()
        {
            var list = await postgres.PartyLists.ToArrayAsync();
            
            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("{partyId}")]
        public async Task<ActionResult<IEnumerable<PartyList>>> GetPartyById(int partyId)
        {
            if (partyId <= 0)
                return BadRequest();

            var party = await postgres.PartyLists.FindAsync(partyId);

            if (party == null)
                return NoContent();

            return Ok(party);
        }

        [HttpGet]
        [Route("{partyId}/Issues")]
        public async Task<ActionResult> GetPartyIssues(int partyId)
        {
            if (partyId <= 0)
                return BadRequest();

            var topics = await postgres.PlatformTopics.ToArrayAsync();
            var entries = await dynamo.QueryAsync<PartyPlatform>(partyId).GetRemainingAsync();

            if (topics == null || entries == null)
                return NoContent();

            var list = new
            {
                topics,
                entries
            };

            string json = JsonSerializer.Serialize(list);

            return Ok(json);
        }

        [HttpGet]
        [Route("{partyId}/Issue/{topicId}")]
        public async Task<ActionResult<IEnumerable<PartyPlatform>>> GetPartyIssueById(int partyId, int topicId)
        {
            if (partyId <= 0 || topicId <= 0 || topicId > 25)
                return BadRequest();

            var issue = await dynamo.LoadAsync<PartyPlatform>(partyId, topicId);

            if (issue == null)
                return NoContent();

            return Ok(issue);
        }
    }
}
