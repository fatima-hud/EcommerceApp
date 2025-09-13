using EcommerceApp.Entities;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
               
                p.ImageUrl,
               name= p.StyleCloth,
                p.Gender,
                rating= p.Rating,
                Quantity=p.Quantity,

                OrginalPrice = p.Price,
             
                DiscountedPrice = p.DiscountSetting != null
                        ? p.Price - (p.Price * p.DiscountSetting.DiscountPercentage / 100)
                        : p.Price

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
            var productColors = await _context.ClothingItems.Where(e => e.ProductId == id).Select(c => c.AllColor).ToListAsync();
            var productSizes = await _context.ClothingItems.Where(e => e.ProductId == id).Select(c => c.Size).ToListAsync();
            var productComments = await _context.Comments.Where(e => e.ProductId == id).Select(c => c.text).ToListAsync();
            var res = new
            {
                ID = result.Id,
               
                Type = result.Type,
                Description = result.Description,
                OrginalPrice = result.Price,
                Name= result.Name,
                StyleCloth= result.StyleCloth,
                DiscountedPrice = result.DiscountSetting != null && result.DiscountSetting.DiscountPercentage > 0
                ? discountedPrice
               : (decimal?)null,


                Rating = result.Rating,
                Discount = result.DiscountSetting != null
    ? $"{result.DiscountSetting.DiscountPercentage}%"
    : null,


                ImageUrl = result.ImageUrl,

                DiscountEndDate = result.DiscountSetting?.EndDate,
                Colors= productColors,
                Sizes= productSizes,
                Comments= productComments,
                Quantity= result.Quantity,

            };
            return Ok(res);





        }

        [AllowAnonymous]
        [HttpPost("SearchProducts")]
        public async Task<IActionResult> SearchProductsAdvanced([FromQuery] string query)
        {
            var products = await _context.Products
                .Where(e =>
                    e.Description.ToLower().Contains(query.ToLower()) ||
                    e.StyleCloth.ToLower().Equals(query.ToLower())
                )
                .Select(p => new
                {
                    Id = p.Id,
                    Type = p.Type,
                    Description = p.Description,
                    Price = p.Price,
                    name = p.StyleCloth,
                    Rating = p.Rating,
                    ImageUrl = p.ImageUrl,
                    Quantity= p.Quantity,
                    DiscountPercentage = p.DiscountSetting != null
                        ? p.Price - (p.Price * p.DiscountSetting.DiscountPercentage / 100)
                        : p.Price
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound("No products found for the given filters");
            }

            return Ok(products);
        }

        [AllowAnonymous]
        [HttpPost("SearchProductsAdvanced")]
        public async Task<IActionResult> SearchProductsAdvanced([FromBody] dtoSearch dto)
        {
            var query = _context.ClothingItems.AsQueryable();

            // فلترة بالسعر
            if (dto.minPrice.HasValue && dto.maxPrice.HasValue)
            {
                // بين min و max
                query = query.Where(e =>
                    (e.Product.Price >= dto.minPrice.Value && e.Product.Price <= dto.maxPrice.Value)
                    ||
                    (
                        e.Product.DiscountSetting != null &&
                        (e.Product.Price - (e.Product.Price * e.Product.DiscountSetting.DiscountPercentage / 100))
                        >= dto.minPrice.Value &&
                        (e.Product.Price - (e.Product.Price * e.Product.DiscountSetting.DiscountPercentage / 100))
                        <= dto.maxPrice.Value
                    )
                );
            }
            else if (dto.minPrice.HasValue)
            {
                // min فقط (>= minPrice)
                query = query.Where(e =>
                    (e.Product.Price >= dto.minPrice.Value)
                    ||
                    (
                        e.Product.DiscountSetting != null &&
                        (e.Product.Price - (e.Product.Price * e.Product.DiscountSetting.DiscountPercentage / 100))
                        >= dto.minPrice.Value
                    )
                );
            }
            else if (dto.maxPrice.HasValue)
            {
                // max فقط (<= maxPrice)
                query = query.Where(e =>
                    (e.Product.Price <= dto.maxPrice.Value)
                    ||
                    (
                        e.Product.DiscountSetting != null &&
                        (e.Product.Price - (e.Product.Price * e.Product.DiscountSetting.DiscountPercentage / 100))
                        <= dto.maxPrice.Value
                    )
                );
            }

            // فلترة بالستايل
            if (!string.IsNullOrWhiteSpace(dto.StyleCloth))
            {
                query = query.Where(e => e.Product.StyleCloth.ToLower().Equals(dto.StyleCloth.ToLower()));
            }

            // فلترة باللون
            if (!string.IsNullOrWhiteSpace(dto.Color))
            {
                query = query.Where(e => e.Product.Description.ToLower().Contains(dto.Color.ToLower()));
            }

            // فلترة بالمقاس
            if (!string.IsNullOrWhiteSpace(dto.Size))
            {
                query = query.Where(e => e.Size.ToLower().Equals(dto.Size.ToLower()));
            }

            var products = await query
                .Select(p => new
                {
                    Id = p.ProductId,
                    Type = p.Product.Type,
                    Description = p.Product.Description,
                    Price = p.Product.Price,
                    Size = p.Size,
                    name = p.Product.StyleCloth,
                    Rating = p.Product.Rating,
                    ImageUrl = p.Product.ImageUrl,
                  Quantity= p.Product.Quantity,
                    DiscountPercentage = p.Product.DiscountSetting != null
                        ? p.Product.Price - (p.Product.Price * p.Product.DiscountSetting.DiscountPercentage / 100)
                        : p.Product.Price
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound("No products found for the given filters");
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
                name=p.StyleCloth,
                p.Rating, p.ImageUrl,
                CategoryName = p.Category.Name,
                Quantity=p.Quantity,
                DiscountPercentage = p.DiscountSetting != null
                        ? p.Price - (p.Price * p.DiscountSetting.DiscountPercentage / 100)
                        : p.Price


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


        [AllowAnonymous]
        [HttpGet("GetProductByName")]
        public async Task<IActionResult> GetProductsByName(string name)
        {

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Product name is required.");
            }


            var products = await _context.Products
                .Where(e => e.Name.ToLower() == name.ToLower())
                .Select(p => new
                {
                    p.Id,
                    p.Type,
                    p.Description,
                    p.Price,
                    p.Rating,
                    p.ImageUrl,
                   name= p.StyleCloth,
                    CategoryName = p.Category.Name,
                    Quantity=p.Quantity,
                    p.Name,
                    DiscountPercentage = p.DiscountSetting != null
                        ? p.Price - (p.Price * p.DiscountSetting.DiscountPercentage / 100)
                        : p.Price
                })
                .ToListAsync();

            if (products == null || products.Count == 0)
            {
                return NotFound("Product not found.");
            }

            return Ok(products);
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


            var popular = await _context.SearchStatics
     .OrderByDescending(s => s.Count)
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
            var stat = await _context.SearchStatics.FirstOrDefaultAsync(s => s.Term == searchTerm);
            
            if (stat == null)
            {
               var x= new SearchStatic { Term = searchTerm, Count = 1 }; 
                _context.SearchStatics.Add(x);
            }
            else
            {
                stat.Count++;
            }
            


            _context.SearchHistories.Add(searchHistory);
          
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "تم حفظ البحث." });
        }
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddProduct([FromBody] DtoProduct dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var entity = new Product
            {
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                Quantity = dto.Quantity,
                DiscountSettingId = dto.DiscountSettingId,
                Rating = dto.Rating,
                ImageUrl = dto.ImageUrl,
                Gender = dto.Gender,
                Season = dto.Season,
                Type = dto.type,       // انتبه لاسم الخاصية لديك
                Name = dto.Name,
                StyleCloth = dto.StyleCloth
            };
            _context.Products.Add(entity);
            await _context.SaveChangesAsync();
            var x = new ClothingItem
            {
                Style = dto.StyleCloth,
                Color = dto.Color,
                Size = dto.Size,
                ProductId = entity.Id,
                AllColor = dto.Color
            };
            _context.ClothingItems.Add(x);

          
            await _context.SaveChangesAsync();
            return Ok("Admin add product");
        }
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Not found product");

            
            var relatedItems = _context.ClothingItems.Where(e => e.ProductId == id);
            _context.ClothingItems.RemoveRange(relatedItems);

            
            var cartItems = _context.CartItems.Where(e => e.ProductId == id);
            _context.CartItems.RemoveRange(cartItems);

            var orderItems = _context.OrderItems.Where(e => e.ProductId == id);
            _context.OrderItems.RemoveRange(orderItems);
            await _context.SaveChangesAsync();

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return Ok("Deleted successfully");
        }


        [HttpPut]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var product=await _context.Products.FindAsync(dto.Id);
            if (product == null) return NotFound("not found product");
            if (dto.Price.HasValue)
            {
                product.Price = dto.Price.Value;
            }


            if (!string.IsNullOrWhiteSpace(dto.ProductName))
            {
                product.StyleCloth = dto.ProductName;
            }
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                product.Description = dto.Description;
            }

            if (dto.Quantity.HasValue)
            {
                product.Quantity = dto.Quantity.Value;
            }

            if (dto.DiscountSettingId.HasValue)
            {
                   product.DiscountSettingId = dto.DiscountSettingId;
           
            }

        
           
            

            
            await _context.SaveChangesAsync();
            return Ok("Admin update product");
        }
        [HttpPost("AddDiscount")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddDiscount([FromBody] DtoDiscountSetting discount)
        {
            var dis = new DiscountSetting
            {
                DiscountPercentage = discount.DiscountPercentage,
                StartDate = discount.StartDate
                ,
                EndDate = discount.EndDate
            };
            _context.DiscountSettings.Add(dis);
            await _context.SaveChangesAsync();
            return Ok("Admin add discount");
        }


        [HttpDelete("DeleteDiscount")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteDiscount(int id)
        {
            var discount = await _context.DiscountSettings.FindAsync(id);
            if (discount == null)
                return NotFound();

            // تحديث كل المنتجات المرتبطة بالخصم لتصير NULL
            var productsWithDiscount = _context.Products.Where(p => p.DiscountSettingId == id);
            foreach (var product in productsWithDiscount)
            {
                product.DiscountSettingId = null;
            }

            // حذف الخصم نفسه
            _context.DiscountSettings.Remove(discount);

            await _context.SaveChangesAsync();

            return Ok("Discount deleted successfully, related products updated.");
        }

        [HttpPut("UpdateDiscount")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateDiscount(int id, [FromBody] DtoDiscountSetting updatedDiscount)
        {
            var discount = await _context.DiscountSettings.FindAsync(id);
            if (discount == null)
            {
                return NotFound(new { message = "الخصم غير موجود" });
            }

            // تحديث القيم
            discount.DiscountPercentage = updatedDiscount.DiscountPercentage;
            discount.StartDate = updatedDiscount.StartDate;
            discount.EndDate = updatedDiscount.EndDate;

            _context.DiscountSettings.Update(discount);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم تعديل الخصم بنجاح", discount });
        }
        [HttpGet("GetAllDiscount")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<DiscountSetting>>> GetDiscounts()
        {
            var discounts = await _context.DiscountSettings.ToListAsync();
            return Ok(discounts);
        }
    }







}

    




