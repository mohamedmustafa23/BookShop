namespace BookShop.DTOs.Response
{
    public class FilterBookResponse
    {
        public string? Title { get; set; }
        public int? CategoryId { get; set; }
        public int? AuthorId { get; set; }

    }
}
