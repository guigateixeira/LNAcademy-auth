using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public abstract class Product
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }
    
    [Column("title")]
    public string Title { get; set; }
    
    [Column("description")]
    public string Description { get; set; }
    
    [Column("price")]
    public decimal Price { get; set; }
    
    [Column("currency")]
    public Currency Currency { get; set; } = Currency.SATS;
    
    [Column("creator_id")]
    public Guid CreatorId { get; set; }
    
    [Column("is_published")]
    public bool IsPublished { get; set; }
    
    [Column("cover_image_url")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Cover image URL is optional")]
    public string? CoverImageUrl { get; set; }
    
    [Column("type")]
    public ProductType Type { get; set; }
    
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
    
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
    
    [Column("deleted_at")]
    public DateTime? DeletedAt { get; set; }
}

public enum ProductType
{
    Course,
    Book
}

public enum Currency
{
    SATS,
    USD,
}