namespace BookShop.DTOs.Request
{
    public record FilterBookRequest(
        string? title, int? categoryId, int? authorId
    );
}
