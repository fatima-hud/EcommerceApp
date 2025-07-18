using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EcommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }
      /*  [HttpGet("UnreadCount")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
                return Unauthorized("Invalid user");
            int count = await _context.Notifications.Where(r => r.UserId == Id && !r.IsRead).CountAsync();

            return Ok(count);
        }
        [HttpPut("markasread/{NotificationId}")]
        public async Task<IActionResult> UpdateNotification(int NotificationId)
        {
            var noty = await _context.Notifications.FindAsync(NotificationId);
            if (noty == null)
            {
                return BadRequest("Notification not found.");
            }
            noty.IsRead = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Notification marked as read." });

        }*/
    }
}
