using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Vote")]
    [ApiController]
    public class VoteController : BaseController
    {
        public VoteController(PostgresDbContext context, IDynamoDBContext client, IOptions<AppSettings> app) : base(context, client, app) { }

        //[HttpPost]
        //[Route("Step/1")]
        
    }
}
