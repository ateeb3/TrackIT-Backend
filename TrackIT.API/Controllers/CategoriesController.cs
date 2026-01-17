using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackIT.Core.DTOs;
using TrackIT.Core.Entities;
using TrackIT.Core.Interfaces;

namespace TrackIT.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController(IUnitOfWork unitOfWork) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(id);

            if (category == null) return NotFound($"Category with ID {id} not found.");

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                // If you want accurate counts, you might need a custom repo method later.
                // For now, this prevents null errors:
                AssetCount = category.Assets?.Count ?? 0
            });
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _unitOfWork.Repository<Category>().GetAllAsync();

            var dtos = categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                // Note: Standard Repo doesn't include Assets by default, 
                // so AssetCount might be 0 unless we add Include() logic later.
                AssetCount = c.Assets?.Count ?? 0
            });

            return Ok(dtos);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto input)
        {
            var category = new Category { Name = input.Name };

            await _unitOfWork.Repository<Category>().AddAsync(category);
            await _unitOfWork.CompleteAsync();

            return Ok(new CategoryDto { Id = category.Id, Name = category.Name });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // Restrict updates to Admins
        public async Task<IActionResult> UpdateCategory(int id, CreateCategoryDto input)
        {
            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(id);

            if (category == null) return NotFound($"Category with ID {id} not found.");

            // Update fields
            category.Name = input.Name;

            // Save changes
            await repo.UpdateAsync(category);
            await _unitOfWork.CompleteAsync();

            return NoContent(); // Standard 204 response for updates
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var repo = _unitOfWork.Repository<Category>();
            var category = await repo.GetByIdAsync(id);

            if (category == null) return NotFound();

            // Note: The Global Exception Handler or DB Constraint will catch this 
            // if we try to delete a category that has assets (Restrict Delete).
            await repo.DeleteAsync(category);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
    }
}