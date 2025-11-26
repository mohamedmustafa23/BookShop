using System.ComponentModel.DataAnnotations;

namespace BookShop.DTOs.Requests
{
    public class UpdateBookRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public DateTime PublishYear { get; set; }

        public IFormFile? NewImg { get; set; } = default;

        public int CategoryId { get; set; }
        public int AuthorId { get; set; }
    }
}
