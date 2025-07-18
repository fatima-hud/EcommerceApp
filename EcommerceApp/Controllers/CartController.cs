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
        private  readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }
      

        [Authorize]
        [HttpGet("GetUserCart")]
        public async Task<IActionResult>GetItem()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }
            
            var cart =await _context.Carts.Include(e=>e.CartItems).ThenInclude(e=>e.Product).Where(e=>e.UserId == Id).FirstOrDefaultAsync();
            if (cart == null) { return NotFound("cart is not found"); }
                var item = cart.CartItems.Select(e => new
                {
                    productId = e.ProductId,
                   productType =e.Product.Type,
                   productImage= e.Product.ImageUrl,
                   productPrice= e.Price,
                   productQuantity= e.Quantity,
                   
                }).ToList();
            return Ok(item);
        }


        [HttpPut("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody]DtoUpdateQuantity dto)
        {
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

            cartItem.Quantity =dto.Quantity;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Quantity updated successfully",
                itemId = item.Id,
                newQuantity =dto.Quantity
            });
         

        }
    }
}



/*
 *    [Authorize]
       [HttpPost("AddToCart")]
       public async Task<IActionResult> AddToCart([FromBody] AddToCart dto)
       {
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
               // المنتج موجود: زد الكمية فقط
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
                   Price = product.Price


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
            var userId= User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)||!int.TryParse(userId, out int Id))
            {
                return Unauthorized("Invalid user ID");
            }


            var result = await _context.CartItems.Include(e => e.Cart).Where(e => e.Cart.UserId == Id).CountAsync();
            return Ok(new { result });
        }
          [HttpDelete("delete comment")]
        public async Task<IActionResult>Delete([FromBody] int commentId)
        {
            var comment=await _context.Comments.FindAsync(commentId);
            if (comment==null) return NotFound("not found");
             _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return Ok("comment is deleted successfully");
        }
 */