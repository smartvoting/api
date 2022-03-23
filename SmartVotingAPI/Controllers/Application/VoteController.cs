using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO.Vote;
using SmartVotingAPI.Models.Postgres;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Vote")]
    [ApiController]
    //[Authorize]
    [Authorize(Roles = "Voter")]
    public class VoteController : BaseController
    {
        private static HttpClient client = new HttpClient();

        public VoteController(PostgresDbContext _context, IOptions<AppSettings> _app) : base(_context, _app) { }

        #region Step One
        [HttpPost]
        [Route("Step/1")]
        [AllowAnonymous]
        public async Task<IActionResult> StepOne(StepOne data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            var voter = await postgres.VoterLists
                .Where(a => a.FirstName.Equals(data.FirstName))
                //.Where(a => a.MiddleName.Equals(data.MiddleName))
                .Where(a => a.LastName.Equals(data.LastName))
                //.Where(a => Equals(a.BirthDate, data.BirthDate))
                .Where(a => a.Gender == data.Gender)
                .Where(a => a.StreetNumber == data.StreetNumber)
                .Where(a => a.StreetName.Equals(data.StreetName))
                //.Where(a => a.UnitNumber == data.UnitNumber)
                .Where(a => a.City.Equals(data.City))
                .Where(a => a.ProvinceId == data.Province)
                .Where(a => a.PostCode.Equals(data.PostCode))
                .Select(a => new VoterToken
                {
                    VoterId = a.VoterId.ToString(),
                    RidingId = a.RidingId
                })
                .FirstOrDefaultAsync();

            if (voter == null)
                return BadRequest(NewReturnMessage("No voter found with the provided information."));

            string token = CreateToken(voter);

            return Ok(token);
        }
        #endregion

        #region Step Two
        [HttpPost]
        [Route("Step/2")]
        public async Task<IActionResult> StepTwo(StepTwo data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            string voterClaim = User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString();

            var security = await postgres.VoterSecurities
                .Where(a => a.VoterId.Equals(voterClaim))
                .Where(a => a.CardId.Equals(data.CardId))
                .Where(a => a.CardPin == data.CardPin)
                .Where(a => a.Sin == data.SinDigits)
                .FirstOrDefaultAsync();

            if (security == null)
                return BadRequest(NewReturnMessage("No voter found with the provided information."));

            return Ok();
        }
        #endregion

        #region Step Three
        [HttpPost]
        [Route("Step/3")]
        public async Task<IActionResult> StepThree(StepThree data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            string voterClaim = User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString();

            var tax = await postgres.VoterSecurities
                .Where(a => a.VoterId.ToString().Equals(voterClaim))
                .Select(a => new int[]
                {
                    a.Tax10100,
                    a.Tax12000,
                    a.Tax12900,
                    a.Tax14299,
                    a.Tax15000,
                    a.Tax23600,
                    a.Tax24400,
                    a.Tax26000,
                    a.Tax31220,
                    a.Tax58240
                })
                .FirstOrDefaultAsync();

            bool lineOne = tax[data.LineOne.LineNumber] == data.LineOne.LineValue;
            bool lineTwo = tax[data.LineTwo.LineNumber] == data.LineTwo.LineValue;
            bool lineThree = tax[data.LineThree.LineNumber] == data.LineThree.LineValue;

            if (lineOne && lineTwo && lineThree)
            {
                Random random = new Random();
                int pin = random.Next(10000000, 100000000);
                var entry = await postgres.VoterSecurities.SingleAsync(a => a.VoterId.ToString().Equals(voterClaim));
                entry.EmailPin = pin;
                postgres.VoterSecurities.Update(entry);
                await postgres.SaveChangesAsync();

                string? email = await postgres.VoterLists.Where(a => a.VoterId.ToString().Equals(voterClaim)).Select(a => a.EmailAddress).FirstOrDefaultAsync();

                if (email == null)
                    return BadRequest(NewReturnMessage("Voter email address not found."));

                JsonObject json = new()
                {
                    ["pin"] = pin
                };

                string emailData = JsonSerializer.Serialize(json);

                bool emailSent = await SendEmailSES(email, "vote-email-pin", emailData);

                //string body = VotePinEmail(pin);

                //bool emailSent = await SendEmailSES(email, "Voter Authentication PIN", body);

                if (emailSent)
                    return Ok();

                return BadRequest(NewReturnMessage("Error sending authentication pin."));
            }

            return BadRequest(NewReturnMessage("Tax information did not match records."));
        }
        #endregion

        #region Step Four
        [HttpPost]
        [Route("Step/4")]
        public async Task<IActionResult> StepFour(StepFour data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            return Ok();
        }
        #endregion

        #region Step Five
        [HttpPost]
        [Route("Step/5")]
        public async Task<IActionResult> StepFive(StepFive data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            return Ok();
        }
        #endregion

        #region Step Six
        [HttpPost]
        [Route("Step/6")]
        public async Task<IActionResult> StepSix(StepSix data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            return Ok();
        }
        #endregion

        #region Helper Methods
        private async Task<string> SecurityChecks(string ip, string key)
        {
            IPAddress address;
            bool isValidIP = IPAddress.TryParse(ip, out address);

            if (!isValidIP)
                return "IP Address is not valid.";

            string baseURL = "https://vpnapi.io/api/";
            string callURL = String.Format("{0}{1}?key={2}", baseURL, ip, appSettings.Value.API.VPN);

            HttpResponseMessage call = await client.GetAsync(callURL);

            if (call.IsSuccessStatusCode)
            {
                var response = await call.Content.ReadAsStringAsync();
                JsonElement root = JsonDocument.Parse(response).RootElement;
                bool vpn = bool.Parse(root.GetProperty("security").GetProperty("vpn").ToString());
                bool proxy = bool.Parse(root.GetProperty("security").GetProperty("proxy").ToString());
                bool tor = bool.Parse(root.GetProperty("security").GetProperty("tor").ToString());
                bool relay = bool.Parse(root.GetProperty("security").GetProperty("relay").ToString());
                if (vpn || proxy || tor || relay)
                    return "Please disconnect from your VPN, Proxy, Tor or Relay service to continue.";
            }

            var apiResult = await postgres.ApiKeys
                .Where(a => a.IsActive)
                .Where(a => a.AuthKey.ToString() == key)
                .Where(a => a.KeyId.ToString() == appSettings.Value.Vote.VoteKeyID)
                .FirstOrDefaultAsync();

            if ((appSettings.Value.Environment.Equals("development") && apiResult.IsDevelopment) || (appSettings.Value.Environment.Equals("production") && apiResult.IsProduction))
                    return "Invalid API Key Provided.";

            return null;
        }

        private string CreateToken(VoterToken voter)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("voter", voter.VoterId),
                new Claim("riding", voter.RidingId.ToString()),
                new Claim(ClaimTypes.Role, "Voter")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettings.Value.Vote.TokenSignature));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: "api.smartvoting.cc",
                claims: claims,
                expires: DateTime.Now.AddDays(14),
                //expires: DateTime.Now.AddMinutes(15),
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        #endregion

        #region Email Body Templates
        private string VotePinEmail(int pin)
        {
            string body = "Your voting pin is: " + pin.ToString() + "\n\nDo not share this pin with anyone. It is needed on step 4 to cast your vote and is unique to you. If someone else has this pin, they may be able to vote on your behalf.";
            return body;
        }
        #endregion
    }
}
