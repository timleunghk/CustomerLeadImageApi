using CustomerLeadImageApi.Data;
using CustomerLeadImageApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerLeadImageApi.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomersController : ControllerBase
    {

        /// <summary>
        /// Manages customers and their uploaded images (stored in Base64 in the database).
        /// One Customer can only upload maximum 10 images
        /// </summary>

        private readonly AppDbContext _context;
        private const int MaxImagesPerCustomer = 10;

        public CustomersController(AppDbContext context) => _context = context;

        // CREATE customer with optional images
        /// <summary>
        /// Creates a new customer with optional uploaded images (max 10).
        /// </summary>
        /// <param name="request">Form-data including customer name and image files.</param>
        /// <returns>Newly created customer details including stored images.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateCustomer([FromForm] CustomerImageUploadRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new ApiResponse<string> { Status = "error", Message = "Customer name is required." });

            if (request.Files.Count > MaxImagesPerCustomer)
                return BadRequest(new ApiResponse<string> { Status = "error", Message = $"Maximum {MaxImagesPerCustomer} images allowed per customer." });

            var customer = new Customer { Name = request.Name };

            foreach (var file in request.Files)
            {
                customer.Images.Add(new CustomerImage
                {
                    Base64Data = await ConvertToBase64(file)
                });
            }

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Customer>
            {
                Status = "success",
                Message = $"{request.Files.Count} file(s) uploaded successfully.",
                Data = customer
            });
        }

        // GET all customers

        /// <summary>
        /// Retrieves all customers with their images.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<Customer>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers.Include(c => c.Images).ToListAsync();

            return Ok(new ApiResponse<List<Customer>>
            {
                Status = "success",
                Message = $"{customers.Count} customer(s) retrieved.",
                Data = customers
            });
        }
        /// <summary>
        /// Retrieves a specific customer by ID with their images.
        /// </summary>
        /// <param name="id">Customer ID.</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.Customers.Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound(new ApiResponse<string> { Status = "error", Message = "Customer not found." });

            return Ok(new ApiResponse<Customer>
            {
                Status = "success",
                Message = "Customer retrieved.",
                Data = customer
            });
        }

        /// <summary>
        /// Returns the raw image file (decoded from Base64) for preview/download.
        /// </summary>
        /// <param name="id">Customer ID.</param>
        /// <param name="imageId">Image ID for that customer.</param>
        [HttpGet("{id}/images/{imageId}/preview")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImagePreview(int id, int imageId)
        {
            var image = await _context.CustomerImages
                .FirstOrDefaultAsync(i => i.CustomerId == id && i.Id == imageId);

            if (image == null)
                return NotFound(new ApiResponse<string> { Status = "error", Message = "Image not found." });

            var bytes = Convert.FromBase64String(image.Base64Data);
            return File(bytes, "image/png");
        }
        /// <summary>
        /// Replace all images for specific customer
        /// </summary>
        /// <param name="id">Customer ID.</param>
        /// <param name="imageId">Image ID for that customer.</param>
        [HttpPut("{id}/images")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReplaceImages(int id, [FromForm] List<IFormFile> files)
        {
            var customer = await _context.Customers.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
                return NotFound(new ApiResponse<string> { Status = "error", Message = "Customer not found." });

            if (files.Count > MaxImagesPerCustomer)
                return BadRequest(new ApiResponse<string> { Status = "error", Message = $"Maximum {MaxImagesPerCustomer} images allowed per customer." });

            _context.CustomerImages.RemoveRange(customer.Images);
            customer.Images.Clear();

            foreach (var file in files)
            {
                customer.Images.Add(new CustomerImage
                {
                    Base64Data = await ConvertToBase64(file)
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Customer>
            {
                Status = "success",
                Message = $"{files.Count} file(s) replaced successfully.",
                Data = customer
            });
        }

        /// <summary>
        /// Add for specific customer
        /// </summary>
        /// <param name="id">Customer ID.</param>
        /// <param name="imageId">Image ID for that customer.</param>
        [HttpPatch("{id}/images")]
        [ProducesResponseType(typeof(ApiResponse<Customer>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> files)
        {
            var customer = await _context.Customers.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
                return NotFound(new ApiResponse<string> { Status = "error", Message = "Customer not found." });

            int availableSlots = MaxImagesPerCustomer - customer.Images.Count;
            if (files.Count > availableSlots)
                return BadRequest(new ApiResponse<string> { Status = "error", Message = $"Only {availableSlots} more images allowed (limit {MaxImagesPerCustomer})." });

            foreach (var file in files)
            {
                customer.Images.Add(new CustomerImage
                {
                    Base64Data = await ConvertToBase64(file)
                });
            }

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<Customer>
            {
                Status = "success",
                Message = $"{files.Count} file(s) added successfully.",
                Data = customer
            });
        }

        /// <summary>
        /// Delete ALL images for specific customer
        /// </summary>
        /// <param name="id">Customer ID.</param>
        /// <param name="imageId">Image ID for that customer.</param>
        [HttpDelete("{id}/images")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAllImages(int id)
        {
            var customer = await _context.Customers.Include(c => c.Images).FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
                return NotFound(new ApiResponse<string> { Status = "error", Message = "Customer not found." });

            _context.CustomerImages.RemoveRange(customer.Images);
            customer.Images.Clear();

            await _context.SaveChangesAsync();

            return Ok(new ApiResponse<string>
            {
                Status = "success",
                Message = "All images deleted successfully."
            });
        }



        /// <summary>
        /// Delete ALL images for specific customer
        /// </summary>
        /// <param name="id">Customer ID.</param>
        /// <param name="imageId">Image ID for that customer.</param>
        [HttpGet("{id}/images/count")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetImageCount(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound(new ApiResponse<string>
                {
                    Status = "error",
                    Message = "Customer not found."
                });

            int count = customer.Images.Count;

            return Ok(new ApiResponse<object>
            {
                Status = "success",
                Message = $"Customer {id} has {count} image(s).",
                Data = new { customerId = id, imageCount = count }
            });
        }


        // Helper method: Convert File Binary format to Base64 Format
        private static async Task<string> ConvertToBase64(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            return Convert.ToBase64String(ms.ToArray());
        }

    }


}