using EcommerceApp.Entities;
using EcommerceApp.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EcommerceApp.Seed
{
    public class SeedData
    {

        public static async Task Seed(IApplicationBuilder app)
        {
            var services = app.ApplicationServices.CreateScope().ServiceProvider;
            var context = services.GetService<AppDbContext>()!;
            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            string jsonFilePath = Path.Combine(env.ContentRootPath, "Seed", "color.json");

            

            await SeedColors(context, jsonFilePath);



            // await SeedShop(context);
            //   await seedAds(context);

        }
        private static async Task SeedColors(AppDbContext context, string jsonFilePath)
        {
            string jsonContent = await File.ReadAllTextAsync(jsonFilePath);
            
            List<ColorJsonModel> colors = JsonSerializer.Deserialize<List<ColorJsonModel>>(
    jsonContent,
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
);

            var validColors = colors.Where(c => !string.IsNullOrEmpty(c.Name) && !string.IsNullOrEmpty(c.Hex)).ToList();
          //  Console.WriteLine(JsonSerializer.Serialize(validColors, new JsonSerializerOptions { WriteIndented = true }));


            foreach (var a in validColors)
            {
                // تحقق إذا اللون موجود مسبقًا
                var exists = await context.ColorEntities.FirstOrDefaultAsync(e => e.Hex == a.Hex);
                if (exists != null)
                    continue;

                // أولًا أنشئ الـ ColorEntity
                var colorEntity = new ColorEntity
                {
                    Name = a.Name,
                    Hex = a.Hex,
                    Rgb = a.Rgb
                };

                context.ColorEntities.Add(colorEntity);
               await context.SaveChangesAsync(); 

               
                foreach (var f in a.Families)
                {
                    var family = new ColorFamily
                    {
                        Name = f,
                        ColorEntityId = colorEntity.Id
                    };
                    context.ColorFamilies.Add(family);
                }
            }

            await context.SaveChangesAsync(); // حفظ كل الـ Families دفعة واحدة
        }
    }
}

