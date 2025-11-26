using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Admin
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IRepository<Author> _authorRepoitory;

        public AuthorsController(IRepository<Author> authorRepoitory)
        {
            _authorRepoitory = authorRepoitory;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var authors = await _authorRepoitory.GetAsync(cancellationToken: cancellationToken, tracked: false);

            if (authors == null)
                return NotFound();

            return Ok(authors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var author = await _authorRepoitory.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);

            if (author == null)
                return NotFound();

            return Ok(author);
        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Author author, CancellationToken cancellationToken)
        {
            await _authorRepoitory.AddAsync(author, cancellationToken);
            await _authorRepoitory.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOne), new { id = author.Id }, new
            {
                success_notification = "Author Created Successfully"
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, Author author, CancellationToken cancellationToken)
        {
            var authorInDb = await _authorRepoitory.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);

            if (authorInDb == null)
                return NotFound();

            authorInDb.FirstName = author.FirstName;
            authorInDb.LastName = author.LastName;


            await _authorRepoitory.CommitAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var author = await _authorRepoitory.GetOneAsync(c => c.Id == id, cancellationToken: cancellationToken);

            if (author == null)
                return NotFound();

            _authorRepoitory.Delete(author);
            await _authorRepoitory.CommitAsync(cancellationToken);

            return NoContent();
        }
    }
}
