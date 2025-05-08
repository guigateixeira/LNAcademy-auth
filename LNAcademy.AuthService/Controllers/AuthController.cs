using System;
using System.Threading.Tasks;
using LNAcademy.AuthService.Data;
using LNAcademy.AuthService.Errors;
using LNAcademy.AuthService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static LNAcademy.AuthService.Middleware.ValidationMiddleware.Validators;
using LNAcademy.AuthService.Services;

namespace LNAcademy.AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserService _userService;

        public AuthController(ILogger<AuthController> logger,  IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public class SignupRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class SignupResponse
        {
            public Guid Id { get; set; }
            public string Email { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        [HttpPost("signup")]
        public async Task<ActionResult<SignupResponse>> Signup(SignupRequest request)
        {
            try
            {
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }

                if (!IsValidPassword(request.Password))
                {
                    return BadRequest(new { message = "Password must be at least 8 characters long" });
                }
                
                var sanitizedEmail = SanitizeInput(request.Email);
                var sanitizedPassword = SanitizeInput(request.Password);

                try
                {
                    var userDto = await _userService.SignupAsync(sanitizedEmail, sanitizedPassword);
                    
                    var response = new SignupResponse
                    {
                        Id = userDto.Id,
                        Email = userDto.Email,
                        CreatedAt = userDto.CreatedAt
                    };
                    
                    return Ok(response);
                }
                catch (AuthError ex)
                {
                    return StatusCode(ex.StatusCode, new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user signup");
                return StatusCode(500, new { message = "An error occurred during signup" });
            }
        }
        
        public class SignInRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class SignInResponse
        {
            public UserDTO User { get; set; }
            public string Token { get; set; }
        }

        [HttpPost("signin")]
        public async Task<ActionResult<SignInResponse>> SignIn(SignInRequest request)
        {
            try
            {
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Invalid email format" });
                }
                
                var sanitizedEmail = SanitizeInput(request.Email);
                var sanitizedPassword = SanitizeInput(request.Password);
        
                try
                {
                    var signedUser = await _userService.SignInAsync(sanitizedEmail, sanitizedPassword);
                    
                    var response = new SignInResponse
                    {
                        User = signedUser.User,
                        Token = signedUser.Token,
                    };
            
                    return Ok(response);
                }
                catch (AuthError ex)
                {
                    return StatusCode(ex.StatusCode, new { message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user signin");
                return StatusCode(500, new { message = "An error occurred during signin" });
            }
        }
    }
}