using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using SmartVotingAPI.Models.DTO.Account;
using System.IdentityModel.Tokens.Jwt;
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
        public async Task<IActionResult> SignIn(SignIn data)
        {
            string pwd_hash = GetTextHash(data.Password);

            var person = await postgres.People
                .Where(a => a.PersonId == data.UserId)
                .Where(a => a.PwdHash.Equals(pwd_hash))
                .FirstOrDefaultAsync();

            if (person == null)
                return BadRequest(NewReturnMessage("No user found with the provided ID and password."));

            if (!person.AccountActive)
                return BadRequest(NewReturnMessage("This user account has been disabled. Contact the system administrator for help."));

            var role = await postgres.RoleLists.FindAsync(person.RoleId);

            UserToken user = new()
            {
                UserId = person.PersonId,
                RoleType = role.RoleCode,
                RoleGroup = role.RoleGroup
            };

            string token = CreateToken(user);

            return Ok(token);
        }

        [HttpPut]
        [Route("UpdatePassword")]
        [Authorize(Roles = "EO, PS, LR")]
        public async Task<IActionResult> UpdatePassword(ChangePassword data)
        {
            if (!data.Password.Equals(data.ConfirmPassword))
                return BadRequest(NewReturnMessage("Both passwords must match in order to change your password."));

            int userClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals(ClaimTypes.UserData)).Value);

            if (userClaim <= 0)
                return BadRequest(NewReturnMessage("Invalid user account number."));

            string pwd_hash = GetTextHash(data.Password);

            var person = await postgres.People.FindAsync(userClaim);

            if (person.PwdHash.Equals(pwd_hash))
                return BadRequest(NewReturnMessage("Your new password must be different than your current one."));

            person.PwdHash = pwd_hash;
            postgres.People.Update(person);
            var result = await postgres.SaveChangesAsync();

            if (result != 1)
                return BadRequest(NewReturnMessage("Failed to update password."));

            return Accepted(NewReturnMessage("Password Updated."));
        }

        [HttpDelete]
        [Route("DisableAccount")]
        [Authorize(Roles = "EO, PS, LR")]
        public async Task<IActionResult> DisableAccount(int userId)
        {
            if (userId <= 0)
                return BadRequest(NewReturnMessage("User ID number provided is invalid."));

            int userClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals(ClaimTypes.UserData)).Value);

            if (userClaim == userId)
                return BadRequest(NewReturnMessage("You can not disable your own account."));

            var person = await postgres.People.FindAsync(userId);

            if (person == null)
                return BadRequest(NewReturnMessage("No user found for the provided account number."));

            person.AccountActive = false;
            postgres.People.Update(person);
            var result = await postgres.SaveChangesAsync();

            if (result != 1)
                return BadRequest(NewReturnMessage("Failed to disable the user account."));

            return Ok(NewReturnMessage("User account disabled."));
        }

        private string CreateToken(UserToken user)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Role, user.RoleGroup),
                new Claim(ClaimTypes.Name, user.RoleType),
                new Claim(ClaimTypes.UserData, user.UserId.ToString())
            };

            var credentials = GetSigningCredentials();

            var token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddHours(24),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
