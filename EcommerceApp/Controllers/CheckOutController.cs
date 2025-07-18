using EcommerceApp.Entities;
using EcommerceApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
      /*  [HttpPost]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
                return Unauthorized("Invalid user");
            var check = await _context.Carts.Include(e => e.CartItems).ThenInclude(e => e.Product).FirstOrDefaultAsync(e => e.UserId == Id);
         
            if (check == null || check.CartItems.Count == 0) {
                return BadRequest("Cart is empty or does not exist.");
            }

            var TotalPrice = check.CartItems.Sum(e => e.Quantity * e.Product.Price);
            var order = new Order
            {
                UserId = Id,
                TotalPrice = TotalPrice,
                ShippingCompanyId = dto.ShippingId,
                CreateAt = DateTime.Now,
                Payment = new Payment
                {
                    PaymentMethod = dto.PaymentMethod,
                },


                OrderItems = check.CartItems.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price,
                    Color=i.Color,
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
                orderDate = order.CreateAt
            });
        } */
    }
}
