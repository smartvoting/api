using System.ComponentModel.DataAnnotations;

namespace SmartVotingAPI.Models.DTO.Account
{
    public class SignIn
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string Password { get; set; } = null!;
    }
}
