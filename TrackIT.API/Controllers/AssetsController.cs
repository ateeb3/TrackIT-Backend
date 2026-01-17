using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrackIT.Core.DTOs;
using TrackIT.Core.Entities;
using TrackIT.Core.Enums;
using TrackIT.Core.Interfaces;

namespace TrackIT.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AssetsController(IUnitOfWork unitOfWork, ILogger<AssetsController> logger) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<AssetsController> _logger = logger;

        // --- NEW OPTIMIZED ENDPOINT ---
        // GET: api/assets/stats
        [HttpGet("stats")]
        public async Task<ActionResult<object>> GetAssetStats()
        {
            // Fetch all assets (In a real scenario with EF Core, you'd use .CountAsync() queries directly)
            // Since we are using a Generic Repository, we get the list first.
            // optimization: For massive DBs, add a Count method to your IRepository.
            var allAssets = await _unitOfWork.Repository<Asset>().GetAsync();

            var stats = new
            {
                Total = allAssets.Count(),
                Assigned = allAssets.Count(a => a.Status == AssetStatus.Assigned),
                Available = allAssets.Count(a => a.Status == AssetStatus.Available),
                Broken = allAssets.Count(a => a.Status == AssetStatus.Broken),
                InRepair = allAssets.Count(a => a.Status == AssetStatus.InRepair),
                Retired = allAssets.Count(a => a.Status == AssetStatus.Retired)
            };

            return Ok(stats);
        }
        // -----------------------------

        // GET: api/assets
        // GET: api/assets?search=hp&page=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<object>> GetAssets(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var repo = _unitOfWork.Repository<Asset>();

            // 1. Build Query
            var allAssets = await repo.GetAsync(null, a => a.Category!);

            // 2. Filter (Search)
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                allAssets = allAssets.Where(a =>
                    a.Name.ToLower().Contains(search) ||
                    a.SerialNumber.ToLower().Contains(search)
                ).ToList();
            }

            // 3. Count Total (Before Paging)
            var totalCount = allAssets.Count();

            // 4. Paging Logic
            var pagedAssets = allAssets
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = pagedAssets.Select(a => new AssetDto
            {
                Id = a.Id,
                Name = a.Name,
                SerialNumber = a.SerialNumber,
                Status = a.Status.ToString(),
                PurchaseDate = a.PurchaseDate,
                CategoryName = a.Category?.Name ?? "Uncategorized"
            });

            // Return Wrapper with Metadata
            return Ok(new
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        // GET: api/assets/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssetDto>> GetAsset(int id)
        {
            var assets = await _unitOfWork.Repository<Asset>()
                .GetAsync(a => a.Id == id, a => a.Category!);

            var asset = assets.FirstOrDefault();

            if (asset == null)
            {
                _logger.LogWarning("Asset with ID {Id} not found", id);
                return NotFound();
            }

            var dto = new AssetDto
            {
                Id = asset.Id,
                Name = asset.Name,
                SerialNumber = asset.SerialNumber,
                Status = asset.Status.ToString(),
                PurchaseDate = asset.PurchaseDate,
                CategoryName = asset.Category?.Name ?? "Uncategorized"
            };

            return Ok(dto);
        }

        // POST: api/assets
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AssetDto>> CreateAsset(CreateAssetDto input)
        {
            _logger.LogInformation("Creating new asset: {Name}", input.Name);

            // 1. Validation: Check if Category exists
            var categoryExists = await _unitOfWork.Repository<Category>().ExistsAsync(input.CategoryId);
            if (!categoryExists)
            {
                return BadRequest($"Category ID {input.CategoryId} does not exist.");
            }

            // 2. Map DTO to Entity
            var asset = new Asset
            {
                Name = input.Name,
                SerialNumber = input.SerialNumber,
                Status = input.Status,
                PurchaseDate = input.PurchaseDate,
                CategoryId = input.CategoryId
            };

            // 3. Save
            await _unitOfWork.Repository<Asset>().AddAsync(asset);
            await _unitOfWork.CompleteAsync();

            return CreatedAtAction(nameof(GetAsset), new { id = asset.Id }, input);
        }

        // PUT: api/assets/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsset(int id, CreateAssetDto input)
        {
            var repo = _unitOfWork.Repository<Asset>();
            var asset = await repo.GetByIdAsync(id);

            if (asset == null) return NotFound();

            asset.Name = input.Name;
            asset.SerialNumber = input.SerialNumber;
            asset.Status = input.Status;
            asset.PurchaseDate = input.PurchaseDate;
            asset.CategoryId = input.CategoryId;

            await repo.UpdateAsync(asset);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/assets/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsset(int id)
        {
            var repository = _unitOfWork.Repository<Asset>();
            var asset = await repository.GetByIdAsync(id);

            if (asset == null) return NotFound();

            await repository.DeleteAsync(asset);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Asset {Id} soft deleted", id);
            return NoContent();
        }

        // PATCH: api/assets/5/status
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAssetStatus(int id, [FromBody] UpdateAssetStatusDto input)
        {
            var repo = _unitOfWork.Repository<Asset>();
            var asset = await repo.GetByIdAsync(id);

            if (asset == null) return NotFound();

            if (input.Status == AssetStatus.Available && asset.Status == AssetStatus.Assigned)
            {
                asset.UserId = null;
            }

            asset.Status = input.Status;

            await repo.UpdateAsync(asset);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }

        // GET: api/assets/my-assets
        [HttpGet("my-assets")]
        public async Task<ActionResult<IEnumerable<AssetDto>>> GetMyAssets()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var assets = await _unitOfWork.Repository<Asset>()
                .GetAsync(a => a.UserId == userId, a => a.Category!);

            var dtos = assets.Select(a => new AssetDto
            {
                Id = a.Id,
                Name = a.Name,
                SerialNumber = a.SerialNumber,
                Status = a.Status.ToString(),
                PurchaseDate = a.PurchaseDate,
                CategoryName = a.Category?.Name ?? "Uncategorized"
            });

            return Ok(dtos);
        }
    }
}