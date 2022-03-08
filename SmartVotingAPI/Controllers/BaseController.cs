using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartVotingAPI.Data;

namespace SmartVotingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly PostgresDbContext postgres;
        protected readonly IDynamoDBContext dynamo;

        public BaseController() { }

        public BaseController(PostgresDbContext context)
        {
            postgres = context;
        }

        public BaseController(IDynamoDBContext client)
        {
            dynamo = client;
        }

        public BaseController(PostgresDbContext context, IDynamoDBContext client)
        {
            postgres = context;
            dynamo = client;
        }
    }
}
