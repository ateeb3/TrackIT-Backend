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
    public class AssignmentsController(IUnitOfWork unitOfWork, ILogger<AssignmentsController> logger) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ILogger<AssignmentsController> _logger = logger;

        // GET: api/assignments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssignmentDto>>> GetAllHistory()
        {
            // Get all history, including Asset and User details
            var history = await _unitOfWork.Repository<AssignmentHistory>()
                .GetAsync(null, h => h.Asset!, h => h.User!);

            var dtos = history.Select(h => new AssignmentDto
            {
                Id = h.Id,
                AssignedDate = h.AssignedDate,
                ReturnDate = h.ReturnDate,
                Notes = h.Notes,
                IsActive = h.ReturnDate == null,

                // NEW: Map the Enum to a string for the frontend badge
                ReturnStatus = h.ReturnStatus.HasValue ? h.ReturnStatus.ToString() : null,

                AssetName = h.Asset?.Name ?? "Unknown Asset",
                SerialNumber = h.Asset?.SerialNumber ?? "N/A",
                UserName = h.User?.FullName ?? "Unknown User",
                Department = h.User?.Department ?? "N/A"
            });

            return Ok(dtos);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckoutAsset(CreateAssignmentDto input)
        {
            var assetRepo = _unitOfWork.Repository<Asset>();

            var asset = await assetRepo.GetByIdAsync(input.AssetId);
            if (asset == null) return NotFound("Asset not found");

            // 1. Validation: Is it already assigned?
            if (asset.Status == AssetStatus.Assigned)
                return BadRequest($"Asset '{asset.Name}' is already assigned.");

            // 2. Validation: Is it physically available?
            if (asset.Status == AssetStatus.Broken ||
                asset.Status == AssetStatus.Retired ||
                asset.Status == AssetStatus.InRepair)
            {
                return BadRequest($"Asset '{asset.Name}' is unavailable (Status: {asset.Status}).");
            }

            // 3. Create History Record
            var history = new AssignmentHistory
            {
                AssetId = input.AssetId,
                UserId = input.UserId,
                AssignedDate = input.AssignedDate,
                Notes = input.Notes
            };

            await _unitOfWork.Repository<AssignmentHistory>().AddAsync(history);

            // 4. Update Asset Status AND Current Holder
            asset.Status = AssetStatus.Assigned;
            asset.UserId = input.UserId; // <--- NEW: Track current user on the asset itself

            await assetRepo.UpdateAsync(asset);

            // 5. Commit Transaction
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Asset {AssetId} assigned to User {UserId}", input.AssetId, input.UserId);

            return Ok(new { Message = "Asset assigned successfully" });
        }

        // POST: api/assignments/return
        [HttpPost("return")]
        public async Task<IActionResult> ReturnAsset(ReturnAssignmentDto input)
        {
            var historyRepo = _unitOfWork.Repository<AssignmentHistory>();
            var history = await historyRepo.GetByIdAsync(input.AssignmentId);

            if (history == null) return NotFound("Assignment record not found");
            if (history.ReturnDate != null) return BadRequest("This asset was already returned.");

            // 1. Update History Record
            history.ReturnDate = DateTime.UtcNow;
            history.ReturnStatus = input.ReturnCondition; // Save the condition (Good/Broken)

            // Append notes if provided
            if (!string.IsNullOrEmpty(input.Notes))
            {
                history.Notes += $" | Return Note: {input.Notes}";
            }

            await historyRepo.UpdateAsync(history);

            // 2. Update the Actual Asset Inventory
            if (history.AssetId.HasValue)
            {
                var assetRepo = _unitOfWork.Repository<Asset>();
                var asset = await assetRepo.GetByIdAsync(history.AssetId.Value);

                if (asset != null)
                {
                    // CRITICAL: Set the asset's status to whatever the user selected
                    // (e.g. if they selected "Broken", asset becomes "Broken")
                    asset.Status = input.ReturnCondition;

                    // Unassign the user so it's free for the next person
                    asset.UserId = null;

                    await assetRepo.UpdateAsync(asset);
                }
            }

            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Asset returned successfully" });
        }
    }
}