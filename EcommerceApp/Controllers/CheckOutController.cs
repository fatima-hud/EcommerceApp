using EcommerceApp.Entities;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace EcommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckOutController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CheckOutController(AppDbContext context)
        {
            _context = context;
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CheckOut([FromBody]ShippingDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
                return Unauthorized("Invalid user");
            var check = await _context.Carts.Include(e => e.CartItems).ThenInclude(e => e.Product).ThenInclude(es=>es.DiscountSetting).FirstOrDefaultAsync(e => e.UserId == Id);
         
            if (check == null || check.CartItems.Count == 0) {
                return BadRequest("Cart is empty or does not exist.");
            }
            if (dto.ShippingAddress.IsNullOrEmpty())
                return BadRequest("Shipping address is required");
                

            var totalPrice = check.CartItems.Sum(i =>
         i.Quantity *
         (i.Product.DiscountSetting != null
             ? i.Product.Price - (i.Product.Price * i.Product.DiscountSetting.DiscountPercentage / 100)
             : i.Product.Price)
     );


            var order = new Order
            {
                UserId = Id,
                TotalPrice = totalPrice,
               
                CreateAt = DateTime.UtcNow,
               
              ShippingAddress = dto.ShippingAddress,
                OrderItems = check.CartItems.Select(i => new OrderItem
                {
                      

                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.DiscountSetting != null
            ? i.Product.Price - (i.Product.Price * i.Product.DiscountSetting.DiscountPercentage / 100)
            : i.Product.Price,
                    Color =i.Color,
                    Size=i.Size,
                }).ToList()
            };
            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(check.CartItems);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Order placed successfully.",
                orderId = order.Id,
                totalAmount = order.TotalPrice,
                orderDate = order.CreateAt,
                ShippingAdress=order.ShippingAddress
            });
        } 
    }
}
