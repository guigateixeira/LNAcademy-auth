using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LNAcademy.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LNAcademy.AuthService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        // Response DTO for user information
        public class UserResponse
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        [HttpGet("me")]
        [Authorize] // This attribute ensures the endpoint requires authentication
        public async Task<ActionResult<UserResponse>> GetMe()
        {
            try
            {
                // Get the authenticated user's ID from the claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    _logger.LogWarning("User ID claim missing or invalid in token");
                    return Unauthorized(new { message = "Invalid token" });
                }
                
                var request = new GetUserRequest { Id = userId };
                
                var user = await _userService.GetUserAsync(request);
                
                if (user == null)
                {
                    _logger.LogWarning($"User with ID {userId} not found in database");
                    return NotFound(new { message = "User not found" });
                }
                
                var response = new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user information");
                return StatusCode(500, new { message = "An error occurred while retrieving user information" });
            }
        }
    }
}