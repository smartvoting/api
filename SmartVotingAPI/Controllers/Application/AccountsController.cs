using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using System.Security.Claims;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Accounts")]
    [ApiController]
    public class AccountsController : BaseController
    {
        public AccountsController(PostgresDbContext _context, IOptions<AppSettings> _app) : base(_context, _app) { }

        [HttpPost]
        [Route("SignIn")]
        public async Task<IActionResult> SignIn(Account data)
        {
            string pwd_hash = GetTextHash(data.Password);

            var person = await postgres.People
                .Where(a => a.PersonId == data.UserId)
                .Where(a => a.PwdHash.Equals(pwd_hash))
                .FirstOrDefaultAsync();

            if (person == null)
                return BadRequest(NewReturnMessage("No user found with the provided ID and password."));

            var role = await postgres.RoleLists.FindAsync(person.RoleId);

            return Ok();
        }

        [HttpPut]
        [Route("UpdatePassword")]
        [Authorize(Roles = "EO, PS, LR")]
        public async Task<IActionResult> UpdatePassword(Account data)
        {
            if (!data.Password.Equals(data.ConfirmPassword))
                return BadRequest(NewReturnMessage("Both passwords must match in order to change your password."));

            string pwd_hash = GetTextHash(data.Password);

            var person = await postgres.People.FindAsync(data.UserId);
            person.PwdHash = pwd_hash;
            postgres.People.Update(person);
            await postgres.SaveChangesAsync();

            return Ok(NewReturnMessage("Password Updated."));
        }

        //private string CreateToken(Account account)
        //{
        //    List<Claim> claims = new()
        //    {
        //        new Claim(ClaimTypes.GroupSid = account.RoleGroup.ToString()),
        //        new Claim(ClaimTypes.Role = account.RoleType.ToString())
        //    };
        //}
    }
}
