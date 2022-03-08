using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Riding")]
    [ApiController]
    public class RidingController : BaseController
    {
        public RidingController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult<IEnumerable<RidingList>>> GetRidingList()
        {
            var list = await postgres.RidingLists.ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("{ridingId}")]
        public async Task<ActionResult<IEnumerable<RidingList>>> GetRidingById(int ridingId)
        {
            if (ridingId <= 0)
                return BadRequest();

            var riding = await postgres.RidingLists.FindAsync(ridingId);

            if (riding == null)
                return NoContent();

            return Ok(riding);
        }
    }
}
