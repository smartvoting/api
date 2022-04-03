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
