using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LNAcademy.AuthService.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidationMiddleware> _logger;

        public ValidationMiddleware(RequestDelegate next, ILogger<ValidationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Continue to the next middleware
            await _next(context);
        }

        // Static utility methods that can be used by controllers or other middleware
        public static class Validators 
        {
            public static bool IsValidEmail(string email)
            {
                if (string.IsNullOrWhiteSpace(email))
                    return false;

                // Simple regex for email validation
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern);
            }

            public static bool IsValidPassword(string password)
            {
                return !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
            }

            public static string SanitizeInput(string input)
            {
                if (string.IsNullOrWhiteSpace(input))
                    return input;

                // Simple sanitization - replace < and > with their HTML entities
                input = input.Replace("<", "&lt;");
                input = input.Replace(">", "&gt;");
                return input;
            }
        }
    }

    // Extension method to make it easy to add this middleware
    public static class ValidationMiddlewareExtensions
    {
        public static IApplicationBuilder UseValidation(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ValidationMiddleware>();
        }
    }
}