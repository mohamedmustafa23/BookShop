using BookShop.DTOs.Requests;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Admin
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IRepository<Book> _bookRepository;
        public BooksController(IRepository<Book> bookRepository) 
        {
            _bookRepository = bookRepository;
        }

        [HttpPost("Get")]
        public async Task<IActionResult> Get(FilterBookRequest filterBookRequest, CancellationToken cancellationToken, [FromQuery] int page = 1)
        {
            var books = await _bookRepository.GetAsync(includes: [b => b.Category, b => b.Author], tracked: false, cancellationToken: cancellationToken);

            if (books == null)
                return NotFound();

            #region Filter Books
            FilterBookResponse filterBookResponse = new();

            if (filterBookRequest.title is not null)
            {
                books = books.Where(e => e.Title.Contains(filterBookRequest.title.Trim()));
                filterBookResponse.Title = filterBookRequest.title;
            }

            if (filterBookRequest.categoryId is not null)
            {
                books = books.Where(e => e.CategoryId == filterBookRequest.categoryId);
                filterBookResponse.CategoryId = filterBookRequest.categoryId;
            }

            if (filterBookRequest.authorId is not null)
            {
                books = books.Where(e => e.AuthorId == filterBookRequest.authorId);
                filterBookResponse.AuthorId = filterBookRequest.authorId;
            }
            #endregion

            #region Pagination
            PaginationResponse paginationResponse = new();

            // Pagination
            paginationResponse.TotalPages = Math.Ceiling(books.Count() / 8.0);
            paginationResponse.CurrentPage = page;
            books = books.Skip((page - 1) * 8).Take(8);
            #endregion

            return Ok(new
            {
                books = books.AsEnumerable(),
                FilterBookResponse = filterBookResponse,
                PaginationResponse = paginationResponse
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);

            if (book == null)
                return NotFound();

            return Ok(book);
        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(CreateBookRequest createBookRequest, CancellationToken cancellationToken)
        {
            Book book = createBookRequest.Adapt<Book>();

            if (createBookRequest.Img is not null && createBookRequest.Img.Length > 0)
            {
                // Save Img in wwwroot
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createBookRequest.Img.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\book_images", fileName);

                using (var stream = System.IO.File.Create(filePath))
                {
                    await createBookRequest.Img.CopyToAsync(stream);
                }

                
                book.Img = fileName;
            }

            
            await _bookRepository.AddAsync(book, cancellationToken);
            await _bookRepository.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOne), new { id = book.Id }, new
            {
                success_notifaction = "Add Book Successfully"
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, UpdateBookRequest updateBookRequest, CancellationToken cancellationToken)
        {
            var bookInDb = await _bookRepository.GetOneAsync(e => e.Id == id, cancellationToken: cancellationToken);
            if (bookInDb is null)
                return NotFound();

            if (updateBookRequest.NewImg is not null)
            {
                if (updateBookRequest.NewImg.Length > 0)
                {
                    // Save Img in wwwroot
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(updateBookRequest.NewImg.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\book_images", fileName);

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await updateBookRequest.NewImg.CopyToAsync(stream);
                    }

                    // Remove old Img in wwwroot
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\book_images", bookInDb.Img);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    // Save Img in db
                    bookInDb.Img = fileName;
                }
            }

            bookInDb.Title = updateBookRequest.Title;
            bookInDb.Description = updateBookRequest.Description;
            bookInDb.Price = updateBookRequest.Price;
            bookInDb.AuthorId = updateBookRequest.AuthorId;
            bookInDb.CategoryId = updateBookRequest.CategoryId;
            bookInDb.PublishYear = updateBookRequest.PublishYear;

            await _bookRepository.CommitAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var book = await _bookRepository.GetOneAsync(e => e.Id == id);

            if (book is null)
                return NotFound();

            // Remove old Img in wwwroot
            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\book_images", book.Img);
            if (System.IO.File.Exists(oldPath))
                System.IO.File.Delete(oldPath);

            _bookRepository.Delete(book);
            await _bookRepository.CommitAsync(cancellationToken);

            return NoContent();
        }
    }
}
