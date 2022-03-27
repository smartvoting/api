using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Account
{
    public class UserToken
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string RoleGroup { get; set; } = null!;

        [Required]
        public string RoleType { get; set; } = null!;
    }
}
