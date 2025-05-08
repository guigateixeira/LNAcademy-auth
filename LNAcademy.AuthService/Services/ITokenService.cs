using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LNAcademy.AuthService.Models;

namespace LNAcademy.AuthService.Services
{
    public interface ITokenService
    {
        string GenerateJwtToken(Guid userId, string email);
        bool ValidateToken(string token);
    }
}