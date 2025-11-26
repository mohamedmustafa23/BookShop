using System.ComponentModel.DataAnnotations;

namespace BookShop.DTOs.Request
{
    public class NewPasswordRequest
    {
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;

    }
}
