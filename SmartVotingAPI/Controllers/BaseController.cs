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
using Microsoft.IdentityModel.Tokens;
using SmartVotingAPI.Data;
using SmartVotingAPI.Models.DTO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SmartVotingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected const int demoElectionId = 45;
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

        protected async Task<Coordinates?> GetRidingCentroid(int ridingId)
        {
            string url = $"https://represent.opennorth.ca/boundaries/federal-electoral-districts/{ridingId}/centroid";

            HttpResponseMessage openNorthCall = await client.GetAsync(url);

            if (openNorthCall.IsSuccessStatusCode)
            {
                var response = await openNorthCall.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(response);
                JsonElement element = json.RootElement;
                Coordinates coordinates = new()
                {
                    Latitude = element.GetProperty("coordinates")[1].GetDecimal(),
                    Longitude = element.GetProperty("coordinates")[0].GetDecimal(),
                    Type = element.GetProperty("type").ToString()
                };
                return coordinates;
            }

            return null;
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
                //new KeyValuePair<string, string>("secret", appSettings.Value.API.HcaptchaSecret),
                //new KeyValuePair<string, string>("response", token),
                new KeyValuePair<string, string>("secret", "0x0000000000000000000000000000000000000000"),
                new KeyValuePair<string, string>("response", "10000000-aaaa-bbbb-cccc-000000000001"),
                new KeyValuePair<string, string>("remoteip", remoteIp)
            };

            HttpResponseMessage response = await client.PostAsync(baseUrl, new FormUrlEncodedContent(postData));

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                JsonDocument json = JsonDocument.Parse(responseBody);
                JsonElement element = json.RootElement;
                bool status = bool.Parse(element.GetProperty("success").ToString());
                Console.WriteLine(status);
                return status;
            }

            return false;
        }

        protected async Task<bool> SendEmailSES(string recipient, string destination, string subject, string body)
        {
            string sender = appSettings.Value.AmazonSES.EmailAddress;
            string name = appSettings.Value.AmazonSES.DisplayName;
            string username = appSettings.Value.AmazonSES.Username;
            string password = appSettings.Value.AmazonSES.Password;
            string host = appSettings.Value.AmazonSES.Host;
            int port = appSettings.Value.AmazonSES.Port;

            MailMessage message = new MailMessage();
            message.IsBodyHtml = true;
            message.From = new MailAddress(sender, name);
            message.To.Add(new MailAddress(destination, recipient));
            message.Subject = subject;
            message.Body = body;
            
            using (var client = new SmtpClient(host, port))
            {
                client.Credentials = new NetworkCredential(username, password);
                client.EnableSsl = true;
                try
                {
                    Console.WriteLine("Attempting to send email...");
                    client.Send(message);
                    Console.WriteLine("Email sent!");
                } catch (Exception e)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + e.Message);
                    return false;
                }
            }

            return true;
            //var emailClient = new AmazonSimpleEmailServiceClient(new BasicAWSCredentials(username, password), RegionEndpoint.USEast1);

            //var request = new SendTemplatedEmailRequest
            //{
            //    Source = sender,
            //    Destination = new Destination { ToAddresses = new List<string> { recipient } },
            //    Template = template,
            //    TemplateData = data
            //};

            //try
            //{
            //    Console.WriteLine("Sending email using Amazon SES...");
            //    var result = await emailClient.SendTemplatedEmailAsync(request);
            //    Console.WriteLine("The email was sent successfully.");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("The email was not sent.");
            //    Console.WriteLine("Error message: " + ex.Message);
            //}

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
        }

        #region Security Tokens
        protected SigningCredentials GetSigningCredentials()
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettings.Value.TokenSignature));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            return credentials;
        }
        #endregion

        #region Voter Datasets
        protected int GetRidingId(int index)
        {
            int[] list = { 10001, 10002, 10003, 10004, 10005, 10006, 10007, 11001, 11002, 11003, 11004, 12001, 12002, 12003, 12004, 12005, 12006, 12007, 12008, 12009, 12010, 12011, 13001, 13002, 13003, 13004, 13005, 13006, 13007, 13008, 13009, 13010, 24001, 24002, 24003, 24004, 24005, 24006, 24007, 24008, 24009, 24010, 24011, 24012, 24013, 24014, 24015, 24016, 24017, 24018, 24019, 24020, 24021, 24022, 24023, 24024, 24025, 24026, 24027, 24028, 24029, 24030, 24031, 24032, 24033, 24034, 24035, 24036, 24037, 24038, 24039, 24040, 24041, 24042, 24043, 24044, 24045, 24046, 24047, 24048, 24049, 24050, 24051, 24052, 24053, 24054, 24055, 24056, 24057, 24058, 24059, 24060, 24061, 24062, 24063, 24064, 24065, 24066, 24067, 24068, 24069, 24070, 24071, 24072, 24073, 24074, 24075, 24076, 24077, 24078, 35001, 35002, 35003, 35004, 35005, 35006, 35007, 35008, 35009, 35010, 35011, 35012, 35013, 35014, 35015, 35016, 35017, 35018, 35019, 35020, 35021, 35022, 35023, 35024, 35025, 35026, 35027, 35028, 35029, 35030, 35031, 35032, 35033, 35034, 35035, 35036, 35037, 35038, 35039, 35040, 35041, 35042, 35043, 35044, 35045, 35046, 35047, 35048, 35049, 35050, 35051, 35052, 35053, 35054, 35055, 35056, 35057, 35058, 35059, 35060, 35061, 35062, 35063, 35064, 35065, 35066, 35067, 35068, 35069, 35070, 35071, 35072, 35073, 35074, 35075, 35076, 35077, 35078, 35079, 35080, 35081, 35082, 35083, 35084, 35085, 35086, 35087, 35088, 35089, 35090, 35091, 35092, 35093, 35094, 35095, 35096, 35097, 35098, 35099, 35100, 35101, 35102, 35103, 35104, 35105, 35106, 35107, 35108, 35109, 35110, 35111, 35112, 35113, 35114, 35115, 35116, 35117, 35118, 35119, 35120, 35121, 46001, 46002, 46003, 46004, 46005, 46006, 46007, 46008, 46009, 46010, 46011, 46012, 46013, 46014, 47001, 47002, 47003, 47004, 47005, 47006, 47007, 47008, 47009, 47010, 47011, 47012, 47013, 47014, 48001, 48002, 48003, 48004, 48005, 48006, 48007, 48008, 48009, 48010, 48011, 48012, 48013, 48014, 48015, 48016, 48017, 48018, 48019, 48020, 48021, 48022, 48023, 48024, 48025, 48026, 48027, 48028, 48029, 48030, 48031, 48032, 48033, 48034, 59001, 59002, 59003, 59004, 59005, 59006, 59007, 59008, 59009, 59010, 59011, 59012, 59013, 59014, 59015, 59016, 59017, 59018, 59019, 59020, 59021, 59022, 59023, 59024, 59025, 59026, 59027, 59028, 59029, 59030, 59031, 59032, 59033, 59034, 59035, 59036, 59037, 59038, 59039, 59040, 59041, 59042, 60001, 61001, 62001 };
            return list[index];
        }

        protected string GetFemaleName(int index)
        {
            string[] list = { "Abbie", "Abigail", "Ada", "Aila", "Alba", "Alice", "Amber", "Amelia", "Anna", "Arabella", "Aria", "Aurora", "Autumn", "Ava", "Ayda", "Ayla", "Bella", "Bonnie", "Callie", "Charlotte", "Chloe", "Cora", "Daisy", "Eden", "Eilidh", "Elizabeth", "Ella", "Ellie", "Elsie", "Emilia", "Emily", "Emma", "Erin", "Esme", "Eva", "Eve", "Evie", "Faith", "Florence", "Freya", "Georgia", "Grace", "Gracie", "Hallie", "Hannah", "Harley", "Harper", "Hollie", "Holly", "Hope", "Imogen", "Indie", "Iona", "Isabella", "Isla", "Ivy", "Jessica", "Katie", "Layla", "Lexi", "Lilly", "Lily", "Lottie", "Lucie", "Lucy", "Luna", "Maeve", "Maisie", "Matilda", "Maya", "Mia", "Mila", "Millie", "Mirren", "Mollie", "Molly", "Myla", "Niamh", "Nina", "Olivia", "Orla", "Phoebe", "Piper", "Poppy", "Quinn", "Rebecca", "Remi", "Robyn", "Rosa", "Rose", "Rosie", "Ruby", "Scarlett", "Sienna", "Skye", "Sofia", "Sophia", "Sophie", "Summer", "Thea", "Violet", "Willow", "Zara", "Zoe" };
            return list[index];
        }

        protected string GetMaleName(int index)
        {
            string[] list = { "Aaron", "Adam", "Alexander", "Alfie", "Andrew", "Angus", "Archie", "Arlo", "Arran", "Arthur", "Ben", "Benjamin", "Blair", "Blake", "Brodie", "Brody", "Caleb", "Callan", "Callum", "Cameron", "Carter", "Charlie", "Cody", "Cole", "Connor", "Cooper", "Daniel", "David", "Dylan", "Elijah", "Elliot", "Ellis", "Ethan", "Finlay", "Finley", "Finn", "Frankie", "Fraser", "Freddie", "George", "Grayson", "Hamish", "Harris", "Harrison", "Harry", "Hunter", "Innes", "Isaac", "Jack", "Jackson", "Jacob", "James", "Jamie", "Jaxon", "Joey", "John", "Joseph", "Joshua", "Jude", "Kai", "Leo", "Leon", "Lewis", "Liam", "Logan", "Louie", "Louis", "Luca", "Lucas", "Luke", "Mason", "Matthew", "Max", "Michael", "Muhammad", "Murray", "Myles", "Nathan", "Noah", "Oliver", "Ollie", "Oscar", "Owen", "Reuben", "Riley", "Robbie", "Robert", "Roman", "Rory", "Ruairidh", "Ruaridh", "Ryan", "Samuel", "Sonny", "Teddy", "Theo", "Theodore", "Thomas", "Tommy", "William" };
            return list[index];
        }

        protected string GetLastName(int index)
        {
            string[] list = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "Garcia", "Rodriguez", "Wilson", "Martinez", "Anderson", "Taylor", "Thomas", "Hernandez", "Moore", "Martin", "Jackson", "Thompson", "White", "Lopez", "Lee", "Gonzalez", "Harris", "Clark", "Lewis", "Robinson", "Walker", "Perez", "Hall", "Young", "Allen", "Sanchez", "Wright", "King", "Scott", "Green", "Baker", "Adams", "Nelson", "Hill", "Ramirez", "Campbell", "Mitchell", "Roberts", "Carter", "Phillips", "Evans", "Turner", "Torres", "Parker", "Collins", "Edwards", "Stewart", "Flores", "Morris", "Nguyen", "Murphy", "Rivera", "Cook", "Rogers", "Morgan", "Peterson", "Cooper", "Reed", "Bailey", "Bell", "Gomez", "Kelly", "Howard", "Ward", "Cox", "Diaz", "Richardson", "Wood", "Watson", "Brooks", "Bennett", "Gray", "James", "Reyes", "Cruz", "Hughes", "Price", "Myers", "Long", "Foster", "Sanders", "Ross", "Morales", "Powell", "Sullivan", "Russell", "Ortiz", "Jenkins", "Gutierrez", "Perry", "Butler", "Barnes", "Fisher", "Henderson", "Coleman", "Simmons", "Patterson", "Jordan", "Reynolds", "Hamilton", "Graham", "Kim", "Gonzales", "Alexander", "Ramos", "Wallace", "Griffin", "West", "Cole", "Hayes", "Chavez", "Gibson", "Bryant", "Ellis", "Stevens", "Murray", "Ford", "Marshall", "Owens", "McDonald", "Harrison", "Ruiz", "Kennedy", "Wells", "Alvarez", "Mendoza", "Castillo", "Olson", "Webb", "Washington", "Tucker", "Freeman", "Burns", "Henry", "Vasquez", "Simpson", "Snyder", "Crawford", "Jimenez", "Porter", "Mason", "Shaw", "Gordon", "Wagner" };
            return list[index];
        }

        protected DateOnly GetBirthDate()
        {
            Random random = new Random();
            // Range from January 1 1920 to January 1 2004
            int startRange = 700899;
            int endRange = 731580;
            int newDate = random.Next(startRange, endRange + 1);
            return DateOnly.FromDayNumber(newDate);
        }

        protected string GetStreetName(int index)
        {
            string[] list = { "High Street", "Station Road", "Main Street", "Park Road", "Church Road", "Church Street", "London Road", "Victoria Road", "Green Lane", "Manor Road", "Church Lane", "Park Avenue", "The Avenue", "The Crescent", "Queens Road", "New Road", "Grange Road", "Kings Road", "Kingsway", "Windsor Road", "Highfield Road", "Mill Lane", "Alexander Road", "York Road", "St. John Road", "Main Road", "Broadway", "King Street", "The Green", "Springfield Road", "George Street", "Park Lane", "Victoria Street", "Albert Road", "Queensway", "New Street", "Queen Street", "West Street", "North Street", "Manchester Road", "The Grove", "Richmond Road", "Grove Road", "South Street", "School Lane", "The Drive", "North Road", "Stanley Road", "Chester Road", "Mill Road", "Tupac Lane", "Frying Pan Road", "This Street", "That Street", "The Other Street", "Roast Meat Hill Road", "100 Year Party Court", "Zzyzx Road", "Chicken Dinner Road ", "Error Place", "Bad Route Road", "Duh Drive", "Puddin Ridge Road", "Anyhow Lane", "Evergreen Terrace", "Anyhow Lane", "Linger Longer Road", "Chicken Gristle Road", "Pillow Talk Court", "Squeezepenny Lane", "Farfrompoopen Road", "This Aint It Road", "Old Trash Pile Road", "Tater Peeler Road", "Yellowsnow Road", "Poopdeck Street", "Bucket of Blood Street", "Psycho Path", "Shades of Death Road", "Fresh Holes Road", "Kitchen-Dick Road", "English Muffin Way", "One Fun Place", "Little Smokies Lane", "Chow Mein Lane", "Alcohol Mary Road", "Stub Toe Lane", "Hanky Panky Street", "Booger Branch Road", "Rascally Rabbit Road", "Dutch Oven Avenue", "Chicken Dinner Road", "Weiner Cutoff Road", "Queen Bush Road", "Buttertubs Drive", "Ragged Ass Road", "Road to Nowhere", "The Tragically Hip Way", "Avenue Road", "Ha Ha Creek Road" };
            return list[index];
        }

        protected string GetCityName(int index)
        {
            string[] list = { "Toronto", "Montreal", "Vancouver", "Calgary", "Edmonton", "Ottawa", "Mississauga", "Winnipeg", "Quebec City", "Hamilton", "Brampton", "Surrey", "Kitchener", "Laval", "Halifax", "London", "Victoria", "Markham", "St. Catharines", "Niagara Falls", "Vaughan", "Gatineau", "Windsor", "Saskatoon", "Longueuil", "Burnaby", "Regina", "Richmond", "Richmond Hill", "Oakville", "Burlington", "Barrie", "Oshawa", "Sherbrooke", "Saguenay", "Levis", "Kelowna", "Abbotsford", "Coquitlam", "Trois-Rivieres", "Guelph", "Cambridge", "Whitby", "Ajax", "Langley", "Saanich", "Terrebonne", "Milton", "St. John's", "Moncton", "Thunder Bay", "Dieppe", "Waterloo", "Delta", "Chatham", "Red Deer", "Kamloops", "Brantford", "Cape Breton", "Lethbridge", "Saint-Jean-sur-Richelieu", "Clarington", "Pickering", "Nanaimo", "Sudbury", "North Vancouver", "Brossard", "Repentigny", "Newmarket", "Chilliwack", "White Rock", "Maple Ridge", "Peterborough", "Kawartha Lakes", "Prince George", "Sault Ste. Marie", "Sarnia", "Wood Buffalo", "New Westminster", "Chateauguay", "Saint-Jerome", "Drummondville", "Saint John", "Caledon", "St. Albert", "Granby", "Medicine Hat", "Grande Prairie", "St. Thomas", "Airdrie", "Halton Hills", "Saint-Hyacinthe", "Lac-Brome", "Port Coquitlam", "Fredericton", "Blainville", "Aurora", "Welland", "North Bay", "Beloeil", "Belleville", "Mirabel", "Shawinigan", "Dollard-des-Ormeaux", "Brandon", "Rimouski", "Cornwall", "Stouffville", "Georgina", "Victoriaville", "Vernon", "Duncan", "Saint-Eustache", "Quinte West", "Charlottetown", "Mascouche", "West Vancouver", "Salaberry-de-Valleyfield", "Rouyn-Noranda", "Timmins", "Sorel-Tracy", "New Tecumseth", "Woodstock", "Boucherville", "Mission", "Vaudreuil-Dorion", "Brant", "Lakeshore", "Innisfil", "Prince Albert", "Langford Station", "Bradford West Gwillimbury", "Campbell River", "Spruce Grove", "Moose Jaw", "Penticton", "Port Moody" };
            return list[index];
        }

        protected string GetUnitNumber()
        {
            Random random = new Random();
            int length = random.Next(0, 6);
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                int x = random.Next(0, validChars.Length);
                chars[i] = validChars[x];
            }
            return new string(chars);
        }

        protected string GetPostCode()
        {
            Random random = new Random();
            string postCode = "";
            postCode += GetRandomChar().ToString().ToUpper();
            postCode += random.Next(0, 10).ToString();
            postCode += GetRandomChar().ToString().ToUpper();
            postCode += " ";
            postCode += random.Next(0, 10).ToString();
            postCode += GetRandomChar().ToString().ToUpper();
            postCode += random.Next(0, 10).ToString();
            return postCode;
        }

        protected char GetRandomChar()
        {
            char[] letters = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
            Random random = new Random();
            int index = random.Next(0, 26);
            return letters[index];
        }

        protected string GetEmailAddress(int index)
        {
            string[] list =
            {
                "abatvrmdz1lsztzv@skdprojects.net",
                "fa8xc3apih2eamj1@skdprojects.net",
                "jgn3ccgkexbjzdgn@skdprojects.net",
                "jmtppd8ajkvwwtvb@skdprojects.net",
                "xzerusp4e8e8qxmg@skdprojects.net"
            };
            return list[index];
        }

        protected string GetPhoneNumber()
        {
            Random random = new Random();
            string phoneNumber = "";
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += "-";
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += "-";
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            phoneNumber += random.Next(0, 10).ToString();
            return phoneNumber;
        }

        protected string GetCardId()
        {
            Random random = new Random();
            int length = 12;
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                int x = random.Next(0, validChars.Length);
                chars[i] = validChars[x];
            }
            return new string(chars);
        }
        #endregion
    }
}
