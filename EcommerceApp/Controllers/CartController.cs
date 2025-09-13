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
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }


        [Authorize]
        [HttpGet("GetUserCart")]
        public async Task<IActionResult> GetItem()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }

            var cart = await _context.Carts
    .Include(e => e.CartItems)
        .ThenInclude(e => e.Product)
            .ThenInclude(p => p.DiscountSetting) // 👈 أضف هذا
    .Where(e => e.UserId == Id)
    .FirstOrDefaultAsync();

            if (cart == null) { return NotFound("cart is not found"); }
            var item = cart.CartItems.Select(e => new
            {
                productId = e.ProductId,
                productType = e.Product.Type,
                productImage = e.Product.ImageUrl,
                productPrice = e.Price,
                productQuantity = e.Quantity,
                name=e.Product.StyleCloth,
                 DiscountedPrice = e.Product.DiscountSetting != null
                        ? e.Product.Price - (e.Product.Price * e.Product.DiscountSetting.DiscountPercentage / 100)
                        :  e.Product.Price

            }).ToList();
            return Ok(item);
        }

        [Authorize]
        [HttpPut("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] DtoUpdateQuantity dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
                return Unauthorized("Invalid user");
            var item = await _context.Carts.Include(e => e.CartItems).Where(e => e.UserId == Id).FirstOrDefaultAsync();

            if (item == null) { return NotFound("Item not found in the specified cart for this user."); }
            if (dto.Quantity <= 0)
            {
                return BadRequest("Quantity must be greater than 0.");
            }

            var cartItem = item.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
            if (cartItem == null)
                return NotFound("Product not found in cart");

            cartItem.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Quantity updated successfully",
                itemId = item.Id,
                newQuantity = dto.Quantity
            });


        }


        /*[Authorize]
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCart dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
          
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                return Unauthorized("Invalid user");
            var cart = await _context.Carts.Include(e => e.CartItems).Where(e => e.UserId == parsedUserId)
                .FirstOrDefaultAsync();
            if (cart == null)
            {
                cart = new Cart { UserId = parsedUserId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return NotFound("Not Found Product");
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
          ci.ProductId == dto.ProductId &&
          ci.Size == dto.Size &&
          ci.Color == dto.Color
      );

            if (existingItem != null)
            {
                
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    CartId = cart.Id,
                    Size = dto.Size,
                    Color = dto.Color,
                    Quantity = dto.Quantity,
                    Price = product.DiscountSetting != null
                        ? product.Price - (product.Price * product.DiscountSetting.DiscountPercentage / 100)
                        : product.Price
                };

                _context.CartItems.Add(cartItem);
            }

        
        await _context.SaveChangesAsync();
            return Ok("Product added to cart successfully");


        }*/

        [Authorize]
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCart dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                return Unauthorized("Invalid user");

            var cart = await _context.Carts
                .Include(e => e.CartItems)
                .FirstOrDefaultAsync(e => e.UserId == parsedUserId);

            if (cart == null)
            {
                cart = new Cart { UserId = parsedUserId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            // ✅ اجلب المنتج مع الخصم
            var product = await _context.Products
                .Include(p => p.DiscountSetting)
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

            if (product == null)
            {
                return NotFound("Not Found Product");
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci =>
                ci.ProductId == dto.ProductId &&
                ci.Size == dto.Size &&
                ci.Color == dto.Color
            );

            // ✅ احسب السعر مع الخصم
            decimal finalPrice = product.DiscountSetting != null
                ? product.Price - (product.Price * product.DiscountSetting.DiscountPercentage / 100)
                : product.Price;

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
                existingItem.Price = finalPrice; // 👈 تأكد انه يتحدث إذا تغير الخصم
            }
            else
            {
                var cartItem = new CartItem
                {
                    ProductId = dto.ProductId,
                    CartId = cart.Id,
                    Size = dto.Size,
                    Color = dto.Color,
                    Quantity = dto.Quantity,
                    Price = finalPrice
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return Ok("Product added to cart successfully");
        }

        [Authorize]

        [HttpGet("GetNumberProductsInCart")]
        public async Task<IActionResult> GetNumberProductsInCart()
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }


            var result = await _context.CartItems.Include(e => e.Cart).Where(e => e.Cart.UserId == Id).CountAsync();
            return Ok(new { result });
        }
        [Authorize]
        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteItem(int productId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }
            var cart = await _context.Carts.Where(e => e.UserId == Id).FirstOrDefaultAsync();


            var result = await _context.CartItems.Where(e => e.CartId == cart.Id).ToListAsync();
            if (result == null)
            {
                return NotFound("Cart not found for this user");
            }
            var products = await _context.CartItems.Where(e => e.CartId == cart.Id).Where(e => e.ProductId == productId).FirstOrDefaultAsync();
            if (products == null)
            {
                return NotFound("Not Found Product");
            }
            _context.CartItems.Remove(products);
            await _context.SaveChangesAsync();

            return Ok("Product removed from cart successfully.");






        }
        [Authorize]
        [HttpGet("GetTotalPrice")]
        public async Task<IActionResult> GetTotalPrice()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }
            var res = await _context.Carts.Where(e => e.UserId == Id).FirstOrDefaultAsync();
            if (res == null)
            {
                return NotFound("Cart not found for this user");


            }
            var item = await _context.CartItems.Where(e => e.CartId == res.Id).SumAsync(i =>  i.Quantity *
          
        
         (i.Product.DiscountSetting != null
             ? i.Price - (i.Price * i.Product.DiscountSetting.DiscountPercentage / 100)
             : i.Price)
     
);
            return Ok(item);


        }

        [Authorize]
        [HttpGet("GetMyOrders")]
        public async Task<IActionResult> GetMyOrders()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                return Unauthorized("Invalid user");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                     .ThenInclude(p => p.DiscountSetting)
                .Where(o => o.UserId == parsedUserId)
                .OrderByDescending(o => o.CreateAt)
                .ToListAsync();

            var result = orders.Select(o => new
            {
                o.Id,
                o.TotalPrice,
                o.CreateAt,
                o.ShippingAddress,
                Items = o.OrderItems.Select(oi => new
                {
                    oi.Id,
                    oi.ProductId,
                    ProductName = oi.Product.StyleCloth,
                    oi.Quantity,
                    oi.Price,
                    oi.Size,
                    oi.Color,
                    oi.Product.ImageUrl,
                     DiscountedPrice = oi.Product.DiscountSetting != null
                        ? oi.Product.Price - (oi.Product.Price * oi.Product.DiscountSetting.DiscountPercentage / 100)
                        : oi.Product.Price
                }).ToList()
            });

            return Ok(result);
        }

    }
}




        
 