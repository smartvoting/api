﻿using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Dynamo;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.ReactObjects;
using System.Text.Json;

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
            //var list = await postgres.PartyLists.ToArrayAsync();

            var list = await postgres.PartyLists
                .Where(p => p.PartyId > 0)
                .Join(postgres.OfficeLists, p => p.OfficeId, o => o.OfficeId, (p, o) => new { p, o })
                .Join(postgres.OfficeTypes, po => po.o.TypeId, t => t.TypeId, (po, t) => new { po, t })
                .Join(postgres.SocialMediaLists, pot => pot.po.p.SocialId, s => s.SocialId, (pot, s) => new { pot, s })
                .Join(postgres.ProvinceLists, pots => pots.pot.po.o.ProvinceId, l => l.ProvinceId, (pots, l) => new {pots, l})
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
                return BadRequest();

            //var party = await postgres.PartyLists.FindAsync(partyId);

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
                .Join(postgres.OfficeLists, ps => ps.p.OfficeId, o => o.OfficeId, (ps, o) => new { ps, o })
                .Join(postgres.OfficeTypes, pso => pso.o.TypeId, t => t.TypeId, (pso, t) => new { pso, t })
                .Join(postgres.ProvinceLists, psot => psot.pso.o.ProvinceId, n => n.ProvinceId, (psot, n) => new { psot, n })
                .Select(x => new Models.ReactObjects.Person
                {
                    FirstName = x.psot.pso.ps.p.FirstName,
                    LastName = x.psot.pso.ps.p.LastName,
                    EmailAddress = x.psot.pso.ps.p.EmailAddress,
                    PhoneNumber = x.psot.pso.ps.p.PhoneNumber,
                    Office = new Office
                    {
                        Type = x.psot.t.TypeName,
                        StreetNumber = x.psot.pso.o.StreetNumber,
                        StreetName = x.psot.pso.o.StreetName,
                        UnitNumber = x.psot.pso.o.UnitNumber,
                        City = x.psot.pso.o.City,
                        Province = x.n.ProvinceName,
                        PostCode = x.psot.pso.o.PostCode,
                        PoBox = x.psot.pso.o.PoBox,
                        IsPublic = x.psot.pso.o.IsPublic
                    },
                    SocialMedia = new SocialMedia
                    {
                        TwitterId = x.psot.pso.ps.s.TwitterId,
                        InstagramId = x.psot.pso.ps.s.InstagramId,
                        FacebookId = x.psot.pso.ps.s.FacebookId,
                        YoutubeId = x.psot.pso.ps.s.YoutubeId,
                        SnapchatId = x.psot.pso.ps.s.SnapchatId,
                        FlickrId = x.psot.pso.ps.s.FlickrId,
                        TiktokId = x.psot.pso.ps.s.TiktokId
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
                return BadRequest();

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
                return BadRequest();

            var issue = await dynamo.LoadAsync<PartyPlatform>(partyId, topicId);

            if (issue == null)
                return NoContent();

            return Ok(issue);
        }
    }
}
