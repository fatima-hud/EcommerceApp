using EcommerceApp.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClothingController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly OutfitSuggestionService _outfitSuggestionService;

        public ClothingController(AppDbContext context, OutfitSuggestionService outfitSuggestionService)
        {
            _context = context;
            _outfitSuggestionService = outfitSuggestionService;
        }
        [HttpGet("GetSuggestOfClothes")]
        public async Task<IActionResult> GetSuggestOfClothes(int id)
        {
            var items = await _context.ClothingItems.FirstOrDefaultAsync(e => e.ProductId == id);


            if (items == null)
            {
                return NotFound("Not Found Product");
            }


            var result = await _outfitSuggestionService.SuggestMatchingItems(id);


            if (!result.Any())
            {
                return NotFound("Not Found any Suggest");
            }
            return Ok(result);


        }
        [HttpGet("GetCasualClothes")]
        public async Task<IActionResult> GetCasualClothes()
        {
            var items = await _context.Products.Where(e => e.Type == "Casual").Select(p => new
            {
               Id= p.Id,
               
               Image= p.ImageUrl
               
            })
                .ToListAsync();


            if (items == null)
            {
                return NotFound("Not Found Product");
            }
            
      return Ok(items);

        }
        [HttpGet("GetEveningWearClothes")]
        public async Task<IActionResult> GetEveningWearClothes()
        {
            var items = await _context.Products.Where(e => e.Type == "EveningWear").Select(p => new
            {
                Id = p.Id,

                Image = p.ImageUrl

            })
                .ToListAsync();


            if (items == null)
            {
                return NotFound("Not Found Product");
            }

            return Ok(items);

        }
        [HttpGet("GetSportClothes")]
        public async Task<IActionResult> GetSportClothes()
        {
            var items = await _context.Products.Where(e => e.Type == "Sport").Select(p => new
            {
                Id = p.Id,

                Image = p.ImageUrl

            })
                .ToListAsync();


            if (items == null)
            {
                return NotFound("Not Found Product");
            }

            return Ok(items);

        }
        [HttpGet("GetFormalClothes")]
        public async Task<IActionResult> GetFormalClothes()
        {
            var items = await _context.Products.Where(e => e.Type == "Formal").Select(p => new
            {
                Id = p.Id,

                Image = p.ImageUrl

            })
                .ToListAsync();


            if (items == null)
            {
                return NotFound("Not Found Product");
            }

            return Ok(items);

        }

    }
}
