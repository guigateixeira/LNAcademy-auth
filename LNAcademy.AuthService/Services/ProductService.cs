using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNAcademy.AuthService.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LNAcademy.AuthService.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository productRepository,
            ILogger<ProductService> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        #region Product Operations

        public async Task<IProductService.ProductDTO> GetProductByIdAsync(Guid id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning($"Product with ID {id} not found");
                throw new ProductNotFoundException($"Product with ID {id} not found");
            }

            return MapToProductDTO(product);
        }

        public async Task<IProductRepository.PaginatedResult<IProductService.ProductDTO>> GetAllProductsAsync(
            IProductRepository.ProductFilterParams filterParams)
        {
            var result = await _productRepository.GetAllAsync(filterParams);

            // Map to DTOs
            var mappedItems = result.Items.Select(p => MapToProductDTO(p)).ToList();

            return new IProductRepository.PaginatedResult<IProductService.ProductDTO>
            {
                Items = mappedItems,
                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        }

        public async Task<IProductRepository.PaginatedResult<IProductService.ProductDTO>> GetMyProductsAsync(
            Guid userId, IProductRepository.ProductFilterParams filterParams)
        {
            // Set the CreatorId filter to only get products by this user
            filterParams.CreatorId = userId;
            
            return await GetAllProductsAsync(filterParams);
        }

        #endregion

        #region Course Operations

        public async Task<IProductService.CourseDTO> GetCourseByIdAsync(Guid id, bool includeDetails = false)
        {
            var course = await _productRepository.GetCourseByIdAsync(id, includeDetails, includeDetails);
            if (course == null)
            {
                _logger.LogWarning($"Course with ID {id} not found");
                throw new ProductNotFoundException($"Course with ID {id} not found");
            }

            return MapToCourseDTO(course);
        }

        public async Task<IProductRepository.PaginatedResult<IProductService.CourseDTO>> GetAllCoursesAsync(
            IProductRepository.ProductFilterParams filterParams)
        {
            // Ensure we're filtering only courses
            filterParams.ProductType = ProductType.Course;
            var result = await _productRepository.GetAllCoursesAsync(filterParams);

            // Map to DTOs
            var mappedItems = result.Items.Select(c => MapToCourseDTO(c)).ToList();

            return new IProductRepository.PaginatedResult<IProductService.CourseDTO>
            {
                Items = mappedItems,
                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        }

        public async Task<IProductService.CourseDTO> CreateCourseAsync(
            IProductService.CreateCourseRequest request, 
            Guid creatorId)
        {
            // Validate currency
            if (!Enum.TryParse<Currency>(request.Currency, out var currency))
            {
                _logger.LogWarning($"Invalid currency: {request.Currency}");
                throw new ValidationException("Invalid currency", "INVALID_CURRENCY");
            }

            // Create course
            var course = new Course
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Currency = currency,
                CreatorId = creatorId,
                IsPublished = request.IsPublished,
                CoverImageUrl = request.CoverImageUrl,
                Level = request.Level
            };

            // Save to database
            course = await _productRepository.CreateCourseAsync(course);

            return MapToCourseDTO(course);
        }

        public async Task<IProductService.CourseDTO> UpdateCourseAsync(
            Guid id, 
            IProductService.UpdateCourseRequest request, 
            Guid userId)
        {
            // Get existing course
            var course = await _productRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with ID {id} not found");
                throw new ProductNotFoundException($"Course with ID {id} not found");
            }

            // Check ownership
            if (course.CreatorId != userId)
            {
                _logger.LogWarning($"User {userId} tried to update course {id} without permission");
                throw new UnauthorizedException("You don't have permission to update this course");
            }

            // Update course properties if provided
            if (request.Title != null)
                course.Title = request.Title;

            if (request.Description != null)
                course.Description = request.Description;

            if (request.Price.HasValue)
                course.Price = request.Price.Value;

            if (request.Currency != null && Enum.TryParse<Currency>(request.Currency, out var currency))
                course.Currency = currency;

            if (request.IsPublished.HasValue)
                course.IsPublished = request.IsPublished.Value;

            if (request.CoverImageUrl != null)
                course.CoverImageUrl = request.CoverImageUrl;

            if (request.Level != null)
                course.Level = request.Level;

            // Update in database
            course = await _productRepository.UpdateCourseAsync(course);

            return MapToCourseDTO(course);
        }

        public async Task<bool> DeleteCourseAsync(Guid id, Guid userId)
        {
            // Get existing course
            var course = await _productRepository.GetCourseByIdAsync(id);
            if (course == null)
            {
                _logger.LogWarning($"Course with ID {id} not found");
                return false;
            }

            // Check ownership
            if (course.CreatorId != userId)
            {
                _logger.LogWarning($"User {userId} tried to delete course {id} without permission");
                throw new UnauthorizedException("You don't have permission to delete this course");
            }

            // Delete course
            return await _productRepository.DeleteAsync(id);
        }

        #endregion

        #region Book Operations

        public async Task<IProductService.BookDTO> GetBookByIdAsync(Guid id)
        {
            var book = await _productRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID {id} not found");
                throw new ProductNotFoundException($"Book with ID {id} not found");
            }

            return MapToBookDTO(book);
        }

        public async Task<IProductRepository.PaginatedResult<IProductService.BookDTO>> GetAllBooksAsync(
            IProductRepository.ProductFilterParams filterParams)
        {
            // Ensure we're filtering only books
            filterParams.ProductType = ProductType.Book;
            var result = await _productRepository.GetAllBooksAsync(filterParams);

            // Map to DTOs
            var mappedItems = result.Items.Select(b => MapToBookDTO(b)).ToList();

            return new IProductRepository.PaginatedResult<IProductService.BookDTO>
            {
                Items = mappedItems,
                TotalItems = result.TotalItems,
                Page = result.Page,
                PageSize = result.PageSize,
                TotalPages = result.TotalPages
            };
        }

        public async Task<IProductService.BookDTO> CreateBookAsync(
            IProductService.CreateBookRequest request, 
            Guid creatorId)
        {
            // Validate currency
            if (!Enum.TryParse<Currency>(request.Currency, out var currency))
            {
                _logger.LogWarning($"Invalid currency: {request.Currency}");
                throw new ValidationException("Invalid currency", "INVALID_CURRENCY");
            }

            // Create book
            var book = new Book
            {
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                Currency = currency,
                CreatorId = creatorId,
                IsPublished = request.IsPublished,
                CoverImageUrl = request.CoverImageUrl,
                Author = request.Author,
                Language = request.Language,
                Format = request.Format,
                PreviewUrl = request.PreviewUrl,
                DownloadUrl = request.DownloadUrl
            };

            // Save to database
            book = await _productRepository.CreateBookAsync(book);

            return MapToBookDTO(book);
        }

        public async Task<IProductService.BookDTO> UpdateBookAsync(
            Guid id, 
            IProductService.UpdateBookRequest request, 
            Guid userId)
        {
            // Get existing book
            var book = await _productRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID {id} not found");
                throw new ProductNotFoundException($"Book with ID {id} not found");
            }

            // Check ownership
            if (book.CreatorId != userId)
            {
                _logger.LogWarning($"User {userId} tried to update book {id} without permission");
                throw new UnauthorizedException("You don't have permission to update this book");
            }

            // Update book properties if provided
            if (request.Title != null)
                book.Title = request.Title;

            if (request.Description != null)
                book.Description = request.Description;

            if (request.Price.HasValue)
                book.Price = request.Price.Value;

            if (request.Currency != null && Enum.TryParse<Currency>(request.Currency, out var currency))
                book.Currency = currency;

            if (request.IsPublished.HasValue)
                book.IsPublished = request.IsPublished.Value;

            if (request.CoverImageUrl != null)
                book.CoverImageUrl = request.CoverImageUrl;

            if (request.Author != null)
                book.Author = request.Author;

            if (request.Language != null)
                book.Language = request.Language;

            if (request.Format != null)
                book.Format = request.Format;

            if (request.PreviewUrl != null)
                book.PreviewUrl = request.PreviewUrl;

            if (request.DownloadUrl != null)
                book.DownloadUrl = request.DownloadUrl;

            // Update in database
            book = await _productRepository.UpdateBookAsync(book);

            return MapToBookDTO(book);
        }

        public async Task<bool> DeleteBookAsync(Guid id, Guid userId)
        {
            // Get existing book
            var book = await _productRepository.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning($"Book with ID {id} not found");
                return false;
            }

            // Check ownership
            if (book.CreatorId != userId)
            {
                _logger.LogWarning($"User {userId} tried to delete book {id} without permission");
                throw new UnauthorizedException("You don't have permission to delete this book");
            }

            // Delete book
            return await _productRepository.DeleteAsync(id);
        }

        #endregion

        #region Mapping Methods

        private IProductService.ProductDTO MapToProductDTO(Product product)
        {
            return new IProductService.ProductDTO
            {
                Id = product.Id,
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                Currency = product.Currency.ToString(),
                CreatorId = product.CreatorId,
                IsPublished = product.IsPublished,
                CoverImageUrl = product.CoverImageUrl,
                Type = product.Type.ToString(),
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }

        private IProductService.CourseDTO MapToCourseDTO(Course course)
        {
            return new IProductService.CourseDTO
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                Currency = course.Currency.ToString(),
                CreatorId = course.CreatorId,
                IsPublished = course.IsPublished,
                CoverImageUrl = course.CoverImageUrl,
                Type = course.Type.ToString(),
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                Level = course.Level,
                ModuleCount = course.Modules?.Count ?? 0,
                LessonCount = course.Modules?.Sum(m => m.Lessons?.Count ?? 0) ?? 0
            };
        }

        private IProductService.BookDTO MapToBookDTO(Book book)
        {
            return new IProductService.BookDTO
            {
                Id = book.Id,
                Title = book.Title,
                Description = book.Description,
                Price = book.Price,
                Currency = book.Currency.ToString(),
                CreatorId = book.CreatorId,
                IsPublished = book.IsPublished,
                CoverImageUrl = book.CoverImageUrl,
                Type = book.Type.ToString(),
                CreatedAt = book.CreatedAt,
                UpdatedAt = book.UpdatedAt,
                Author = book.Author,
                Language = book.Language,
                Format = book.Format,
                PreviewUrl = book.PreviewUrl
            };
        }

        #endregion
    }

    // Custom exceptions
    public class ProductNotFoundException : Exception
    {
        public ProductNotFoundException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    public class ValidationException : Exception
    {
        public string ErrorCode { get; }

        public ValidationException(string message, string errorCode) : base(message)
        {
            ErrorCode = errorCode;
        }
    }
}