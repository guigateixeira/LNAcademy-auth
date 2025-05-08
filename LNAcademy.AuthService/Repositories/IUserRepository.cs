using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LNAcademy.AuthService.Models;

namespace LNAcademy.AuthService.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> CreateAsync(User user);
        Task<User?> UpdateAsync(User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> SaveChangesAsync();
    }
}