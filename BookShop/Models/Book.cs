using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class Book
    {
        public int Id { get; set; }
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
        public string Img { get; set; } = string.Empty;
        public Category Category { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public Author Author { get; set; }
        [Required]
        public int AuthorId { get; set; }

    }
}
