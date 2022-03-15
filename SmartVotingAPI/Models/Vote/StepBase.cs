using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.Vote
{
    public class StepBase
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RemoteIp { get; set; }
    }
}
