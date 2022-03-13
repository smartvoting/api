using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;

namespace SmartVotingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IOptions<AppSettings> appSettings;
        protected readonly PostgresDbContext postgres;
        protected readonly IDynamoDBContext dynamo;

        public BaseController() { }
        public BaseController(PostgresDbContext context) => postgres = context;
        public BaseController(IDynamoDBContext client) => dynamo = client;
        public BaseController(IOptions<AppSettings> app) => appSettings = app;
        public BaseController(PostgresDbContext context, IDynamoDBContext client)
        {
            postgres = context;
            dynamo = client;
        }
        public BaseController(PostgresDbContext context, IDynamoDBContext client, IOptions<AppSettings> app)
        {
            postgres = context;
            dynamo = client;
            appSettings = app;
        }

        protected string GetMapquestCall(string postCode)
        {
            string baseUrl = "http://www.mapquestapi.com/geocoding/v1/address";
            //string apiKey = "CcqNKJY75Y0TotzTG1JZhLo3F8MrulEA";
            return String.Format("{0}?key={1}&location={2}", baseUrl, appSettings.Value.MapquestAPIKey, postCode);
        }

        protected string GetONBoundariesByCoordCall (string lat, string lng)
        {
            string baseUrl = "https://represent.opennorth.ca/boundaries/?contains=";
            return String.Format("{0}{1},{2}", baseUrl, lat, lng);
        }

        protected string GetONBoundariesByRidingIdCall(int ridingId, bool shape = false)
        {
            string baseUrl = "https://represent.opennorth.ca/boundaries/federal-electoral-districts/";

            if (shape)
                return String.Format("{0}{1}/shape", baseUrl, ridingId.ToString(), "shape");

            return String.Format("{0}{1}", baseUrl, ridingId.ToString());
        }
    }
}
