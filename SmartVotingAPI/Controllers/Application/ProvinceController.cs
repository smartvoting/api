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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;

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
