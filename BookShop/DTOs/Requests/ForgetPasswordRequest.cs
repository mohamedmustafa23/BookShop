using System.ComponentModel.DataAnnotations;

namespace BookShop.DTOs.Request
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string UserNameOREmail { get; set; } = string.Empty;
    }
}
