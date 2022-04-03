using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using System.Text.Json;
using System.Collections;
using Microsoft.Extensions.Options;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Riding")]
    [ApiController]
    public class RidingController : BaseController
    {
        private static HttpClient client = new HttpClient();
        public RidingController(PostgresDbContext _context, IDynamoDBContext _client, IOptions<AppSettings> _app) : base(_context, _client, _app) { }

        #region Routes
        [HttpGet]
        [Route("List")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingList()
        {
            //var list = await postgres.RidingLists.ToArrayAsync();

            var list = await postgres.RidingLists
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
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        #region Locate With Parameter
        [HttpGet]
        [Route("{ridingId}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingById(int ridingId)
        {
            if (ridingId <= 0)
                return BadRequest(new { message = "Invalid riding id number." });

            //var riding = await postgres.RidingLists.FindAsync(ridingId);

            Riding? riding = await GetRidingByIdNumber(ridingId);

            if (riding == null)
                return NoContent();

            return Ok(riding);
        }

        [HttpGet]
        [Route("Locate/City/{city}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingByCity(string city)
        {
            if (String.IsNullOrEmpty(city))
                return BadRequest(NewReturnMessage("A city is required."));

            var ridings = await postgres.OfficeLists
                .Where(o => o.TypeId == 3)
                .Where(o => o.City.ToLower().Equals(city.ToLower()))
                .Join(postgres.RidingLists, o => o.OfficeId, r => r.OfficeId, (o, r) => new { o, r })
                .Join(postgres.ProvinceLists, or => or.o.ProvinceId, p => p.ProvinceId, (or, p) => new { or, p })
                .Select(x => new Riding
                {
                    Id = x.or.r.RidingId,
                    Name = x.or.r.RidingName,
                    Email = x.or.r.RidingEmail,
                    Phone = x.or.r.RidingPhone,
                    Fax = x.or.r.RidingFax,
                    Office = new Office
                    {
                        StreetNumber = x.or.o.StreetNumber,
                        StreetName = x.or.o.StreetName,
                        UnitNumber = x.or.o.UnitNumber,
                        City = x.or.o.City,
                        Province = x.p.ProvinceName,
                        PostCode = x.or.o.PostCode,
                        IsPublic = x.or.o.IsPublic
                    }
                })
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (ridings == null)
                return BadRequest(NewReturnMessage("No ridings found located in the city provided."));

            //foreach (Riding riding in ridings)
            //{
            //    Coordinates gps = await GetRidingCentroid(riding.Id);
            //    if (gps != null)
            //        riding.Centroid = gps;
            //}

            return Ok(ridings);
        }

        [HttpGet]
        [Route("Locate/Candidate/{candidateName}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingByCandidateName(string candidateName)
        {
            if (String.IsNullOrEmpty(candidateName))
                return BadRequest(NewReturnMessage("A candidate name is required."));

            // rold_id for candidates = 5
            var ridings = await postgres.People
                .Where(p => p.RoleId == 5)
                .Where(p => p.FirstName.Contains(candidateName) || p.LastName.Contains(candidateName))
                .Join(postgres.RidingLists, p => p.RidingId, r => r.RidingId, (p, r) => new { p, r })
                .Join(postgres.OfficeLists, pr => pr.r.OfficeId, o => o.OfficeId, (pr, o) => new { pr, o })
                .Join(postgres.ProvinceLists, pro => pro.o.ProvinceId, l => l.ProvinceId, (pro, l) => new { pro, l })
                .Select(x => new Riding
                {
                    Id = x.pro.pr.r.RidingId,
                    Name = x.pro.pr.r.RidingName,
                    Email = x.pro.pr.r.RidingEmail,
                    Phone = x.pro.pr.r.RidingPhone,
                    Fax = x.pro.pr.r.RidingFax,
                    Office = new Office
                    {
                        StreetNumber = x.pro.o.StreetNumber,
                        StreetName = x.pro.o.StreetName,
                        UnitNumber = x.pro.o.UnitNumber,
                        City = x.pro.o.City,
                        Province = x.l.ProvinceName,
                        PostCode = x.pro.o.PostCode,
                        IsPublic = x.pro.o.IsPublic
                    }
                })
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (ridings == null)
                return BadRequest(NewReturnMessage("No ridings found located in the city provided."));

            //foreach (var riding in ridings)
            //{
            //    Coordinates gps = await GetRidingCentroid(riding.Id);
            //    if (gps != null)
            //        riding.Centroid = gps;
            //}

            return Ok(ridings);
        }

        [HttpGet]
        [Route("Locate/Riding/{ridingName}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingByRidingName(string ridingName)
        {
            if (String.IsNullOrEmpty(ridingName))
                return BadRequest(NewReturnMessage("A riding name is required."));

            var ridings = await postgres.RidingLists
                .Where(r => r.RidingName.ToLower().Contains(ridingName.ToLower()))
                .Join(postgres.OfficeLists, r => r.OfficeId, o => o.OfficeId, (r, o) => new { r, o })
                .Join(postgres.ProvinceLists, ro => ro.o.ProvinceId, l => l.ProvinceId, (ro, l) => new { ro, l })
                .Select(x => new Riding
                {
                    Id = x.ro.r.RidingId,
                    Name = x.ro.r.RidingName,
                    Email = x.ro.r.RidingEmail,
                    Phone = x.ro.r.RidingPhone,
                    Fax = x.ro.r.RidingFax,
                    Office = new Office
                    {
                        StreetNumber = x.ro.o.StreetNumber,
                        StreetName = x.ro.o.StreetName,
                        UnitNumber = x.ro.o.UnitNumber,
                        City = x.ro.o.City,
                        Province = x.l.ProvinceName,
                        PostCode = x.ro.o.PostCode,
                        IsPublic = x.ro.o.IsPublic
                    }
                })
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (ridings == null)
                return BadRequest(NewReturnMessage("No ridings found located in the city provided."));

            //foreach (var riding in ridings)
            //{
            //    Coordinates gps = await GetRidingCentroid(riding.Id);
            //    if (gps != null)
            //        riding.Centroid = gps;
            //}

            return Ok(ridings);
        }

        [HttpGet]
        [Route("Locate/PostCode/{postCode}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetRidingByPostCode(string postCode)
        {
            if (String.IsNullOrEmpty(postCode))
                return BadRequest(NewReturnMessage("A postal code is required."));

            HttpResponseMessage mapquestCall = await client.GetAsync(GetMapquestCall(postCode));

            if (mapquestCall.IsSuccessStatusCode)
            {
                var mapquestResponse = await mapquestCall.Content.ReadAsStringAsync();
                JsonDocument mapquestJson = JsonDocument.Parse(mapquestResponse);
                JsonElement mapquestRoot = mapquestJson.RootElement;
                var mapquestElement = mapquestRoot.GetProperty("results")[0].GetProperty("locations")[0].GetProperty("latLng");
                string lat = mapquestElement.GetProperty("lat").ToString();
                string lng = mapquestElement.GetProperty("lng").ToString();

                HttpResponseMessage openNorthCall = await client.GetAsync(GetONBoundariesByCoordCall(lat, lng));

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

                    //Console.WriteLine();

                    if (ridingId == -1)
                        return BadRequest(new { message = "Failed to get riding id number from Open North API." });

                    //var riding = await postgres.RidingLists.FindAsync(ridingId);

                    Riding? riding = await GetRidingByIdNumber(ridingId);

                    if (riding == null)
                        return NoContent();

                    return Ok(riding);
                }

                return BadRequest(new { message = "Open North API call failed." });
            }

            return BadRequest(new { message = "Mapquest API call failed." });
        }
        #endregion

        #region Centroid & Shapes
        [HttpGet]
        [Route("Outline/Centroid/{ridingId}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetCentroidById(int ridingId)
        {
            if (ridingId <= 0)
                return BadRequest(new { message = "Invalid riding id number." });

            HttpResponseMessage openNorthCall = await client.GetAsync(GetONBoundariesByRidingIdCall(ridingId));

            if (openNorthCall.IsSuccessStatusCode)
            {
                var openNorthResponse = await openNorthCall.Content.ReadAsStringAsync();
                JsonDocument openNorthJson = JsonDocument.Parse(openNorthResponse);
                JsonElement openNorthRoot = openNorthJson.RootElement;
                Riding riding = new Riding();
                riding.Id = ridingId;
                riding.Name = openNorthRoot.GetProperty("name").ToString();
                riding.Centroid = new Coordinates
                {
                    Latitude = openNorthRoot.GetProperty("centroid").GetProperty("coordinates")[1].GetDecimal(),
                    Longitude = openNorthRoot.GetProperty("centroid").GetProperty("coordinates")[0].GetDecimal(),
                    Type = openNorthRoot.GetProperty("centroid").GetProperty("type").ToString()
                };

                return Ok(riding);
            }

            return BadRequest(NewReturnMessage("Open North API call failed."));
        }

        [HttpGet]
        [Route("Outline/Shape/{ridingId}")]
        public async Task<ActionResult<IEnumerable<Riding>>> GetShapeById(int ridingId)
        {
            if (ridingId <= 0)
                return BadRequest(new { message = "Invalid riding id number." });

            HttpResponseMessage onBoundaryCall = await client.GetAsync(GetONBoundariesByRidingIdCall(ridingId));
            HttpResponseMessage onShapeCall = await client.GetAsync(GetONBoundariesByRidingIdCall(ridingId, true));

            if (onBoundaryCall.IsSuccessStatusCode && onShapeCall.IsSuccessStatusCode)
            {
                var onBoundaryResponse = await onBoundaryCall.Content.ReadAsStringAsync();
                JsonDocument onBoundaryJson = JsonDocument.Parse(onBoundaryResponse);
                JsonElement onBoundaryRoot = onBoundaryJson.RootElement;

                var onShapeResponse = await onShapeCall.Content.ReadAsStringAsync();
                JsonDocument onShapeJson = JsonDocument.Parse(onShapeResponse);
                JsonElement onShapeRoot = onShapeJson.RootElement;

                var rawCoords = onShapeRoot.GetProperty("coordinates")[0][0].EnumerateArray();
                int length = onShapeRoot.GetProperty("coordinates")[0][0].GetArrayLength();
                Console.WriteLine("Shape Length: " + length);

                ArrayList shape = new ArrayList();

                foreach (JsonElement element in rawCoords)
                {
                    Coordinates point = new Coordinates()
                    {
                        Latitude = element[1].GetDecimal(),
                        Longitude = element[0].GetDecimal()
                    };

                    shape.Add(point);
                }

                Riding riding = new Riding();
                riding.Id = ridingId;
                riding.Name = onBoundaryRoot.GetProperty("name").ToString();
                riding.Outline = shape;

                return Ok(riding);
            }

            return BadRequest(new { message = "Open North API call failed." });
        }
        #endregion

        #endregion

        #region Support Methods
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

            riding.Candidates = candidates;

            return riding;
        }
        #endregion
    }
}
