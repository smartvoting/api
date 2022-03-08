using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Volunteer")]
    [ApiController]
    public class VolunteerController : BaseController
    {
        public VolunteerController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpPost]
        [Route("Apply")]
        public async Task<IActionResult> PostApplyAsync(VolunteerApplication application)
        {
            if (application == null)
                return BadRequest();

            postgres.VolunteerApplications.Add(application);
            await postgres.SaveChangesAsync();

            return Ok();
        }
    }
}
