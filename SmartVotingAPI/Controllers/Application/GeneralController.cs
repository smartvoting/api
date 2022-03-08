using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartVotingAPI.Models.Dynamo;

namespace SmartVotingAPI.Controllers.Application
{
    [Route("[controller]")]
    [ApiController]
    public class GeneralController : BaseController
    {
        public GeneralController(IDynamoDBContext client) : base(client) { }

        [HttpGet]
        [Route("{type}/{id}")]
        public async Task<IActionResult> AgencyInfo(string id, string type)
        {
            if (id == null || type == null)
                return NoContent();

            if (id.ToLower() != "ec" && id.ToLower() != "sv")
                return NoContent();

            if (type.ToLower() != "about" && type.ToLower() != "security")
                return NoContent();

            var post = await dynamo.LoadAsync<AgencyInfo>(id.ToLower(), type.ToLower());
            return Ok(post);
        }
    }
}
