using Amazon.QLDB.Driver;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using SmartVotingAPI.Models.DTO.Vote;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Amazon.QLDB.Driver.Generic;
using Amazon.QLDB.Driver.Serialization;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Vote")]
    [ApiController]
    //[Authorize]
    [Authorize(Roles = "Voter")]
    public class VoteController : BaseController
    {
        private const string tokenIssuer = "api.smartvoting.cc";
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
                .Where(a => !a.VoteCast)
                .Select(a => new VoterToken
                {
                    VoterId = a.VoterId.ToString(),
                    RidingId = a.RidingId
                })
                .FirstOrDefaultAsync();

            if (voter == null)
                return BadRequest(NewReturnMessage("No voter found with the provided information."));

            string token = CreateVoterToken(voter);

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

            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());

            //var security = await postgres.VoterSecurities
            //    .Where(a => a.VoterId.Equals(voterClaim))
            //    .Where(a => a.CardId.Equals(data.CardId))
            //    .Where(a => a.CardPin == data.CardPin)
            //    .Where(a => a.Sin == data.SinDigits)
            //    .FirstOrDefaultAsync();

            var security = await postgres.VoterSecurities
                .Where(a => a.VoterId.Equals(voterClaim))
                .Select(a => new
                {
                    a.CardId,
                    a.CardPin,
                    a.Sin
                })
                .FirstOrDefaultAsync();

            if (security == null)
                return BadRequest(NewReturnMessage("No voter found with the provided information."));

            bool validRequest = security.CardId.Equals(data.CardId) && security.CardPin.Equals(data.CardPin) && security.Sin.Equals(data.SinDigits);

            if (!validRequest)
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

            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());

            var tax = await postgres.VoterSecurities
                .Where(a => a.VoterId.Equals(voterClaim))
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

            if (!lineOne || !lineTwo || !lineThree)
                return BadRequest(NewReturnMessage("Tax information did not match records."));

            Random random = new Random();
            int pin = random.Next(10000000, 100000000);
            var voter = await postgres.VoterSecurities.FindAsync(voterClaim);
            voter.EmailPin = pin;
            postgres.VoterSecurities.Update(voter);
            await postgres.SaveChangesAsync();

            var email = await postgres.VoterLists
                .Where(a => a.VoterId.Equals(voterClaim))
                .Select(a => new
                { 
                    a.EmailAddress
                })
                .FirstOrDefaultAsync();

            if (email == null)
                return BadRequest(NewReturnMessage("Voter email address not found."));

            JsonObject json = new()
            {
                ["pin"] = pin
            };

            string emailData = JsonSerializer.Serialize(json);

            //bool emailSent = await SendEmailSES(email, "vote-email-pin", emailData);
            bool emailSent = true;

            //string body = VotePinEmail(pin);

            //bool emailSent = await SendEmailSES(email, "Voter Authentication PIN", body);

            if (emailSent)
                return Ok();

            return BadRequest(NewReturnMessage("Error sending authentication pin."));


        }
        #endregion

        #region Step Four
        [HttpPost]
        [Route("Step/4")]
        public async Task<ActionResult<IEnumerable<Candidate>>> StepFour(StepFour data)
        {
            string message = await SecurityChecks(data.RemoteIp, data.ApiKey);
            if (message != null)
                return Unauthorized(NewReturnMessage(message));

            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());
            int ridingClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString());
            const int candidateRoleId = 5;

            var pin = await postgres.VoterSecurities
                .Where(a => a.VoterId.Equals(voterClaim))
                .Select(a => a.EmailPin)
                .FirstOrDefaultAsync();

            if (pin == data.EmailPin && ridingClaim > 0)
            {
                var list = await postgres.People
                    .Where(a => a.RoleId == candidateRoleId)
                    .Where(a => a.RidingId == ridingClaim)
                    .Join(postgres.PartyLists, a => a.PartyId, b => b.PartyId, (a, b) => new { a, b })
                    .Join(postgres.RidingLists, ab => ab.a.RidingId, c => c.RidingId, (ab, c) => new { ab, c })
                    .Select(z => new Candidate
                    {
                        CandidateId = z.ab.a.PersonId,
                        FirstName = z.ab.a.FirstName,
                        LastName = z.ab.a.LastName,
                        PartyId = z.ab.a.PartyId,
                        PartyName = z.ab.b.PartyName,
                        RidingId = ridingClaim,
                        RidingName = z.c.RidingName
                    })
                    .ToArrayAsync();

                if (list == null)
                    return BadRequest(NewReturnMessage("An error occurred getting the riding candidates."));

                return Ok(list);
            }

            return BadRequest(NewReturnMessage("The email pin provided does not match the pin on record."));
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

            string voterClaim = User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString();
            int ridingClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString());

            VoterToken voter = new()
            {
                VoterId = voterClaim,
                RidingId = ridingClaim
            };

            VoteToken vote = new()
            {
                RidingId = data.RidingId,
                CandidateId = data.CandidateId
            };

            string token = CreateVoteToken(voter, vote);

            return Ok(token);
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


            if (!data.UserConfirmation)
                return BadRequest(NewReturnMessage("Please confirm your vote. You can NOT change it once you have confirmed."));

            VoterToken voter = new()
            {
                VoterId = User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString(),
                RidingId = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString())
            };

            VoteToken vote = new()
            {
                CandidateId = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("candidate")).Value.ToString()),
                RidingId = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString())
            };

            //Console.WriteLine();

            //IAsyncQldbDriver driver = AsyncQldbDriver.Builder().WithLedger(appSettings.Value.Vote.LedgerID).Build();
            IAsyncQldbDriver driver = AsyncQldbDriver.Builder()
                .WithLedger(appSettings.Value.Vote.LedgerID)
                .WithSerializer(new ObjectSerializer())
                .Build();

            //IAsyncResult<VoterToken> voterCheck = await driver.Execute(async txn =>
            //{
            //    //string query = "SELECT * FROM voters WHERE VoterId = " + voter.VoterId;
            //    return await txn.Execute(txn.Query<VoterToken>(query));
            //});

            //Console.WriteLine();

            var result = await driver.Execute(async txn =>
            {
                IQuery<VoterToken> voterQuery = txn.Query<VoterToken>("INSERT INTO Voters ?", voter);
                IQuery<VoteToken> voteQuery = txn.Query<VoteToken>("INSERT INTO Ballots ?", vote);
                await txn.Execute(voterQuery);
                await txn.Execute(voteQuery);
                return true;
            });

            Console.WriteLine();

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

        private string CreateVoterToken(VoterToken voter)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("voter", voter.VoterId),
                new Claim("riding", voter.RidingId.ToString()),
                new Claim("election", appSettings.Value.Vote.ElectionID.ToString()),
                new Claim(ClaimTypes.Role, "Voter")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettings.Value.Vote.TokenSignature));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddDays(14),
                //expires: DateTime.Now.AddMinutes(15),
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private string CreateVoteToken(VoterToken voter, VoteToken vote)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("voter", voter.VoterId),
                new Claim("candidate", vote.CandidateId.ToString()),
                new Claim("riding", vote.RidingId.ToString()),
                new Claim("election", appSettings.Value.Vote.ElectionID.ToString()),
                new Claim("timestamp", DateTime.Now.ToString()),
                new Claim(ClaimTypes.Role, "Voter")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettings.Value.Vote.TokenSignature));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddDays(14),
                //expires: DateTime.Now.AddMinutes(10),
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
