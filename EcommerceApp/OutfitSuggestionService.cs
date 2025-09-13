using EcommerceApp.Entities;
using EcommerceApp.Models;
using EcommerceApp.SeedData;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text.Json;

namespace EcommerceApp
{
    public class OutfitSuggestionService
    {
        private readonly AppDbContext _context;

        public OutfitSuggestionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsCompatibleColorByFamily(string colorHex1, string colorHex2)
        {
            colorHex1 = colorHex1.Trim().ToLower();
            colorHex2 = colorHex2.Trim().ToLower();

            var families1 = await _context.ColorEntities
                .Where(c => c.Hex.ToLower().Trim() == colorHex1)
                .SelectMany(c => c.Families.Select(f => f.Name.ToLower().Trim()))
                .ToListAsync();

            var families2 = await _context.ColorEntities
                .Where(c => c.Hex.ToLower().Trim() == colorHex2)
                .SelectMany(c => c.Families.Select(f => f.Name.ToLower().Trim()))
                .ToListAsync();

            var color1Name = await _context.ColorEntities
                .Where(c => c.Hex.ToLower().Trim() == colorHex1)
                .Select(c => c.Name.ToLower().Trim())
                .FirstOrDefaultAsync();

            var color2Name = await _context.ColorEntities
                .Where(c => c.Hex.ToLower().Trim() == colorHex2)
                .Select(c => c.Name.ToLower().Trim())
                .FirstOrDefaultAsync();

            // الفحص بالاتجاهين
            return families1.Contains(color2Name) || families2.Contains(color1Name);
        }

        public async Task<IEnumerable<SuggestDto>> SuggestMatchingItems(int id)
        {
            var item = await _context.ClothingItems
                .Include(e => e.Product)
                .FirstOrDefaultAsync(e => e.ProductId == id);

            var itemSizes = await _context.ClothingItems
                .Where(c => c.ProductId == item.ProductId)
                .Select(c => c.Size)
                .ToListAsync();

            var items = await _context.ClothingItems
                .Include(e => e.Product)
                .Where(e => e.ProductId != item.ProductId)
                .Where(e =>
                    // إذا القطعة الحالية أو الثانية Shoes → تجاهل شرط الـ Type
                    (item.Product.StyleCloth.ToLower().Trim() == "shoes" || e.Product.StyleCloth.ToLower().Trim() == "shoes")
                    ||
                    (e.Product.Type.Trim() == item.Product.Type.Trim())
                    ||
                    (e.Product.Type.Trim() == "Casual" && (item.Product.Type.ToLower().Trim() == "Sport" && item.Product.StyleCloth.ToLower().Trim() == "shoes"))
                    ||
                    ((e.Product.Type.ToLower().Trim() == "Sport" && e.Product.StyleCloth.ToLower().Trim() == "shoes") && item.Product.Type.Trim() == "Casual")
                    ||
                    (e.Product.Type.Trim() == "Formal" && item.Product.Type.Trim() == "EveningWear")
                    ||
                    (e.Product.Type.Trim() == "EveningWear" && item.Product.Type.Trim() == "Formal")
                )
                .Where(e => e.Product.Gender == item.Product.Gender)
                .Where(e =>
                    e.Product.Season.ToLower().Trim() == item.Product.Season.ToLower().Trim()
                    || e.Product.Season.ToLower().Trim() == "all"
                    || item.Product.Season.ToLower().Trim() == "all"
                    || ((item.Product.Season.ToLower().Trim() == "autumn" && e.Product.Season.ToLower().Trim() == "winter" && e.Product.StyleCloth.ToLower().Trim() == "shoes")
                        || (item.Product.Season.ToLower().Trim() == "winter" && item.Product.StyleCloth.ToLower().Trim() == "shoes" && e.Product.Season.ToLower().Trim() == "autumn"))
                )
                .Where(e =>
                    (item.Product.StyleCloth.ToLower() == "shoes" || e.Product.StyleCloth.ToLower() == "shoes")
                        ? true   // إذا أي واحد من القطع Shoes → تجاهل شرط الـ Size
                        : itemSizes.Contains(e.Size) // غير هيك لازم تتطابق المقاسات
                )
                .Where(e => e.Product.StyleCloth.Trim() != item.Product.StyleCloth.Trim())
                .ToListAsync();

            var uniqueItems = items
                .GroupBy(e => e.ProductId)
                .Select(g => g.First())
                .ToList();

            var styleCompatibility = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "Pants", new List<string>{ "Shirt", "T-shirt", "Shoes" } },
                { "Shirt", new List<string>{ "Pants", "T-shirt", "Shoes", "ShortPants" } },
                { "Skirt", new List<string>{ "Shirt", "Shoes", "T-shirt" } },
                { "Dress", new List<string>{ "Shoes" } },
                { "Shoes", new List<string>{ "Pants", "Shirt", "T-shirt", "Skirt", "Dress", "ShortPants" } },
                { "T-shirt", new List<string>{ "Pants", "ShortPants", "Skirt", "Shoes" } },
                { "ShortPants", new List<string>{ "Shirt", "T-shirt", "Shoes" } }
            };

