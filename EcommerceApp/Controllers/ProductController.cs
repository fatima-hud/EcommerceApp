using EcommerceApp.Entities;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;

        }
        [AllowAnonymous]
        [HttpGet("flash-sale")]
        public async Task<IActionResult> GetFlashSaleProducts()
        {
            var Today = DateTime.UtcNow;
            var products = await _context.Products.Include(e => e.DiscountSetting).Where(e => e.DiscountSetting != null
            && e.DiscountSetting.StartDate <= Today && Today <= e.DiscountSetting.EndDate
            && e.DiscountSetting.DiscountPercentage > 0).ToListAsync();
            var result = products.Select(p => new
            {
                p.Id,
                p.Type,
                p.Name,
                p.ImageUrl,

                OrginalPrice = p.Price,
                Discount = p.DiscountSetting.DiscountPercentage,
                DiscountedPrice = p.Price - (p.Price * (p.DiscountSetting.DiscountPercentage / 100))

            });
            return Ok(result);


        }
        [AllowAnonymous]
        [HttpGet("GetDetailsByProduct/{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var result = await _context.Products.Include(e => e.DiscountSetting).FirstOrDefaultAsync(p => p.Id == id);
            if (result == null)
                return NotFound("This product is not found");
            decimal discountedPrice = result.Price;

            if (result.DiscountSetting != null && result.DiscountSetting.DiscountPercentage > 0)
            {
                discountedPrice = result.Price - (result.Price * (decimal)result.DiscountSetting.DiscountPercentage / 100);
            }
            var res = new
            {
                ID = result.Id,

                Type = result.Type,
                Description = result.Description,
                OrginalPrice = result.Price,
                DiscountedPrice = result.DiscountSetting != null && result.DiscountSetting.DiscountPercentage > 0
                ? discountedPrice
               : (decimal?)null,


                Rating = result.Rating,
                Discount = result.DiscountSetting != null
    ? $"{result.DiscountSetting.DiscountPercentage}%"
    : null,


                ImageUrl = result.ImageUrl,

                DiscountEndDate = result.DiscountSetting?.EndDate
            };
            return Ok(res);





        }
        [AllowAnonymous]
        [HttpGet("SearchProducts")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("please enter your search");
            }
            var products = await _context.Products.Where(e => e.Type.Contains(query) || e.Description.Contains(query))
                .Select(p => new
                {
                    Id = p.Id,
                    Type = p.Type,
                    Description = p.Description,
                    Price = p.Price,
                    Rating = p.Rating,
                    ImageUrl = p.ImageUrl,
                    DiscountSetting = p.DiscountSetting


                }).ToListAsync();
            if (products is null || products.Count == 0)
            {
                return NotFound("Not Found your search");
            }

            return Ok(products);

        }
        [AllowAnonymous]
        [HttpGet("GetProductByCatgeory/{id}")]
        public async Task<IActionResult> GetProductsByCatgeory(int id)
        {
            var product = await _context.Products.Include(e => e.Category).Include(e => e.DiscountSetting).Where(e => e.CategoryId == id).Select(p => new
            {
                p.Id
                , p.Type,
                p.Description,
                p.Price,
                p.Rating, p.ImageUrl,
                CategoryName = p.Category.Name,
                DiscountPercentage = p.Price - (p.Price * p.DiscountSetting.DiscountPercentage / 100)


            }).ToListAsync();
            if (product is null || product.Count == 0)
            { return NotFound("product is  not found"); }
            return Ok(product);
        }

        [AllowAnonymous]
        [HttpGet("GetAllCategory")]
        public async Task<IActionResult> GetAllCategory()
        {
            var categories = await _context.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }
        [AllowAnonymous]
        [HttpGet("GetSixCategory")]
        public async Task<IActionResult> GetSixCategory()
        {
            var categories = await _context.Categories
                .Take(6).Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }





        [Authorize] 
        [HttpGet("history")]
        public async Task<IActionResult> GetSearchHistory()
        {


            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }



            var history = await _context.SearchHistories
                .Where(h => h.UserId == Id)
                .OrderByDescending(h => h.CreatedAt)
                .Select(h => h.SearchQuery)
                .ToListAsync();

            return Ok(history);
        }

        [Authorize]
        [HttpDelete("history")]
        public async Task<IActionResult> DeleteSearchHistory()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }

            var userHistory = await _context.SearchHistories
                .Where(h => h.UserId == Id)
                .ToListAsync();

            if (!userHistory.Any())
                return NotFound("لا يوجد سجل بحث للمستخدم.");

            _context.SearchHistories.RemoveRange(userHistory);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم حذف سجل البحث بنجاح." });
        }

        [AllowAnonymous]
        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularSearches()
        {
            var popular = await _context.SearchHistories
                .GroupBy(h => h.SearchQuery)
                .Select(g => new
                {
                    Term = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return Ok(popular);
        }


        [Authorize] 
        [HttpPost("add-search")]
        public async Task<IActionResult> AddSearchTerm([FromBody] string searchTerm)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest("كلمة البحث غير صالحة."); 

            var searchHistory = new SearchHistory
            {
                UserId = Id,                 
                SearchQuery = searchTerm.Trim(),        
                CreatedAt = DateTime.UtcNow    
            };

            _context.SearchHistories.Add(searchHistory); 
            await _context.SaveChangesAsync();           

            return Ok(new { success = true, message = "تم حفظ البحث." }); 
        }



    }
}
    




