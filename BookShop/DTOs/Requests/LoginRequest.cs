using System.ComponentModel.DataAnnotations;

namespace BookShop.DTOs.Request
{
    public class LoginRequest
    {

        [Required]
        public string UserNameOREmail { get; set; } = string.Empty;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
