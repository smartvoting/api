using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartVotingAPI.Models.Dynamo;
using System.Text.Json;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1")]
    [ApiController]
    public class GeneralController : BaseController
    {
        public GeneralController(IDynamoDBContext client) : base(client) { }

        [HttpGet]
        [Route("{documentType}/{agencyCode}")]
        public async Task<ActionResult<IEnumerable<AgencyInfo>>> GetAgencyInfo(string documentType, string agencyCode)
        {
            if (agencyCode == null || documentType == null)
                return BadRequest();

            if (agencyCode.ToLower() != "ec" && agencyCode.ToLower() != "sv")
                return BadRequest();

            if (documentType.ToLower() != "about" && documentType.ToLower() != "security")
                return BadRequest();

            var post = await dynamo.LoadAsync<AgencyInfo>(agencyCode.ToLower(), documentType.ToLower());
            
            if (post == null)
                return NoContent();

            return Ok(post);
        }
    }
}
