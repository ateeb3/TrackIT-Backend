using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TrackIT.API.DTOs.Dashboard;

using TrackIT.Core.DTOs;
using TrackIT.Core.Enums;
using TrackIT.Data.Context;   // Required for AssetStatus

namespace TrackIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            // 1. Fetch Stats
            var totalAssets = await _context.Assets.CountAsync();
            var inUse = await _context.Assets.CountAsync(a => a.Status == AssetStatus.Assigned);
            var available = await _context.Assets.CountAsync(a => a.Status == AssetStatus.Available);

            // Maintenance combines 'InRepair' + 'Broken'
            var maintenance = await _context.Assets.CountAsync(a => a.Status == AssetStatus.InRepair || a.Status == AssetStatus.Broken);

            // 2. Trend (Placeholder)
            var trend = 12;

            // 3. Utilization %
            var utilization = totalAssets > 0 ? (int)((double)inUse / totalAssets * 100) : 0;

            // 4. Fetch Recent Activity
            // We include the 'User' table so we can get the person's name
            var recentAssets = await _context.Assets
                .Include(a => a.User) // <--- CRITICAL: Join the User table
                .OrderByDescending(a => a.Id)
                .Take(5)
                .Select(a => new RecentActivityDto
                {
                    Id = a.Id,

                    // FIXED: Uses 'Name' instead of 'Model'
                    AssetName = a.Name,

                    // FIXED: Uses 'User' instead of 'AssignedTo'
                    // Checks if 'User' is null. If not, grabs 'FullName'.
                    AssignedTo = a.User != null ? a.User.FullName : "Unassigned",

                    Status = a.Status.ToString(),
                    Timestamp = a.PurchaseDate
                })
                .ToListAsync();

            // 5. Build Response
            var response = new DashboardDto
            {
                Stats = new DashboardStatsDto
                {
                    TotalAssets = totalAssets,
                    TotalAssetsTrend = trend,
                    InUse = inUse,
                    UtilizationRate = utilization,
                    Available = available,
                    Maintenance = maintenance
                },
                RecentActivities = recentAssets
            };

            return Ok(response);
        }
    }
}