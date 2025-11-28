using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.Areas.Admin
{
    [Area("Admin")]
    [Route("api/[Area]/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly IRepository<Category> _categoriesRepository;

        public CategoriesController(IRepository<Category> categoriesRepository)
        {
            _categoriesRepository = categoriesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var categories = await _categoriesRepository.GetAsync(cancellationToken: cancellationToken, tracked: false);

            if (categories == null)
                return NotFound();

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(int id, CancellationToken cancellationToken)
        {
            var category = await _categoriesRepository.GetOneAsync(c => c.Id == id , cancellationToken: cancellationToken);

            if(category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost("")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Create(Category category, CancellationToken cancellationToken)
        {
            await _categoriesRepository.AddAsync(category, cancellationToken);
            await _categoriesRepository.CommitAsync(cancellationToken);

            return CreatedAtAction(nameof(GetOne), new { id = category.Id }, new
            {
                success_notification = "Category Created Successfully"
            });
        }
        [HttpPut("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Edit(int id, Category category, CancellationToken cancellationToken)
        {
            var categoryInDb = await _categoriesRepository.GetOneAsync(c => c.Id==id , cancellationToken: cancellationToken);

            if (categoryInDb == null) 
                return NotFound();

            categoryInDb.Name = category.Name;

            await _categoriesRepository.CommitAsync(cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{SD.SUPER_ADMIN_ROLE},{SD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var category = await _categoriesRepository.GetOneAsync(c => c.Id==id, cancellationToken: cancellationToken);

            if (category == null) 
                return NotFound();

            _categoriesRepository.Delete(category);
            await _categoriesRepository.CommitAsync(cancellationToken);

            return NoContent();
        }

    }
}
