using EcommerceApp.Entities;
using EcommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApp
{
    public class OutfitSuggestionService
    {
        private readonly AppDbContext _context;

        public OutfitSuggestionService(AppDbContext context)
        {
            _context = context;
        }


        public async Task<bool> IsCompatibleColor(string color1, string color2)
        {
            var result = await _context.CompartibleColors.Where(e =>( e.Color == color1 && e.ColorCompartible == color2)||(e.ColorCompartible==color1 && e.Color==color2)).ToListAsync();
            return result.Any();

        }
        public async Task<IEnumerable<SuggestDto>> SuggestMatchingItems(int id)
        {
            var item = _context.ClothingItems.Include(e=>e.Product).FirstOrDefault(e => e.Id == id);
            var items = _context.ClothingItems.Include(e=>e.Product).Where(e => e.ProductId!= item.ProductId).Where(e => e.Style == item.Style).Where(e => e.Product.Type != item.Product.Type)
                .Where(e => e.Product.Gender == item.Product.Gender).Where(e => e.Product.Season == item.Product.Season).ToList();

            var result = new List<SuggestDto>();
           
            foreach (var i in items)
            {
                if (await IsCompatibleColor(item.Color, i.Color) == true)
                {
                   

                    var x = new SuggestDto
                    {
                        Id = i.Id,
                        Color = i.Color,
                        Size = i.Size,
                        Style = i.Style,

                        ProductId = i.ProductId,
                       Image=i.Product.ImageUrl,
                       Price = i.Product.Price




                    };
                    result.Add(x);
                }
            }
            


            return result;




        }
    }
}
