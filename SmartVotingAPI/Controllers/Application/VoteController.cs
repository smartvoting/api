using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Vote;
using System.Text.Json.Nodes;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Vote")]
    [ApiController]
    public class VoteController : BaseController
    {
        public VoteController(PostgresDbContext context, IDynamoDBContext client, IOptions<AppSettings> app) : base(context, client, app) { }

        [HttpPost]
        [Route("Step/1")]
        public async Task<IActionResult> StepOne(StepOne data)
        {
            //bool IsValidRequest = await VerifyHcaptcha(data.Token, data.RemoteIp);
            bool IsValidRequest = true;
            var json = new JsonObject
            {
                ["status"] = false
            };
            return Unauthorized(json);
        }
    }
}
