using System;
using System.Threading.Tasks;
using LNAcademy.AuthService.Models;
using LNAcademy.AuthService.Repositories;
using Microsoft.Extensions.Logging;
using BCrypt.Net;
using LNAcademy.AuthService.Errors;

namespace LNAcademy.AuthService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenService _tokenService;

        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _tokenService = tokenService;
        }

        public async Task<UserDTO> SignupAsync(string email, string password)
        {
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning($"Signup attempt with existing email: {email}");
                throw new AuthError("Email already in use.", 400);
            }
            
            var userId = Guid.NewGuid();
            
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                Id = userId,
                Email = email,
                Password = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            _logger.LogInformation($"New user created with email: {email}");
            
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserDTO> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning($"Login attempt with non-existent email: {email}");
                throw new AuthError("Wrong email or password.", 401);
            }

            // Verify the password hash using BCrypt
            bool passwordIsValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            
            if (!passwordIsValid)
            {
                _logger.LogWarning($"Failed login attempt for user: {email}");
                throw new AuthError("Wrong email or password.", 401);
            }

            _logger.LogInformation($"Successful login for user: {email}");

            // Return user DTO
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<SigninDTO> SignInAsync(string email, string password)
        {
            var user = await this.ValidateCredentialsAsync(email, password);
            var token = this._tokenService.GenerateJwtToken(user.Id, user.Email);

            return new SigninDTO
            {
                Token = token,
                User = user
            };
        }
        
        public async Task<UserDTO> GetUserAsync(GetUserRequest request)
        {
            User? user = null;

            // Check if we have an ID
            if (request.Id.HasValue)
            {
                user = await _userRepository.GetByIdAsync(request.Id.Value);
            }
            // Otherwise try to find by email
            else if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user = await _userRepository.GetByEmailAsync(request.Email);
            }
            else
            {
                _logger.LogWarning("GetUserAsync called with neither ID nor email");
                throw new AuthError(
                    "INVALID_REQUEST", 
                    400
                );
            }

            // User not found
            if (user == null)
            {
                _logger.LogWarning($"User not found. ID: {request.Id}, Email: {request.Email}");
                return null;
            }

            // Return user DTO
            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }
    }
}