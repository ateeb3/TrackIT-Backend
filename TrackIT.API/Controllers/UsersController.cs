using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackIT.Core.DTOs; // Ensure you have DTOs or create them locally
using TrackIT.Core.Entities;

namespace TrackIT.API.Controllers
{
    [Authorize(Roles = "Admin")] // Only Admins can manage users
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(UserManager<AppUser> userManager) : ControllerBase
    {
        // 1. GET: List all employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.Department,
                    // Check if they are locked out or active?
                    IsActive = true
                })
                .ToListAsync();

            return Ok(users);
        }

        // 2. POST: Create a new employee

        [HttpPost]
        public async Task<IActionResult> CreateUser(RegisterDto input)
        {
            // check if email exists
            if (await userManager.FindByEmailAsync(input.Email) != null)
                return BadRequest("Email is already taken.");

            var user = new AppUser
            {
                UserName = input.Email,
                Email = input.Email,
                FullName = input.FullName,
                Department = input.Department,
                EmailConfirmed = true // Auto-confirm since Admin created it
            };

            // Create User with the password provided by Admin
            var result = await userManager.CreateAsync(user, input.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign "Employee" role by default
            await userManager.AddToRoleAsync(user, "Employee");

            return Ok(new { Message = "Employee created successfully" });
        }

        // 3. DELETE: Remove an employee
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Don't let Admin delete themselves!
            if (User.Identity?.Name == user.Email)
                return BadRequest("You cannot delete your own account.");

            // Soft Delete (using the IsDeleted flag we added earlier)
            user.IsDeleted = true;
            await userManager.UpdateAsync(user);

            return NoContent();
        }
    }
}