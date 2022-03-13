using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.ReactObjects;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Province")]
    [ApiController]
    public class ProvinceController : BaseController
    {
        public ProvinceController(PostgresDbContext context) : base(context) { }

        [HttpGet]
        [Route("List")]
        public async Task<ActionResult<IEnumerable<Province>>> GetProvinceList()
        {
            //var list = await postgres.ProvinceLists.ToArrayAsync();

            var list = await postgres.ProvinceLists
                .Select(x => new Province
                {
                    Id = x.ProvinceId,
                    Name = x.ProvinceName
                })
                .OrderBy(z => z.Id)
                .ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("{provinceId}")]
        public async Task<ActionResult<IEnumerable<Province>>> GetProvinceById(int provinceId)
        {
            if (provinceId <= 0)
                return BadRequest(new { message = "Invalid province id number." });

            //var province = await postgres.ProvinceLists.FindAsync(provinceId);

            var province = await postgres.ProvinceLists
                .Where(p => p.ProvinceId == provinceId)
                .Select(x => new Province
                {
                    Id = x.ProvinceId,
                    Name = x.ProvinceName
                })
                .FirstOrDefaultAsync();

            if (province == null)
                return NoContent();

            return Ok(province);
        }
    }
}
