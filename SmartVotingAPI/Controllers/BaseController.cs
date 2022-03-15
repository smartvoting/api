using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using System.Text.Json.Nodes;

namespace SmartVotingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected readonly IOptions<AppSettings> appSettings;
        protected readonly PostgresDbContext postgres;
        protected readonly IDynamoDBContext dynamo;
        private static HttpClient client = new HttpClient();

        public BaseController() { }
        public BaseController(PostgresDbContext dbContext) => postgres = dbContext;
        public BaseController(IDynamoDBContext dynamoContext) => dynamo = dynamoContext;
        public BaseController(IOptions<AppSettings> app) => appSettings = app;
        public BaseController(PostgresDbContext dbContext, IDynamoDBContext dynamoContext)
        {
            postgres = dbContext;
            dynamo = dynamoContext;
        }
        public BaseController(PostgresDbContext dbContext, IDynamoDBContext dynamoContext, IOptions<AppSettings> app)
        {
            postgres = dbContext;
            dynamo = dynamoContext;
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

        protected async Task<bool> VerifyHcaptcha(string token, string remoteIp)
        {
            string baseUrl = "https://hcaptcha.com/siteverify";
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("secret", appSettings.Value.HcaptchaSecret),
                new KeyValuePair<string, string>("response", token),
                new KeyValuePair<string, string>("remoteip", remoteIp)
            };
            HttpResponseMessage response = await client.PostAsync(baseUrl, new FormUrlEncodedContent(postData));

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseBody);
            }
            return false;
        }
    }
}
