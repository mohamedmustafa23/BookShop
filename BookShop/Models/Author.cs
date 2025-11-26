using System.ComponentModel.DataAnnotations;

namespace BookShop.Models
{
    public class Author
    {
        public int Id { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
     
    }
}
