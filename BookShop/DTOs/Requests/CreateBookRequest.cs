using System.ComponentModel.DataAnnotations;

namespace BookShop.DTOs.Requests
{
    public class CreateBookRequest
    {
        [Required]
        [MinLength(3)]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public DateTime PublishYear { get; set; }
        [Required]
        public IFormFile Img { get; set; } = default;
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public int AuthorId { get; set; }
    }
}
