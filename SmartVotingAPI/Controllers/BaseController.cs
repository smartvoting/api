using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartVotingAPI.Data;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

namespace SmartVotingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected const string tokenIssuer = "api.smartvoting.cc";
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
        public BaseController(PostgresDbContext dbContext, IOptions<AppSettings> app)
        {
            postgres = dbContext;
            appSettings = app;
        }
        public BaseController(PostgresDbContext dbContext, IDynamoDBContext dynamoContext, IOptions<AppSettings> app)
        {
            postgres = dbContext;
            dynamo = dynamoContext;
            appSettings = app;
        }

        protected JsonObject NewReturnMessage(string message)
        {
            JsonObject json = new JsonObject
            {
                ["message"] = message
            };

            return json;
        }

        protected string GetMapquestCall(string postCode)
        {
            string baseUrl = "http://www.mapquestapi.com/geocoding/v1/address";
            //string apiKey = "CcqNKJY75Y0TotzTG1JZhLo3F8MrulEA";
            return String.Format("{0}?key={1}&location={2}", baseUrl, appSettings.Value.API.Mapquest, postCode);
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

        protected string GetTextHash(string text)
        {
            byte[] temp = null;

            using (HashAlgorithm algorithm = SHA256.Create())
                temp = algorithm.ComputeHash(Encoding.UTF8.GetBytes(text));

            StringBuilder sb = new();
            foreach (byte b in temp)
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        protected async Task<bool> VerifyHcaptcha(string token, string remoteIp)
        {
            string baseUrl = "https://hcaptcha.com/siteverify";
            List<KeyValuePair<string, string>> postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("secret", appSettings.Value.API.HcaptchaSecret),
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

        protected async Task<bool> SendEmailSES(string recipient, string template, string data)
        {
            string sender = "noreply@mail.smartvoting.cc";
            string username = appSettings.Value.AmazonAWS.Username;
            string password = appSettings.Value.AmazonAWS.Password;
            var emailClient = new AmazonSimpleEmailServiceClient(new BasicAWSCredentials(username, password), RegionEndpoint.USEast1);

            var request = new SendTemplatedEmailRequest
            {
                Source = sender,
                Destination = new Destination { ToAddresses = new List<string> { recipient } },
                Template = template,
                TemplateData = data
            };

            try
            {
                Console.WriteLine("Sending email using Amazon SES...");
                var result = await emailClient.SendTemplatedEmailAsync(request);
                Console.WriteLine("The email was sent successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("The email was not sent.");
                Console.WriteLine("Error message: " + ex.Message);
            }

            //using (var client = new AmazonSimpleEmailServiceClient(RegionEndpoint.USEast1))
            //{
            //    var request = new SendEmailRequest
            //    {
            //        Source = source,
            //        Destination = new Destination
            //        {
            //            ToAddresses = new List<string> { recipient }
            //        },
            //        Message = new Message
            //        {
            //            Subject = new Content(subject),
            //            Body = new Body
            //            {
            //                //Html = new Content
            //                //{
            //                //    Charset = "UTF-8",
            //                //    Data = message
            //                //},
            //                Text = new Content
            //                {
            //                    Charset = "UTF-8",
            //                    Data = message
            //                }
            //            }
            //        }
            //    };

            //    try
            //    {
            //        Console.WriteLine("Sending email using Amazon SES...");
            //        var response = client.SendEmailAsync(request);
            //        Console.WriteLine("The email was sent successfully.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("The email was not sent.");
            //        Console.WriteLine("Error message: " + ex.Message);
            //    }
            //}

            return true;
        }
    }
}
