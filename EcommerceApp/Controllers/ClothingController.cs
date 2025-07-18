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

      /*  [HttpGet("GetSuggestOfClothes")]
        public async Task<IActionResult> GetSuggestOfClothes(int id)
        {
            var items = await _context.ClothingItems.FirstOrDefaultAsync(e=>e.Id==id);
           
    
            if(items == null)
            {
                return NotFound("Not Found Product");
            }
          
           
            var result = await _outfitSuggestionService.SuggestMatchingItems(id);
         
            if (! result.Any())
            {
                return NotFound("Not Found any Suggest");
            }
            return Ok(result
           
                
         );

        }*/
    }
}
