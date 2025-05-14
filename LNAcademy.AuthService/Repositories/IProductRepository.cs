// Repositories/IProductRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LNAcademy.AuthService.Repositories
{
    public interface IProductRepository
    {
        // Define pagination and filter parameters
        public class ProductFilterParams
        {
            public string? SearchTerm { get; set; } // Search in title/description
            public int Page { get; set; } = 1;
            public int PageSize { get; set; } = 10;
            public bool IncludeUnpublished { get; set; } = false;
            public ProductType? ProductType { get; set; } // Filter by type
            public Currency? Currency { get; set; } // Filter by currency
            public decimal? MinPrice { get; set; } // Price range filter
            public decimal? MaxPrice { get; set; }
            public Guid? CreatorId { get; set; } // Filter by creator
        }

        // Paginated result class
        public class PaginatedResult<T>
        {
            public IEnumerable<T> Items { get; set; }
            public int TotalItems { get; set; }
            public int Page { get; set; }
            public int PageSize { get; set; }
            public int TotalPages { get; set; }
            public bool HasPreviousPage => Page > 1;
            public bool HasNextPage => Page < TotalPages;
        }

        // Generic product operations
        Task<Product?> GetByIdAsync(Guid id);
        Task<PaginatedResult<Product>> GetAllAsync(ProductFilterParams filterParams);
        Task<bool> DeleteAsync(Guid id);

        // Course specific operations
        Task<Course?> GetCourseByIdAsync(Guid id, bool includeModules = false, bool includeLessons = false);
        Task<PaginatedResult<Course>> GetAllCoursesAsync(ProductFilterParams filterParams);
        Task<Course> CreateCourseAsync(Course course);
        Task<Course?> UpdateCourseAsync(Course course);

        // Book specific operations
        Task<Book?> GetBookByIdAsync(Guid id);
        Task<PaginatedResult<Book>> GetAllBooksAsync(ProductFilterParams filterParams);
        Task<Book> CreateBookAsync(Book book);
        Task<Book?> UpdateBookAsync(Book book);
        
        // Save changes
        Task<bool> SaveChangesAsync();
    }
}