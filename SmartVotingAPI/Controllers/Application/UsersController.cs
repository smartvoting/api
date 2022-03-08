using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/User")]
    [ApiController]
    public class UsersController : BaseController
    {
        public UsersController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }
    }
}
