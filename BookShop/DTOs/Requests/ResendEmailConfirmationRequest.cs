using System.ComponentModel.DataAnnotations;

namespace BookShop.DTOs.Request
{
    public class ResendEmailConfirmationRequest
    {
        [Required]
        public string UserNameOREmail { get; set; } = string.Empty;
    }
}