            var result = new List<SuggestDto>();

            foreach (var i in uniqueItems)
            {
                if (await IsCompatibleColorByFamily(item.Color, i.Color))
                {
                    if (styleCompatibility.TryGetValue(item.Product.StyleCloth, out var compatibleStyles))
                    {
                        if (!compatibleStyles.Contains(i.Product.StyleCloth, StringComparer.OrdinalIgnoreCase))
                            continue; // StyleCloth غير متوافق
                    }

                    // منع Sport Shoes مع EveningWear أو Formal
                    bool isSportShoesConflict =
                        (item.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                            && item.Product.Type.Equals("sport", StringComparison.OrdinalIgnoreCase)
                            && (i.Product.Type.Equals("eveningwear", StringComparison.OrdinalIgnoreCase)
                                || i.Product.Type.Equals("formal", StringComparison.OrdinalIgnoreCase)))
                        ||
                        (i.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                            && i.Product.Type.Equals("sport", StringComparison.OrdinalIgnoreCase)
                            && (item.Product.Type.Equals("eveningwear", StringComparison.OrdinalIgnoreCase)
                                || item.Product.Type.Equals("formal", StringComparison.OrdinalIgnoreCase)));

                    // منع EveningWear Shoes مع Casual أو Sport
                    bool isEveningShoesConflict =
                        (item.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                            && item.Product.Type.Equals("eveningwear", StringComparison.OrdinalIgnoreCase)
                            && (i.Product.Type.Equals("casual", StringComparison.OrdinalIgnoreCase)
                                || i.Product.Type.Equals("sport", StringComparison.OrdinalIgnoreCase)))
                        ||
                        (i.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                            && i.Product.Type.Equals("eveningwear", StringComparison.OrdinalIgnoreCase)
                            && (item.Product.Type.Equals("casual", StringComparison.OrdinalIgnoreCase)
                                || item.Product.Type.Equals("sport", StringComparison.OrdinalIgnoreCase)));

                    bool isFormalShoesConflict =
                       (item.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                           && item.Product.Type.Equals("formal", StringComparison.OrdinalIgnoreCase)
                           && (i.Product.Type.Equals("casual", StringComparison.OrdinalIgnoreCase)
                               || i.Product.Type.Equals("sport", StringComparison.OrdinalIgnoreCase)))
                       ||
                       (i.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                           && i.Product.Type.Equals("formal", StringComparison.OrdinalIgnoreCase)
                           && (item.Product.Type.Equals("casual", StringComparison.OrdinalIgnoreCase)
                               || item.Product.Type.Equals("sport", StringComparison.OrdinalIgnoreCase)));
                    bool isCasualShoesConflict =
                      (item.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                          && item.Product.Type.Equals("casual", StringComparison.OrdinalIgnoreCase)
                          && (i.Product.Type.Equals("eveningwear", StringComparison.OrdinalIgnoreCase)
                              || i.Product.Type.Equals("formal", StringComparison.OrdinalIgnoreCase)))
                      ||
                      (i.Product.StyleCloth.Equals("shoes", StringComparison.OrdinalIgnoreCase)
                          && i.Product.Type.Equals("casual", StringComparison.OrdinalIgnoreCase)
                          && (item.Product.Type.Equals("eveningwear", StringComparison.OrdinalIgnoreCase)
                              || item.Product.Type.Equals("formal", StringComparison.OrdinalIgnoreCase)));



                    if (isSportShoesConflict || isEveningShoesConflict||isCasualShoesConflict||isFormalShoesConflict)
                        continue;



                    var x = new SuggestDto
                    {
                        ClothingitemID = i.Id,
                        Color = i.Color,
                        Size = i.Size,
                        Style = i.Product.Type,
                        Type = i.Product.StyleCloth,
                        ProductId = i.ProductId,
                        Image = i.Product.ImageUrl,
                        Price = i.Product.Price,
                        Discountsetting = i.Product.DiscountSetting != null
                        ? i.Product.Price - (i.Product.Price * i.Product.DiscountSetting.DiscountPercentage / 100)
                        : i.Product.Price
                    };

                    result.Add(x);
                }
            }

            return result;
        }
    }
}
