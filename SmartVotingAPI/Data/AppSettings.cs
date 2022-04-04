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
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class Capi
    {
        public string Mapquest { get; set; } = null!;
        public string HcaptchaSecret { get; set; } = null!;
        public string VPN { get; set; } = null!;
    }

    public class Cses
    {
        public string EmailAddress { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
    }

    public class Cvote
    {
        public int ElectionID { get; set; }
        public string SecretKey { get; set; } = null!;
        public string LedgerID { get; set; } = null!;
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
