
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RetailApp.Data;
using RetailApp.Models;

namespace RetailApp.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly RetailDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductsController(RetailDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetActiveProducts()
        {
            return await _context.Products
                .Where(p => p.Status == "Active")
                .OrderByDescending(p => p.PostedDate)
                .ToListAsync();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string productName, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice, [FromQuery] DateTime? minPostedDate, [FromQuery] DateTime? maxPostedDate)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(productName))
                query = query.Where(p => p.Name.Contains(productName));
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);
            if (minPostedDate.HasValue)
                query = query.Where(p => p.PostedDate >= minPostedDate.Value);
            if (maxPostedDate.HasValue)
                query = query.Where(p => p.PostedDate <= maxPostedDate.Value);

            return await query.ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product)
        {
            if (product.Price > 10000)
                return BadRequest("Price cannot exceed $10,000.");

            _context.Products.Add(product);
            if (product.Price > 5000)
            {
                _context.ApprovalQueue.Add(new ApprovalQueue
                {
                    ProductId = product.Id,
                    Action = "Create"
                });
            }

            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetActiveProducts), new { id = product.Id }, product);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] Product updatedProduct)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            if (updatedProduct.Price > product.Price * 1.5m)
            {
                _context.ApprovalQueue.Add(new ApprovalQueue
                {
                    ProductId = productId,
                    Action = "Update"
                });
            }

            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            product.Status = updatedProduct.Status;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            _context.ApprovalQueue.Add(new ApprovalQueue
            {
                ProductId = productId,
                Action = "Delete"
            });

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("approval-queue")]
        public async Task<ActionResult<IEnumerable<ApprovalQueue>>> GetApprovalQueue()
        {
            return await _context.ApprovalQueue
                .OrderBy(aq => aq.RequestDate)
                .ToListAsync();
        }

        [HttpPut("approval-queue/{approvalId}/approve")]
        public async Task<IActionResult> ApproveProduct(int approvalId)
        {
            var approval = await _context.ApprovalQueue.FindAsync(approvalId);
            if (approval == null)
                return NotFound();

            _context.ApprovalQueue.Remove(approval);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("approval-queue/{approvalId}/reject")]
        public async Task<IActionResult> RejectProduct(int approvalId)
        {
            var approval = await _context.ApprovalQueue.FindAsync(approvalId);
            if (approval == null)
                return NotFound();

            _context.ApprovalQueue.Remove(approval);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
