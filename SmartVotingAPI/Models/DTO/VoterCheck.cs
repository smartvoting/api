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

using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO
{
    public class VoterCheck
    {
        [Required]
        public bool IsCitizen { get; set; }

        [Required]
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [Range(1, 3)]
        public int Gender { get; set; }

        [Required]
        public int StreetNumber { get; set; }

        [Required]
        public string StreetName { get; set; } = null!;

        public string? UnitNumber { get; set; }

        [Required]
        public string City { get; set; } = null!;

        [Required]
        [Range(1, 13)]
        public int Province { get; set; }

        [Required]
        [StringLength(7)]
        public string PostCode { get; set; } = null!;
    }
}
