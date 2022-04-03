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

using Amazon.QLDB.Driver;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using SmartVotingAPI.Models.DTO.Vote;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Amazon.QLDB.Driver.Serialization;
using SmartVotingAPI.Models.QLDB;
using Microsoft.AspNetCore.Cors;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Vote")]
    [ApiController]
    [Authorize(Roles = "Voter")]
    [EnableCors("corsvoting")]
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
            //DateTime timestamp = DateTime.Now;
            bool validIp = await IsValidIpAddress(data.RemoteIp);
            bool validElection = await IsActiveElection(data.AuthKey);
            if (!validIp || !validElection)
                return Unauthorized();

            if (!data.IsCitizen)
                return Unauthorized();

            DateOnly dob = DateOnly.FromDateTime(data.BirthDate);

            var voterEntry = await postgres.VoterLists
                .Where(a => a.FirstName.Equals(data.FirstName))
                .Where(a => a.LastName.Equals(data.LastName))
                .Where(a => a.BirthDate.CompareTo(dob) == 0)
                .Where(a => a.Gender == data.Gender)
                .Where(a => a.StreetNumber == data.StreetNumber)
                .Where(a => a.StreetName.Equals(data.StreetName))
                .Where(a => a.City.Equals(data.City))
                .Where(a => a.ProvinceId == data.Province)
                .Where(a => a.PostCode.Equals(data.PostCode))
                .FirstOrDefaultAsync();

            if (voterEntry == null)
                return NoContent();

            if (voterEntry.VoteCast)
                return Unauthorized();

            bool verifiedMiddleName = false;
            bool verifiedUnitNumber = false;

            if (!string.IsNullOrEmpty(voterEntry.MiddleName) && !string.IsNullOrEmpty(data.MiddleName))
                verifiedMiddleName = voterEntry.MiddleName.Equals(data.MiddleName);

            if (!string.IsNullOrEmpty(voterEntry.UnitNumber) && !string.IsNullOrEmpty(data.UnitNumber))
                verifiedUnitNumber = voterEntry.UnitNumber.Equals(data.UnitNumber);

            VoterToken voter = new()
            {
                VoterId = voterEntry.VoterId.ToString(),
                RidingId = voterEntry.RidingId
            };

            string token = CreateToken(voter);

            return Ok(token);
        }
        #endregion

        #region Step Two
        [HttpPost]
        [Route("Step/2")]
        public async Task<IActionResult> StepTwo(StepTwo data)
        {
            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());

            bool validIp = await IsValidIpAddress(data.RemoteIp);
            bool validAuthKey = await IsValidAuthKey(data.AuthKey);
            bool validVoterClaim = IsValidVoterGuid(voterClaim);
            if (!validIp || !validAuthKey)
                return Unauthorized();
            if (!validVoterClaim)
                return BadRequest();

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
                return BadRequest();

            bool validRequest = security.CardId.Equals(data.CardId) && security.CardPin.Equals(data.CardPin) && security.Sin.Equals(data.SinDigits);

            if (!validRequest)
                return BadRequest();

            return Ok();
        }
        #endregion

        #region Step Three
        [HttpPost]
        [Route("Step/3")]
        public async Task<IActionResult> StepThree(StepThree data)
        {
            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());

            bool validIp = await IsValidIpAddress(data.RemoteIp);
            bool validAuthKey = await IsValidAuthKey(data.AuthKey);
            bool validVoterClaim = IsValidVoterGuid(voterClaim);

            if (!validIp || !validAuthKey)
                return Unauthorized();
            if (!validVoterClaim)
                return BadRequest();

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
                return BadRequest();

            Random random = new Random();
            int pin = random.Next(10000000, 100000000);
            var voter = await postgres.VoterSecurities.FindAsync(voterClaim);
            voter.EmailPin = pin;
            postgres.VoterSecurities.Update(voter);
            await postgres.SaveChangesAsync();

            var person = await postgres.VoterLists
                .Where(a => a.VoterId.Equals(voterClaim))
                .Select(a => new
                { 
                    a.FirstName,
                    a.LastName,
                    a.EmailAddress
                })
                .FirstOrDefaultAsync();

            if (person == null)
                return BadRequest();

            string name = person.FirstName + " " + person.LastName;
            string subject = "Email PIN Verification - Smart Voting CC";
            string body = "<h1>Smart Voting CC</h1>" +
                "<hr/>" +
                "<p>Your voter email verification pin is: <strong><code>" + pin + "</code></strong></p>" +
                "<hr/>" +
                "<h3>DO NOT SHARE THIS PIN NUMBER WITH ANYONE</h3>";

            bool result = await SendEmailSES(name, person.EmailAddress, subject, body);

            if (!result)
                return BadRequest();

            return Ok();
        }
        #endregion

        #region Step Four
        [HttpPost]
        [Route("Step/4")]
        public async Task<ActionResult<IEnumerable<Candidate>>> StepFour(StepFour data)
        {
            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());
            int ridingClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString());

            bool validIp = await IsValidIpAddress(data.RemoteIp);
            bool validAuthKey = await IsValidAuthKey(data.AuthKey);
            bool validVoterClaim = IsValidVoterGuid(voterClaim);

            if (!validIp || !validAuthKey)
                return Unauthorized();
            if (!validVoterClaim)
                return BadRequest();

            bool response = await VerifyHcaptcha(data.HcaptchaResponse, data.RemoteIp);
            if (!response)
                return Unauthorized(NewReturnMessage("hCaptcha Failed 4.1"));

            const int candidateRoleId = 5;

            var pin = await postgres.VoterSecurities
                .Where(a => a.VoterId.Equals(voterClaim))
                .Select(a => a.EmailPin)
                .FirstOrDefaultAsync();

            if (pin != data.EmailPin || ridingClaim <= 0)
                return BadRequest();

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
                return BadRequest();

            return Ok(list);
        }
        #endregion

        #region Step Five
        [HttpPost]
        [Route("Step/5")]
        public async Task<IActionResult> StepFive(StepFive data)
        {
            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());
            int ridingClaim = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString());
            int candidateId = data.CandidateId;

            bool validIp = await IsValidIpAddress(data.RemoteIp);
            bool validAuthKey = await IsValidAuthKey(data.AuthKey);
            bool validVoterClaim = IsValidVoterGuid(voterClaim);

            if (!validIp || !validAuthKey)
                return Unauthorized();
            if (!validVoterClaim)
                return BadRequest();

            var candidate = await postgres.People
                .Where(a => a.PersonId == candidateId)
                .Where(a => a.RidingId == ridingClaim)
                .FirstOrDefaultAsync();

            if (candidate == null)
                return BadRequest();
            
            VoterToken voter = new()
            {
                VoterId = voterClaim.ToString(),
                RidingId = ridingClaim
            };

            BallotToken vote = new()
            {
                CandidateId = candidateId,
                RidingId = ridingClaim
            };

            string token = CreateToken(voter, vote);

            return Ok(token);
        }
        #endregion

        #region Step Six
        [HttpPost]
        [Route("Step/6")]
        public async Task<IActionResult> StepSix(StepSix data)
        {
            Guid voterClaim = Guid.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString());
            string voterId = User.Claims.FirstOrDefault(a => a.Type.Equals("voter")).Value.ToString();
            int candidateId = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("candidate")).Value.ToString());
            int ridingId = int.Parse(User.Claims.FirstOrDefault(a => a.Type.Equals("riding")).Value.ToString());

            bool validIp = await IsValidIpAddress(data.RemoteIp);
            bool validAuthKey = await IsValidAuthKey(data.AuthKey);
            bool validVoterClaim = IsValidVoterGuid(voterClaim);

            if (!validIp || !validAuthKey)
                return Unauthorized();
            if (!validVoterClaim)
                return BadRequest();
            if (candidateId <= 0)
                return BadRequest();
            if (ridingId <= 0)
                return BadRequest();

            bool response = await VerifyHcaptcha(data.HcaptchaResponse, data.RemoteIp);
            if (!response)
                return Unauthorized(NewReturnMessage("hCaptcha Failed 6.1"));

            if (!data.UserConfirmation)
                return BadRequest();

            IAsyncQldbDriver driver = AsyncQldbDriver.Builder()
                .WithLedger(appSettings.Value.Vote.LedgerID)
                .WithSerializer(new ObjectSerializer())
                .Build();

            try
            {
                var readResult = await driver.Execute(async txn =>
                {
                    IQuery<VoterToken> query = txn.Query<VoterToken>("SELECT * FROM Voters WHERE VoterId = ?", voterId);
                    var result = await txn.Execute(query);
                    return await result.ToArrayAsync();
                });

                if (readResult.Length > 0)
                    return Unauthorized(NewReturnMessage("A vote has already been recorded by this voter. The vote will not be saved or changed."));
            } catch (Exception e)
            {
                Console.WriteLine("=== START =================================================");
                Console.WriteLine("BREAK - 6.1");
                Console.WriteLine("E: " + e.Message);
                Console.WriteLine("=== END ===================================================");
                return BadRequest();
            }

            DateTime timestamp = DateTime.Now;

            VoterToken voter = new()
            {
                VoterId = voterId,
                RidingId = ridingId,
                Timestamp = timestamp,
                IpAddress = data.RemoteIp
            };

            BallotToken vote = new()
            {
                CandidateId = candidateId,
                RidingId = ridingId,
                Timestamp = timestamp
            };

            try
            {
                await driver.Execute(async txn =>
                {
                    IQuery<VoterToken> voterQuery = txn.Query<VoterToken>("INSERT INTO Voters ?", voter);
                    IQuery<BallotToken> voteQuery = txn.Query<BallotToken>("INSERT INTO Ballots ?", vote);
                    await txn.Execute(voterQuery);
                    await txn.Execute(voteQuery);
                });
            } catch (Exception e)
            {
                Console.WriteLine("=== START =================================================");
                Console.WriteLine("BREAK - 6.2");
                Console.WriteLine("E: " + e.Message);
                Console.WriteLine("=== END ===================================================");
                return BadRequest();
            }

            var voterEntry = await postgres.VoterLists.FindAsync(Guid.Parse(voter.VoterId));
            voterEntry.VoteCast = true;
            postgres.VoterLists.Update(voterEntry);
            await postgres.SaveChangesAsync();

            var candidate = await postgres.People.FindAsync(candidateId);
            var riding = await postgres.RidingLists.FindAsync(ridingId);

            string name = voterEntry.FirstName + " " + voterEntry.LastName;
            string subject = "Vote Confirmation - Smart Voting CC";
            string body = "<h1>Smart Voting CC</h1>" +
                "<hr/>" +
                "<p>You have successfully cast your ballot in the election.</p>" +
                "<p>You Voted For: <strong>" + candidate.FirstName + " " + candidate.LastName + "</strong></p>" +
                "<p>Your Riding Is: <strong>" + riding.RidingName + "</strong></p>" +
                "<p>Your Voter ID Number: <strong>" + voterId.ToUpper() + "</strong></p>" +
                "<hr/>" +
                "<h3>DO NOT SHARE THIS EMAIL WITH ANYONE</h3>";

            bool result = await SendEmailSES(name, voterEntry.EmailAddress, subject, body);

            if (!result)
                return BadRequest();

            return Ok();
        }
        #endregion

        #region Helper Methods
        // Returns true is validation is successful otherwise false
        private async Task<bool> IsValidIpAddress(string ip)
        {
            IPAddress address;
            bool isValidIP = IPAddress.TryParse(ip, out address);

            if (!isValidIP)
                return false;

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
                    return false;
                //return "Please disconnect from your VPN, Proxy, Tor or Relay service to continue.";
            }

            return true;
        }

        // Returns true is validation is successful otherwise false
        private async Task<bool> IsValidAuthKey(string key)
        {
            Guid publicKey = Guid.Parse(key);
            Guid secretKey = Guid.Parse(appSettings.Value.Vote.SecretKey);

            var result = await postgres.ElectionTokens.FindAsync(publicKey);

            if (result == null)
                return false;

            if (!result.IsActive)
                return false;

            if (result.SecretId.CompareTo(secretKey) != 0)
                return false;

            return true;
        }

        // Returns true is validation is successful otherwise false
        private async Task<bool> IsActiveElection(string key)
        {
            Guid publicKey = Guid.Parse(key);
            Guid secretKey = Guid.Parse(appSettings.Value.Vote.SecretKey);
            //DateOnly date = DateOnly.FromDateTime(timestamp);
            //TimeOnly time = TimeOnly.FromDateTime(timestamp);

            var result = await postgres.ElectionTokens.FindAsync(publicKey);

            if (result == null)
                return false;

            if (!result.IsActive)
                return false;

            if (result.SecretId.CompareTo(secretKey) != 0)
                return false;

            //if (!result.ElectionDate.Equals(date))
            //    return false;

            //if (!time.IsBetween(result.StartTime, result.EndTime))
            //    return false;

            return true;
        }

        // Returns true is validation is successful otherwise false
        private bool IsValidVoterGuid(Guid voterId) => voterId != Guid.Empty;

        private string CreateToken(VoterToken voter)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("voter", voter.VoterId),
                new Claim("riding", voter.RidingId.ToString()),
                new Claim("election", appSettings.Value.Vote.ElectionID.ToString()),
                new Claim(ClaimTypes.Role, "Voter")
            };

            var credentials = GetSigningCredentials();

            var token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddDays(14),
                //expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private string CreateToken(VoterToken voter, BallotToken vote)
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

            var credentials = GetSigningCredentials();

            var token = new JwtSecurityToken(
                issuer: tokenIssuer,
                claims: claims,
                expires: DateTime.Now.AddDays(14),
                //expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        #endregion
    }
}
