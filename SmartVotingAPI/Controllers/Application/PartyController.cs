using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;

namespace SmartVotingAPI.Controllers.Application
{
    [Route("[controller]")]
    [ApiController]
    public class PartyController : BaseController
    {
        public PartyController(PostgresDbContext context) : base(context) { }

        [HttpGet]
        [Route("List")]
        public async Task<IActionResult> GetAsync()
        {
            var parties = await postgres.PartyLists.ToListAsync();
            return Ok(parties);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return NoContent();
            }

            var party = await postgres.PartyLists.FindAsync(id);
            return Ok(party);
        }
    }
}
