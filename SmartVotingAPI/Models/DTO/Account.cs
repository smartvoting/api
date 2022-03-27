using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO
{
    public class Account
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Password { get; set; } = null!;

        public string? ConfirmPassword { get; set; }

        public string? RoleGroup { get; set; }
        public string? RoleType { get; set; }
    }
}
