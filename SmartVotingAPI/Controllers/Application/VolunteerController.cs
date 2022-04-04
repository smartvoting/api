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

using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using SmartVotingAPI.Data;
using System.Text.Json.Nodes;

namespace SmartVotingAPI.Controllers.Application
{
    [ApiVersion("1")]
    [Route("v1/Volunteer")]
    [ApiController]
    public class VolunteerController : BaseController
    {
        public VolunteerController(PostgresDbContext context, IDynamoDBContext client) : base(context, client) { }

        [HttpPost]
        [Route("Apply")]
        public async Task<IActionResult> PostApply(Models.DTO.VolunteerApplication application)
        {
            if (application.PartyId <= 0 || application.RidingId <= 0 || string.IsNullOrEmpty(application.FirstName) || string.IsNullOrEmpty(application.LastName) || string.IsNullOrEmpty(application.PhoneNumber) || string.IsNullOrEmpty(application.EmailAddress))
                return BadRequest();

            DateTime timestamp = DateTime.Now;
            Models.Postgres.VolunteerApplication volunteerApplication = new()
            {
                PartyId = application.PartyId,
                RidingId = application.RidingId,
                FirstName = application.FirstName,
                LastName = application.LastName,
                PhoneNumber = application.PhoneNumber,
                EmailAddress = application.EmailAddress,
                LegalResident = application.LegalResident,
                PastVolunteer = application.PastVolunteer,
                PartyMember = application.PartyMember,
                IsApproved = false,
                Submitted = timestamp,
                Updated = timestamp
            };

            postgres.VolunteerApplications.Add(volunteerApplication);
            var result = await postgres.SaveChangesAsync();

            if (result != 1)
                return BadRequest(new { message = "Failed to save entry in database." });

            int id = volunteerApplication.ApplicationId;

            var json = new JsonObject
            {
                ["application_id"] = id
            };

            return Ok(json);
        }
    }
}
