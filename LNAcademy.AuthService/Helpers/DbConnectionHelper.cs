using Microsoft.Extensions.Configuration;
using System;

namespace LNAcademy.AuthService.Helpers
{
    public static class DbConnectionHelper
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            // Get base connection string from configuration
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Database connection string is not configured.");
            
            // Get required environment variables
            string host = Environment.GetEnvironmentVariable("POSTGRES_HOST")
                ?? throw new InvalidOperationException("POSTGRES_HOST environment variable is not set.");
            
            string port = Environment.GetEnvironmentVariable("POSTGRES_PORT")
                ?? throw new InvalidOperationException("POSTGRES_PORT environment variable is not set.");
            
            string database = Environment.GetEnvironmentVariable("POSTGRES_DB")
                ?? throw new InvalidOperationException("POSTGRES_DB environment variable is not set.");
            
            string user = Environment.GetEnvironmentVariable("POSTGRES_USER")
                ?? throw new InvalidOperationException("POSTGRES_USER environment variable is not set.");
            
            string password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
                ?? throw new InvalidOperationException("POSTGRES_PASSWORD environment variable is not set.");
            
            // Replace placeholders with environment variable values
            connectionString = connectionString
                .Replace("${POSTGRES_HOST}", host)
                .Replace("${POSTGRES_PORT}", port)
                .Replace("${POSTGRES_DB}", database)
                .Replace("${POSTGRES_USER}", user)
                .Replace("${POSTGRES_PASSWORD}", password);
            
            return connectionString;
        }
    }
}