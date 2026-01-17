using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TrackIT.Core.DTOs;
using TrackIT.Core.Entities;
using TrackIT.Core.Interfaces;

namespace TrackIT.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService) : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly ITokenService _tokenService = tokenService;

        // NOTE: I removed "Register" because you want Admins to create users via UsersController.
        // Keeping a public Register endpoint would allow anyone to join your company app!

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // 1. Find User
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null) return Unauthorized("Invalid Email");

            // 2. Check Password
            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized("Invalid Password");

            // 3. Check if account is "Active" (Soft Delete check)
            if (user.IsDeleted) return Unauthorized("Account is disabled.");

            // 4. Fetch Roles (CRITICAL for Frontend Sidebar)
            var roles = await _userManager.GetRolesAsync(user);

            // 5. Return Token + Roles
            return new UserDto
            {
                Email = user.Email!,
                FullName = user.FullName,
                Token = await _tokenService.CreateTokenAsync(user),
                Roles = roles.ToList() // <--- Sending this to Angular
            };
        }

        [HttpPost("change-password")]
        [Authorize] // User must be logged in
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return NotFound("User not found");

            // Optional: Ensure the logged-in user matches the email in the request
            // (Prevents changing someone else's password if they somehow guessed the email)
            if (User.Identity.Name != user.UserName)
                return Forbid();

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = result.Errors.First().Description });
            }

            return Ok(new { Message = "Password changed successfully" });
        }
    }
}