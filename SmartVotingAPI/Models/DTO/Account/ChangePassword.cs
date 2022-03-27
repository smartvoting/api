using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Account
{
    public class ChangePassword
    {
        [Required]
        public string Password { get; set; } = null!;

        [Required]
        public string? ConfirmPassword { get; set; }
    }
}
