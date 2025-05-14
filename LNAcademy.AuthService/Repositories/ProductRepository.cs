// Repositories/ProductRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNAcademy.AuthService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static LNAcademy.AuthService.Repositories.IProductRepository;

namespace LNAcademy.AuthService.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AuthDbContext _context;
        private readonly ILogger<ProductRepository> _logger;

        public ProductRepository(AuthDbContext context, ILogger<ProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Generic Product Operations

        public async Task<Product?> GetByIdAsync(Guid id)
        {
            return await _context.Products
                .Where(p => p.Id == id && p.DeletedAt == null)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResult<Product>> GetAllAsync(ProductFilterParams filterParams)
        {
            // Start with base query
            var query = _context.Products
                .Where(p => p.DeletedAt == null);

            // Apply filters
            query = ApplyFilters(query, filterParams);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((filterParams.Page - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            // Create paginated result
            return new PaginatedResult<Product>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filterParams.Page,
                PageSize = filterParams.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filterParams.PageSize)
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            // Soft delete
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return false;

            product.DeletedAt = DateTime.UtcNow;
            product.IsPublished = false;

            return await SaveChangesAsync();
        }

        #endregion

        #region Course Operations

        public async Task<Course?> GetCourseByIdAsync(Guid id, bool includeModules = false, bool includeLessons = false)
        {
            var query = _context.Courses
                .Where(c => c.Id == id && c.DeletedAt == null);

            if (includeModules)
            {
                query = query.Include(c => c.Modules.Where(m => m.DeletedAt == null)
                    .OrderBy(m => m.Order));

                if (includeLessons)
                {
                    query = query.Include(c => c.Modules
                        .Where(m => m.DeletedAt == null)
                        .OrderBy(m => m.Order))
                        .ThenInclude(m => m.Lessons
                            .Where(l => l.DeletedAt == null)
                            .OrderBy(l => l.Order));
                }
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<PaginatedResult<Course>> GetAllCoursesAsync(ProductFilterParams filterParams)
        {
            // Start with base query for courses
            var query = _context.Courses
                .Where(c => c.DeletedAt == null);

            // Apply filters
            query = (IQueryable<Course>)ApplyFilters(query, filterParams);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((filterParams.Page - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            // Create paginated result
            return new PaginatedResult<Course>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filterParams.Page,
                PageSize = filterParams.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filterParams.PageSize)
            };
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            course.UpdatedAt = DateTime.UtcNow;
            course.Type = ProductType.Course;

            await _context.Courses.AddAsync(course);
            await SaveChangesAsync();

            return course;
        }

        public async Task<Course?> UpdateCourseAsync(Course course)
        {
            var existingCourse = await _context.Courses
                .Where(c => c.Id == course.Id && c.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (existingCourse == null)
                return null;

            // Update properties
            existingCourse.Title = course.Title;
            existingCourse.Description = course.Description;
            existingCourse.Price = course.Price;
            existingCourse.Currency = course.Currency;
            existingCourse.IsPublished = course.IsPublished;
            existingCourse.CoverImageUrl = course.CoverImageUrl;
            existingCourse.Level = course.Level;
            existingCourse.UpdatedAt = DateTime.UtcNow;

            await SaveChangesAsync();
            return existingCourse;
        }

        #endregion

        #region Book Operations

        public async Task<Book?> GetBookByIdAsync(Guid id)
        {
            return await _context.Books
                .Where(b => b.Id == id && b.DeletedAt == null)
                .FirstOrDefaultAsync();
        }

        public async Task<PaginatedResult<Book>> GetAllBooksAsync(ProductFilterParams filterParams)
        {
            // Start with base query for books
            var query = _context.Books
                .Where(b => b.DeletedAt == null);

            // Apply filters
            query = (IQueryable<Book>)ApplyFilters(query, filterParams);

            // Get total count for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var items = await query
                .Skip((filterParams.Page - 1) * filterParams.PageSize)
                .Take(filterParams.PageSize)
                .ToListAsync();

            // Create paginated result
            return new PaginatedResult<Book>
            {
                Items = items,
                TotalItems = totalItems,
                Page = filterParams.Page,
                PageSize = filterParams.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filterParams.PageSize)
            };
        }

        public async Task<Book> CreateBookAsync(Book book)
        {
            book.CreatedAt = DateTime.UtcNow;
            book.UpdatedAt = DateTime.UtcNow;
            book.Type = ProductType.Book;

            await _context.Books.AddAsync(book);
            await SaveChangesAsync();

            return book;
        }

        public async Task<Book?> UpdateBookAsync(Book book)
        {
            var existingBook = await _context.Books
                .Where(b => b.Id == book.Id && b.DeletedAt == null)
                .FirstOrDefaultAsync();

            if (existingBook == null)
                return null;

            // Update properties
            existingBook.Title = book.Title;
            existingBook.Description = book.Description;
            existingBook.Price = book.Price;
            existingBook.Currency = book.Currency;
            existingBook.IsPublished = book.IsPublished;
            existingBook.CoverImageUrl = book.CoverImageUrl;
            existingBook.Author = book.Author;
            existingBook.Language = book.Language;
            existingBook.Format = book.Format;
            existingBook.DownloadUrl = book.DownloadUrl;
            existingBook.PreviewUrl = book.PreviewUrl;
            existingBook.UpdatedAt = DateTime.UtcNow;

            await SaveChangesAsync();
            return existingBook;
        }

        #endregion

        #region Helper Methods

        private IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductFilterParams filterParams)
        {
            // Filter by publication status
            if (!filterParams.IncludeUnpublished)
            {
                query = query.Where(p => p.IsPublished);
            }

            // Filter by search term (title or description)
            if (!string.IsNullOrWhiteSpace(filterParams.SearchTerm))
            {
                var searchTerm = filterParams.SearchTerm.ToLower();
                query = query.Where(p => 
                    p.Title.ToLower().Contains(searchTerm) || 
                    p.Description.ToLower().Contains(searchTerm));
            }

            // Filter by product type
            if (filterParams.ProductType.HasValue)
            {
                query = query.Where(p => p.Type == filterParams.ProductType.Value);
            }

            // Filter by currency
            if (filterParams.Currency.HasValue)
            {
                query = query.Where(p => p.Currency == filterParams.Currency.Value);
            }

            // Filter by price range
            if (filterParams.MinPrice.HasValue)
            {
                query = query.Where(p => p.Price >= filterParams.MinPrice.Value);
            }

            if (filterParams.MaxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= filterParams.MaxPrice.Value);
            }

            // Filter by creator
            if (filterParams.CreatorId.HasValue)
            {
                query = query.Where(p => p.CreatorId == filterParams.CreatorId.Value);
            }

            // Order by title by default
            query = query.OrderBy(p => p.Title);

            return query;
        }

        #endregion

        public async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database");
                throw;
            }
        }
    }
}