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

namespace SmartVotingAPI.Models.Postgres
{
    public partial class VoterSecurity
    {
        public Guid VoterId { get; set; }
        public string CardId { get; set; } = null!;
        public int CardPin { get; set; }
        public int? EmailPin { get; set; }
        public int Sin { get; set; }
        public int Tax10100 { get; set; }
        public int Tax12000 { get; set; }
        public int Tax12900 { get; set; }
        public int Tax14299 { get; set; }
        public int Tax15000 { get; set; }
        public int Tax23600 { get; set; }
        public int Tax24400 { get; set; }
        public int Tax26000 { get; set; }
        public int Tax31220 { get; set; }
        public int Tax58240 { get; set; }
    }
}
