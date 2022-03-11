using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.ReactObjects;
using System.Text.Json;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Riding")]
    [ApiController]
    public class RidingController : BaseController
    {
        private static HttpClient client = new HttpClient();
        public RidingController(PostgresDbContext _context, IDynamoDBContext _client) : base(_context, _client) { }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingList()
        {
            //var list = await postgres.RidingLists.ToArrayAsync();

            var list = await postgres.RidingLists
                .Join(postgres.OfficeLists, r => r.OfficeId, o => o.OfficeId, (r, o) => new { r, o })
                .Join(postgres.OfficeTypes, ro => ro.o.TypeId, t => t.TypeId, (ro, t) => new {ro, t})
                .Join(postgres.ProvinceLists, rot => rot.ro.o.ProvinceId, p => p.ProvinceId, (rot, p) => new {rot, p})
                .Select(x => new Riding
                {
                    Id = x.rot.ro.r.RidingId,
                    Name = x.rot.ro.r.RidingName,
                    Email = x.rot.ro.r.RidingEmail,
                    Phone = x.rot.ro.r.RidingPhone,
                    Fax = x.rot.ro.r.RidingFax,
                    Office = new Office
                    {
                        Type = x.rot.t.TypeName,
                        StreetNumber = x.rot.ro.o.StreetNumber,
                        StreetName = x.rot.ro.o.StreetName,
                        UnitNumber = x.rot.ro.o.UnitNumber,
                        City = x.rot.ro.o.City,
                        Province = x.p.ProvinceName,
                        PostCode = x.rot.ro.o.PostCode,
                        PoBox = x.rot.ro.o.PoBox,
                        IsPublic = x.rot.ro.o.IsPublic
                    }
                })
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("{ridingId}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingById(int ridingId)
        {
            if (ridingId <= 0)
                return BadRequest();

            //var riding = await postgres.RidingLists.FindAsync(ridingId);

            Riding? riding = await GetRidingByIdNumber(ridingId);

            if (riding == null)
                return NoContent();

            return Ok(riding);
        }

        [HttpGet]
        [Route("Locate/{postCode}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingByPostCode(string postCode)
        {
            if (String.IsNullOrEmpty(postCode))
                return BadRequest();

            HttpResponseMessage mapquestCall = await client.GetAsync(GetMapquestCall(postCode));
            
            if (mapquestCall.IsSuccessStatusCode)
            {
                var mapquestResponse = await mapquestCall.Content.ReadAsStringAsync();
                JsonDocument mapquestJson = JsonDocument.Parse(mapquestResponse);
                JsonElement mapquestRoot = mapquestJson.RootElement;
                var mapquestElement = mapquestRoot.GetProperty("results")[0].GetProperty("locations")[0].GetProperty("latLng");
                string lat = mapquestElement.GetProperty("lat").ToString();
                string lng = mapquestElement.GetProperty("lng").ToString();

                HttpResponseMessage openNorthCall = await client.GetAsync(GetOpenNorthBoundariesCall(lat, lng));

                if (openNorthCall.IsSuccessStatusCode)
                {
                    var openNorthResponse = await openNorthCall.Content.ReadAsStringAsync();
                    JsonDocument openNorthJson = JsonDocument.Parse(openNorthResponse);
                    JsonElement openNorthRoot = openNorthJson.RootElement;
                    var openNorthObjects = openNorthRoot.GetProperty("objects").EnumerateArray();

                    string key = "/boundaries/federal-electoral-districts/";
                    int ridingId = -1;

                    foreach (JsonElement element in openNorthObjects)
                    {
                        string url = element.GetProperty("url").ToString();
                        if (url.Contains(key))
                        {
                            ridingId = Convert.ToInt32(url.Substring(url.Length - 6, 5));
                            break;
                        }
                    }

                    Console.WriteLine();

                    if (ridingId == -1)
                        return BadRequest();

                    //var riding = await postgres.RidingLists.FindAsync(ridingId);

                    Riding? riding = await GetRidingByIdNumber(ridingId);

                    if (riding == null)
                        return NoContent();

                    return Ok(riding);
                }

                return BadRequest();
            }

            return BadRequest();
        }

        private async Task<Riding>? GetRidingByIdNumber(int id)
        {
            var riding = await postgres.RidingLists
                .Where(r => r.RidingId == id)
                .Join(postgres.OfficeLists, r => r.OfficeId, o => o.OfficeId, (r, o) => new { r, o })
                .Join(postgres.OfficeTypes, ro => ro.o.TypeId, t => t.TypeId, (ro, t) => new { ro, t })
                .Join(postgres.ProvinceLists, rot => rot.ro.o.ProvinceId, p => p.ProvinceId, (rot, p) => new { rot, p })
                .Select(x => new Riding
                {
                    Id = x.rot.ro.r.RidingId,
                    Name = x.rot.ro.r.RidingName,
                    Email = x.rot.ro.r.RidingEmail,
                    Phone = x.rot.ro.r.RidingPhone,
                    Fax = x.rot.ro.r.RidingFax,
                    Office = new Office
                    {
                        Type = x.rot.t.TypeName,
                        StreetNumber = x.rot.ro.o.StreetNumber,
                        StreetName = x.rot.ro.o.StreetName,
                        UnitNumber = x.rot.ro.o.UnitNumber,
                        City = x.rot.ro.o.City,
                        Province = x.p.ProvinceName,
                        PostCode = x.rot.ro.o.PostCode,
                        PoBox = x.rot.ro.o.PoBox,
                        IsPublic = x.rot.ro.o.IsPublic
                    }
                })
                .FirstOrDefaultAsync();

            if (riding == null)
                return null;

            var candidates = await postgres.People
                .Where(p => p.RidingId == id && p.RoleId == 5)
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

            riding.Candidates = candidates;

            return riding;
        }
    }
}
