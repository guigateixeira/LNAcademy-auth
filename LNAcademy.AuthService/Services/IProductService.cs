using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using LNAcademy.AuthService.Repositories;

namespace LNAcademy.AuthService.Services
{
    public interface IProductService
    {
        // DTOs for Product Service
        public class ProductDTO
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }
            public string Currency { get; set; }
            public Guid CreatorId { get; set; }
            public bool IsPublished { get; set; }
            public string? CoverImageUrl { get; set; }
            public string Type { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        public class CourseDTO : ProductDTO
        {
            public string Level { get; set; }
            public int ModuleCount { get; set; }
            public int LessonCount { get; set; }
        }

        public class BookDTO : ProductDTO
        {
            public string Author { get; set; }
            public string? Language { get; set; }
            public string? Format { get; set; }
            public string PreviewUrl { get; set; }
        }

        public class CreateProductRequest
        {
            [Required(ErrorMessage = "Title is required")]
            public string Title { get; set; }
    
            [Required(ErrorMessage = "Description is required")]
            public string Description { get; set; }
    
            [Required(ErrorMessage = "Price is required")]
            [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
            public decimal Price { get; set; }
            [Required(ErrorMessage = "Currency is required")]
            public string Currency { get; set; } = "SATS";
            public bool IsPublished { get; set; } = false;
            public string? CoverImageUrl { get; set; }
        }

        public class CreateCourseRequest : CreateProductRequest
        {
            public string Level { get; set; } = "Beginner";
        }

        public class CreateBookRequest : CreateProductRequest
        {
            public string Author { get; set; }
            public string? Language { get; set; }
            public string? Format { get; set; }
            public string? PreviewUrl { get; set; }
            public string? DownloadUrl { get; set; }
        }

        public class UpdateProductRequest
        {
            public string? Title { get; set; }
            public string? Description { get; set; }
            public decimal? Price { get; set; }
            public string? Currency { get; set; }
            public bool? IsPublished { get; set; }
            public string? CoverImageUrl { get; set; }
        }

        public class UpdateCourseRequest : UpdateProductRequest
        {
            public string? Level { get; set; }
        }

        public class UpdateBookRequest : UpdateProductRequest
        {
            public string? Author { get; set; }
            public string? Language { get; set; }
            public string? Format { get; set; }
            public string? PreviewUrl { get; set; }
            public string? DownloadUrl { get; set; }
        }

        // Product operations
        Task<ProductDTO> GetProductByIdAsync(Guid id);
        Task<IProductRepository.PaginatedResult<ProductDTO>> GetAllProductsAsync(
            IProductRepository.ProductFilterParams filterParams);
        Task<IProductRepository.PaginatedResult<ProductDTO>> GetMyProductsAsync(
            Guid userId, IProductRepository.ProductFilterParams filterParams);

        // Course operations
        Task<CourseDTO> GetCourseByIdAsync(Guid id, bool includeDetails = false);
        Task<IProductRepository.PaginatedResult<CourseDTO>> GetAllCoursesAsync(
            IProductRepository.ProductFilterParams filterParams);
        Task<CourseDTO> CreateCourseAsync(CreateCourseRequest request, Guid creatorId);
        Task<CourseDTO> UpdateCourseAsync(Guid id, UpdateCourseRequest request, Guid userId);
        Task<bool> DeleteCourseAsync(Guid id, Guid userId);

        // Book operations
        Task<BookDTO> GetBookByIdAsync(Guid id);
        Task<IProductRepository.PaginatedResult<BookDTO>> GetAllBooksAsync(
            IProductRepository.ProductFilterParams filterParams);
        Task<BookDTO> CreateBookAsync(CreateBookRequest request, Guid creatorId);
        Task<BookDTO> UpdateBookAsync(Guid id, UpdateBookRequest request, Guid userId);
        Task<bool> DeleteBookAsync(Guid id, Guid userId);
        Task<BookDTO> PublishBookAsync(Guid bookId, Guid userId);
    }
}