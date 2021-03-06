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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Dynamo;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.DTO;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using LinqKit;
using System.Security.Claims;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Party")]
    [ApiController]
    public class PartyController : BaseController
    {
        public PartyController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult<IEnumerable<Party>>> GetPartyList()
        {
            var list = await postgres.PartyLists
                .Where(p => p.PartyId > 0)
                .Join(postgres.OfficeLists, p => p.OfficeId, o => o.OfficeId, (p, o) => new { p, o })
                .Join(postgres.OfficeTypes, po => po.o.TypeId, t => t.TypeId, (po, t) => new { po, t })
                .Join(postgres.ProvinceLists, pot => pot.po.o.ProvinceId, l => l.ProvinceId, (pot, l) => new { pot, l })
                .Select(x => new Party
                {
                    Id = x.pot.po.p.PartyId,
                    Name = x.pot.po.p.PartyName,
                    Domain = x.pot.po.p.PartyDomain,
                    EmailAddress = x.pot.po.p.EmailAddress,
                    PhoneNumber = x.pot.po.p.PhoneNumber,
                    FaxNumber = x.pot.po.p.FaxNumber,
                    IsRegistered = x.pot.po.p.IsRegistered,
                    DeregisterReason = x.pot.po.p.DeregisterReason,
                    Updated = x.pot.po.p.Updated,
                    Office = new Office
                    {
                        Type = x.pot.t.TypeName,
                        StreetNumber = x.pot.po.o.StreetNumber,
                        StreetName = x.pot.po.o.StreetName,
                        UnitNumber = x.pot.po.o.UnitNumber,
                        City = x.pot.po.o.City,
                        Province = x.l.ProvinceName,
                        PostCode = x.pot.po.o.PostCode,
                        PoBox = x.pot.po.o.PoBox,
                        IsPublic = x.pot.po.o.IsPublic
                    },
                    SocialMedia = null,
                    PartyLeader = null,
                    Candidates = null
                })
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("{partyId}")]
        public async Task<ActionResult<IEnumerable<Party>>> GetPartyById(int partyId)
        {
            if (partyId <= 0)
                return BadRequest(new { message = "Invalid party id number." });

            var party = await postgres.PartyLists
                .Where(p => p.PartyId == partyId)
                .Join(postgres.OfficeLists, p => p.OfficeId, o => o.OfficeId, (p, o) => new { p, o })
                .Join(postgres.OfficeTypes, po => po.o.TypeId, t => t.TypeId, (po, t) => new { po, t })
                .Join(postgres.SocialMediaLists, pot => pot.po.p.SocialId, s => s.SocialId, (pot, s) => new { pot, s })
                .Join(postgres.ProvinceLists, pots => pots.pot.po.o.ProvinceId, l => l.ProvinceId, (pots, l) => new { pots, l })
                .Select(x => new Party
                {
                    Id = x.pots.pot.po.p.PartyId,
                    Name = x.pots.pot.po.p.PartyName,
                    Domain = x.pots.pot.po.p.PartyDomain,
                    EmailAddress = x.pots.pot.po.p.EmailAddress,
                    PhoneNumber = x.pots.pot.po.p.PhoneNumber,
                    FaxNumber = x.pots.pot.po.p.FaxNumber,
                    IsRegistered = x.pots.pot.po.p.IsRegistered,
                    DeregisterReason = x.pots.pot.po.p.DeregisterReason,
                    Updated = x.pots.pot.po.p.Updated,
                    Office = new Office
                    {
                        Type = x.pots.pot.t.TypeName,
                        StreetNumber = x.pots.pot.po.o.StreetNumber,
                        StreetName = x.pots.pot.po.o.StreetName,
                        UnitNumber = x.pots.pot.po.o.UnitNumber,
                        City = x.pots.pot.po.o.City,
                        Province = x.l.ProvinceName,
                        PostCode = x.pots.pot.po.o.PostCode,
                        PoBox = x.pots.pot.po.o.PoBox,
                        IsPublic = x.pots.pot.po.o.IsPublic
                    },
                    SocialMedia = new SocialMedia
                    {
                        TwitterId = x.pots.s.TwitterId,
                        InstagramId = x.pots.s.InstagramId,
                        FacebookId = x.pots.s.FacebookId,
                        YoutubeId = x.pots.s.YoutubeId,
                        SnapchatId = x.pots.s.SnapchatId,
                        FlickrId = x.pots.s.FlickrId,
                        TiktokId = x.pots.s.TiktokId
                    }
                })
                .FirstOrDefaultAsync();

            if (party == null)
                return NoContent();

            var candidates = await postgres.People
                .Where(p => p.PartyId == partyId && p.RoleId == 5)
                .Join(postgres.SocialMediaLists, p => p.SocialId, s => s.SocialId, (p, s) => new { p, s })
                .Join(postgres.RidingLists, ps => ps.p.RidingId, r => r.RidingId, (ps, r) => new { ps, r })
                .Join(postgres.OfficeLists, psr => psr.r.OfficeId, o => o.OfficeId, (psr, o) => new { psr, o })
                .Join(postgres.OfficeTypes, psro => psro.o.TypeId, t => t.TypeId, (psro, t) => new { psro, t })
                .Join(postgres.ProvinceLists, psrot => psrot.psro.o.ProvinceId, l => l.ProvinceId, (psrot, l) => new { psrot, l })
                .Select(x => new Models.DTO.Person
                {
                    FirstName = x.psrot.psro.psr.ps.p.FirstName,
                    LastName = x.psrot.psro.psr.ps.p.LastName,
                    EmailAddress = x.psrot.psro.psr.ps.p.EmailAddress,
                    PhoneNumber = x.psrot.psro.psr.ps.p.PhoneNumber,
                    Office = new Office
                    {
                        Type = x.psrot.t.TypeName,
                        StreetNumber = x.psrot.psro.o.StreetNumber,
                        StreetName = x.psrot.psro.o.StreetName,
                        UnitNumber = x.psrot.psro.o.UnitNumber,
                        City = x.psrot.psro.o.City,
                        Province = x.l.ProvinceName,
                        PoBox = x.psrot.psro.o.PoBox,
                        IsPublic = x.psrot.psro.o.IsPublic
                    },
                    SocialMedia = new SocialMedia
                    {
                        TwitterId = x.psrot.psro.psr.ps.s.TwitterId,
                        InstagramId = x.psrot.psro.psr.ps.s.InstagramId,
                        FacebookId = x.psrot.psro.psr.ps.s.FacebookId,
                        YoutubeId = x.psrot.psro.psr.ps.s.YoutubeId,
                        SnapchatId = x.psrot.psro.psr.ps.s.SnapchatId,
                        FlickrId = x.psrot.psro.psr.ps.s.FlickrId,
                        TiktokId = x.psrot.psro.psr.ps.s.TiktokId
                    }
                })
                .ToArrayAsync();

            party.Candidates = candidates;

            return Ok(party);
        }

        [HttpGet]
        [Route("{partyId}/Issues")]
        public async Task<ActionResult> GetPartyIssues(int partyId)
        {
            if (partyId <= 0)
                return BadRequest(new { message = "Invalid party id number." });

            var topics = await postgres.PlatformTopics.ToArrayAsync();
            var entries = await dynamo.QueryAsync<PartyPlatform>(partyId).GetRemainingAsync();

            if (topics == null || entries == null)
                return NoContent();

            var list = new
            {
                topics,
                entries
            };

            string json = JsonSerializer.Serialize(list);

            return Ok(json);
        }

        [HttpGet]
        [Route("{partyId}/Issue/{topicId}")]
        public async Task<ActionResult<IEnumerable<PartyPlatform>>> GetPartyIssueById(int partyId, int topicId)
        {
            if (partyId <= 0 || topicId <= 0 || topicId > 25)
                return BadRequest(new { message = "Invalid party or topic id number." });

            var issue = await dynamo.LoadAsync<PartyPlatform>(partyId, topicId);

            if (issue == null)
                return NoContent();

            return Ok(issue);
        }

        [HttpGet]
        [Route("Blog")]
        public async Task<ActionResult<IEnumerable<BlogPost>>> GetBlog([Required] int PartyID, [Required] string PostID)
        {
            if (PartyID <= 0)
                return BadRequest(NewReturnMessage("Party ID must be greater than zero."));

            if (string.IsNullOrEmpty(PostID))
                return BadRequest(NewReturnMessage("Post ID is required."));

            var title = await postgres.PartyBlogLists.FindAsync(Guid.Parse(PostID));

            var blog = await dynamo.LoadAsync<PartyBlog>(PartyID, PostID.ToLower());

            if (blog == null || title == null)
                return NoContent();

            BlogPost post = new();
            post.PostId = PostID;
            post.PartyId = PartyID;
            post.Title = title.PostName;
            post.Body = blog.BlogBody;
            post.Posted = DateTime.Parse(blog.DatePosted.ToString());
            post.Modified = DateTime.Parse(blog.DateModified.ToString());
            post.AuthorId = blog.PersonId;

            return Ok(post);
        }

        [HttpPost]
        [Route("Blog")]
        [Authorize(Roles = "OM")]
        public async Task<IActionResult> PostBlog(BlogPost post)
        {
            if (post == null)
                return BadRequest();

            int writerClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals(ClaimTypes.UserData)).Value.ToString());
            int partyClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("PartyId")).Value.ToString());

            DateTime timestamp = DateTime.Now;

            if (writerClaim <= 0 || partyClaim <= 0)
                return BadRequest();

            PartyBlogList postgresql = new PartyBlogList();
            postgresql.PartyId = partyClaim;
            postgresql.PostName = post.Title;

            postgres.PartyBlogLists.Add(postgresql);
            var pResult = await postgres.SaveChangesAsync();

            PartyBlog dynamodb = new PartyBlog();
            dynamodb.PartyId = partyClaim;
            dynamodb.BlogId = postgresql.PostId.ToString();
            dynamodb.BlogBody = post.Body;
            dynamodb.DatePosted = timestamp.ToString();
            dynamodb.DateModified = timestamp.ToString();
            dynamodb.PersonId = writerClaim;

            await dynamo.SaveAsync(dynamodb);

            return Ok();
        }

        [HttpGet]
        [Route("Blog/List")]
        public async Task<ActionResult<IEnumerable<BlogList>>> GetBlogList(int PartyID)
        {
            if (PartyID < 0)
                return BadRequest(NewReturnMessage("Party ID must be greater than zero."));

            var predicate = PredicateBuilder.New<PartyBlogList>(true);
            predicate = (PartyID > 0) ? predicate.And(x => x.PartyId.Equals(PartyID)) : predicate.And(x => x.PartyId > 0);

            var list = await postgres.PartyBlogLists
                .Where(predicate)
                .Join(postgres.PartyLists, x => x.PartyId, y => y.PartyId, (x, y) => new { x, y })
                .Select(z => new BlogList
                {
                    PartyName = z.y.PartyName,
                    PartyId = z.x.PartyId,
                    PostTitle = z.x.PostName,
                    PostId = z.x.PostId.ToString()
                })
                .OrderBy(z => z.PostTitle)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }
    }
}
