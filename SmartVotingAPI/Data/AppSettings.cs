﻿namespace SmartVotingAPI.Data
{
    public class Caws
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
        public int ElectionID { get; set; }
        public string VoteKeyID { get; set; }
        public string LedgerID { get; set; }
        public string TokenSignature { get; set; }
    }

    public class AppSettings
    {
        public Capi API { get; set; }
        public string Environment { get; set; }
        public Cvote Vote { get; set; }
        public Caws AmazonAWS { get; set; }
    }
}