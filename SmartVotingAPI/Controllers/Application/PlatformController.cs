/*****************************************************************************************
 *     _________                      __       ____   ____     __  .__                   *
 *    /   _____/ _____ _____ ________/  |_     \   \ /   /____/  |_|__| ____    ____     *
 *    \_____  \ /     \\__  \\_  __ \   __\     \   Y   /  _ \   __\  |/    \  / ___\    *
 *    /        \  Y Y  \/ __ \|  | \/|  |        \     (  <_> )  | |  |   |  \/ /_/  >   *
 *   /_______  /__|_|  (____  /__|   |__|         \___/ \____/|__| |__|___|  /\___  /    *
 *           \/      \/     \/                                             \//_____/     *
 *****************************************************************************************
 *   Project Title: Smart Voting                                                         *
 *   Project Website: https://smartvoting.cc/                                            *
 *   API Url: https://api.smartvoting.cc/                                                *
 *   Project Source Code: https://github.com/smartvoting/                                *
 *****************************************************************************************
 *   Project License: GNU General Public License v3.0                                    *
 *   Project Authors: Stephen Davis, Michael Sirna, Matthew Campbell, Satabdi Sangma     *
 *   George Brown College - Computer Programmer Analyst (T127)                           *
 *   Capstone I & II - September 2021 to April 2022                                      *
 *****************************************************************************************/

using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using SmartVotingAPI.Models.Dynamo;
using SmartVotingAPI.Models.Postgres;
using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Platform")]
    [ApiController]
    public class PlatformController : BaseController
    {
        public PlatformController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Platform>>> GetTopic([Required] int PartyID, [Required] int TopicID)
        {
            if (PartyID <= 0)
                return BadRequest(NewReturnMessage("Party ID must be greater than zero."));

            if (TopicID <= 0)
                return BadRequest(NewReturnMessage("Topic ID must be greater than zero."));

            var topic = await postgres.PlatformTopics.FindAsync(TopicID);
            var policy = await dynamo.LoadAsync<PartyPlatform>(PartyID, TopicID);

            if (topic == null || policy == null)
                return NoContent();

            Platform platform = new();
            platform.TopicId = TopicID;
            platform.PartyId = PartyID;
            platform.TopicTitle = topic.TopicTitle;
            platform.TopicBody = policy.TopicBody;
            platform.DateModified = policy.DateModified;

            return Ok(platform);
        }

        [HttpPost]
        [Authorize(Roles = "PM")]
        public async Task<IActionResult> NewTopic(Platform platform)
        {
            int partyClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("PartyId")).Value.ToString());
            
            if (platform == null || partyClaim <= 0)
                return BadRequest();

            DateTime timestamp = DateTime.Now;
            PartyPlatform update = new();
            update.PartyId = partyClaim;
            update.TopicId = platform.TopicId;
            update.TopicBody = platform.TopicBody;
            update.DateModified = timestamp.ToString();

            await dynamo.SaveAsync(update);

            return Ok();
        }

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
