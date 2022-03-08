using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;

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
        public async Task<ActionResult<IEnumerable<ProvinceList>>> GetProvinceList()
        {
            var list = await postgres.ProvinceLists.ToArrayAsync();

            if (list == null)
                return NoContent();

            return Ok(list);
        }

        [HttpGet]
        [Route("{provinceId}")]
        public async Task<ActionResult<IEnumerable<ProvinceList>>> GetProvinceById(int provinceId)
        {
            if (provinceId <= 0)
                return BadRequest();

            var province = await postgres.ProvinceLists.FindAsync(provinceId);

            if (province == null)
                return NoContent();

            return Ok(province);
        }
    }
}
