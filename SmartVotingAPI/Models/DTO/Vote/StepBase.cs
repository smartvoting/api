using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Vote
{
    public class StepBase
    {
        [Required]
        public string ApiKey { get; set; }

        [Required]
        public string RemoteIp { get; set; }
    }
}
