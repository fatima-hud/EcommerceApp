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
    public class CommentController : ControllerBase
    {
private readonly AppDbContext _context;

        public CommentController(AppDbContext context)
        {
            _context = context;
        }
       


        [HttpGet("GetCommentsForProduct/{ProductId}")]
        public async Task<IActionResult> GetCommentsForProduct(int ProductId)
        {
            var Product=await _context.Products.AnyAsync(e=>e.Id==ProductId);
            if (!Product) return NotFound("Product Not Found ");
            var comments=await _context.Comments.Include(e=>e.User).Where(e=>e.ProductId==ProductId)
                .OrderByDescending(e => e.CreatedDate).Select(c=>new
                {
                 Id=c.Id,
                 text=c.text,
                UserName= c.User.UserName,
                CreatedDate= c.CreatedDate

               
                })
                .ToListAsync();
            return Ok(comments);
        }
        [HttpGet("GetTwoCommentsForProduct/{ProductId}")]
        public async Task<IActionResult> GetTwoCommentsForProduct(int ProductId)
        {
            var Product = await _context.Products.AnyAsync(e => e.Id == ProductId);
            if (!Product) return NotFound("Product Not Found ");
            var comments = await _context.Comments.Include(e => e.User).Where(e => e.ProductId == ProductId)
                .OrderByDescending(e => e.CreatedDate).Take(2).Select(c => new
                {
                    Id = c.Id,
                    text = c.text,
                    UserName = c.User.UserName,
                    CreatedDate = c.CreatedDate


                })
                .ToListAsync();
            return Ok(comments);
        }

















     

    }
}


/*
 *  [HttpPut("UpdateComment/{commentId}")]
        public async Task<IActionResult> UpdateComment(int commentId, [FromBody]string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return BadRequest("Comment text cannot be empty");
            var comment=await _context.Comments.FindAsync(commentId);
            if (comment == null) return NotFound("Comment not found");
            comment.text = text;
            comment.CreatedDate= DateTime.Now;
            await _context.SaveChangesAsync();
            return Ok("Comment updated successfully");
        }

 [HttpPost("add comment")]
        public async Task<IActionResult> AddComment([FromBody]AddComment dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
                return Unauthorized("Invalid user");
            var product =await  _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound("not found");
            var comment = new Comment
            {
                ProductId = dto.ProductId,
                text = dto.Text,
                UserId = parsedUserId,
                CreatedDate = DateTime.Now

            };
            _context.Comments.Add(comment);
           await _context.SaveChangesAsync();
            return Ok("Comment added successfully");

        }
*/