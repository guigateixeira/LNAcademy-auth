using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LNAcademy.AuthService.Data;
using LNAcademy.AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace LNAcademy.AuthService.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => u.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateAsync(User user)
        {
            // Check if user exists
            var existingUser = await GetByIdAsync(user.Id);
            if (existingUser == null)
                return null;
            
            // Update user
            _context.Users.Update(user);
            await SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                return false;

            // Soft delete
            user.DeletedAt = DateTime.UtcNow;
            _context.Users.Update(user);
            return await SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return (await _context.SaveChangesAsync() > 0);
        }
    }
}