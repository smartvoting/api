namespace SmartVotingAPI.Data
{
    public class Caws
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class Capi
    {
        public string? Mapquest { get; set; }
        public string? HcaptchaSecret { get; set; }
        public string? VPN { get; set; }
    }

    public class Cses
    {
        public string EmailAddress { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class Cvote
    {
        public int ElectionID { get; set; }
        public string? SecretKey { get; set; }
        public string? LedgerID { get; set; }
    }

    public class AppSettings
    {
        public Cses? AmazonSES { get; set; }
        public Caws? AmazonAWS { get; set; }
        public Capi? API { get; set; }
        public string? Environment { get; set; }
        public string? TokenSignature { get; set; }
        public Cvote? Vote { get; set; }
    }
}
