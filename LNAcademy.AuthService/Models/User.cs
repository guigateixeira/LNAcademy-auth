using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LNAcademy.AuthService.Models
{
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }
        
        [Required]
        [Column("email")]
        public string Email { get; set; }
        
        [Required]
        [Column("password")]
        public string Password { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }
        
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }
    }
}