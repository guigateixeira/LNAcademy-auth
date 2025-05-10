using System;
using System.Threading.Tasks;

namespace LNAcademy.AuthService.Services
{
    public interface IUserService
    {
        Task<UserDTO> SignupAsync(string email, string password);
        Task<UserDTO> ValidateCredentialsAsync(string email, string password);
        Task<SigninDTO> SignInAsync(string email, string password);
        Task<UserDTO> GetUserAsync(GetUserRequest request);
    }
    
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SigninDTO
    {
        public UserDTO User { get; set; }
        public string Token { get; set; }
    }

    public class GetUserRequest
    {
        public string? Email { get; set; }
        public Guid? Id { get; set; }
    }
}