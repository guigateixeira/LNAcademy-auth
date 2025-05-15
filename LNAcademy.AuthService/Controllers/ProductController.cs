using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LNAcademy.AuthService.Repositories;
using LNAcademy.AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LNAcademy.AuthService.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, 
            [FromQuery] string searchTerm = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                var filterParams = new IProductRepository.ProductFilterParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    IncludeUnpublished = false  // Public API only shows published products
                };

                var products = await _productService.GetAllProductsAsync(filterParams);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, new { message = "An error occurred while retrieving products" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                return Ok(product);
            }
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting product {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the product" });
            }
        }

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyProducts([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null, [FromQuery] bool includeUnpublished = true)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var filterParams = new IProductRepository.ProductFilterParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    IncludeUnpublished = includeUnpublished
                };

                var products = await _productService.GetMyProductsAsync(userId, filterParams);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user's products");
                return StatusCode(500, new { message = "An error occurred while retrieving your products" });
            }
        }
    }

    [ApiController]
    [Route("api/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<CoursesController> _logger;

        public CoursesController(IProductService productService, ILogger<CoursesController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                var filterParams = new IProductRepository.ProductFilterParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    ProductType = ProductType.Course,
                    IncludeUnpublished = false
                };

                var courses = await _productService.GetAllCoursesAsync(filterParams);
                return Ok(courses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses");
                return StatusCode(500, new { message = "An error occurred while retrieving courses" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourse(Guid id, [FromQuery] bool includeDetails = false)
        {
            try
            {
                var course = await _productService.GetCourseByIdAsync(id, includeDetails);
                return Ok(course);
            }
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting course {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the course" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCourse([FromBody] IProductService.CreateCourseRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var course = await _productService.CreateCourseAsync(request, userId);
                return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, course);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                return StatusCode(500, new { message = "An error occurred while creating the course" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] IProductService.UpdateCourseRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var course = await _productService.UpdateCourseAsync(id, request, userId);
                return Ok(course);
            }
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating course {id}");
                return StatusCode(500, new { message = "An error occurred while updating the course" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _productService.DeleteCourseAsync(id, userId);
                
                if (result)
                    return NoContent();
                    
                return NotFound(new { message = $"Course with ID {id} not found" });
            }
            catch (UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting course {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the course" });
            }
        }
    }

    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<BooksController> _logger;

        public BooksController(IProductService productService, ILogger<BooksController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] int page = 1, [FromQuery] int pageSize = 10,
            [FromQuery] string searchTerm = null, [FromQuery] decimal? minPrice = null, [FromQuery] decimal? maxPrice = null)
        {
            try
            {
                var filterParams = new IProductRepository.ProductFilterParams
                {
                    Page = page,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    ProductType = ProductType.Book,
                    IncludeUnpublished = false
                };

                var books = await _productService.GetAllBooksAsync(filterParams);
                return Ok(books);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books");
                return StatusCode(500, new { message = "An error occurred while retrieving books" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            try
            {
                var book = await _productService.GetBookByIdAsync(id);
                return Ok(book);
            }
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting book {id}");
                return StatusCode(500, new { message = "An error occurred while retrieving the book" });
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateBook([FromBody] IProductService.CreateBookRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var book = await _productService.CreateBookAsync(request, userId);
                return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating book");
                return StatusCode(500, new { message = "An error occurred while creating the book" });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateBook(Guid id, [FromBody] IProductService.UpdateBookRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var book = await _productService.UpdateBookAsync(id, request, userId);
                return Ok(book);
            }
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating book {id}");
                return StatusCode(500, new { message = "An error occurred while updating the book" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBook(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var result = await _productService.DeleteBookAsync(id, userId);
                
                if (result)
                    return NoContent();
                    
                return NotFound(new { message = $"Book with ID {id} not found" });
            }
            catch (UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting book {id}");
                return StatusCode(500, new { message = "An error occurred while deleting the book" });
            }
        }
        
        [HttpPost("{id}/publish")]
        [Authorize]
        public async Task<IActionResult> PublishBook(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var book = await _productService.PublishBookAsync(id, userId);
                return Ok(book);
            }
            catch (ProductNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { message = ex.Message, errorCode = ex.ErrorCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error publishing book {id}");
                return StatusCode(500, new { message = "An error occurred while publishing the book" });
            }
        }
    }
}