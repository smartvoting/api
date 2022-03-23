namespace SmartVotingAPI.Data
{
    public class Cses
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Capi
    {
        public string Mapquest { get; set; }
        public string HcaptchaSecret { get; set; }
        public string VPN { get; set; }
    }

    public class Cvote
    {
        public string VoteKeyID { get; set; }
        public string TokenSignature { get; set; }
    }

    public class AppSettings
    {
        public Capi API { get; set; }
        public string Environment { get; set; }
        public Cvote Vote { get; set; }
        public Cses AmazonSES { get; set; }
    }
}
