using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Mvc;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.Postgres;
using SmartVotingAPI.Models.DTO;
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
            if (application.PartyId <= 0 || application.RidingId <= 0 || String.IsNullOrEmpty(application.FirstName) || String.IsNullOrEmpty(application.LastName) || String.IsNullOrEmpty(application.PhoneNumber) || String.IsNullOrEmpty(application.EmailAddress))
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
