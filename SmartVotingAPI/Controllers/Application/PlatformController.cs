using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Dynamo;
using SmartVotingAPI.Models.Postgres;
using System.Text.Json;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Platform")]
    [ApiController]
    public class PlatformController : BaseController
    {
        public PlatformController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpGet]
        [Route("Topics")]
        public async Task<ActionResult<IEnumerable<PlatformTopic>>> GetPlatformTopicList()
        {
            var list = await postgres.PlatformTopics.ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("Topic/{topicId}")]
        public async Task<ActionResult<IEnumerable<PlatformTopic>>> GetPlatformTopicById(int topicId)
        {
            if (topicId <= 0)
                return BadRequest(new { message = "Invalid topic id number." });

            var topic = await postgres.PlatformTopics.FindAsync(topicId);

            if (topic == null)
                return NoContent();

            return Ok(topic);
        }
    }
}
