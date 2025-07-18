using EcommerceApp;
using EcommerceApp.Entities;
using EcommerceApp.Helper;
using EcommerceApp.Models;
using EcommerceApp.Services;
using Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;



using System.Text;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("constr")));
builder.Services.AddScoped<IAuthService, AuthService>(); 
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JWT"));
builder.Services.AddScoped<OutfitSuggestionService>();
builder.Configuration.AddEnvironmentVariables();
var connectionString = builder.Configuration["ConnectionStrings_constr"];
var jwtKey = builder.Configuration["JWT_Key"];
var email = builder.Configuration["EmailSettings_Email"];
var emailHost = builder.Configuration["EmailSettings_Host"];
var emailPort = builder.Configuration["EmailSettings_Port"];
var emailPassword = builder.Configuration["EmailSettings_Password"];

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddRoles<IdentityRole>().AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddTransient<IEmailSender, EmailSender>(); 
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    

.AddJwtBearer(o =>
{
    o.RequireHttpsMetadata = false;
    o.SaveToken = true;
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])),
        ClockSkew = TimeSpan.Zero
    };

    // ����� ����� �����
    o.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // ����� ������� ����������
            context.HandleResponse();
            
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = 401,
                message = "(��� ���� �� ������� - ������ ��� ���� �� ��� �����(��� ���� ����� ���� "
            });
            return context.Response.WriteAsync(result);
        },
        OnAuthenticationFailed =  async context =>
        {
            try
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "��� ������ �� ������ - ���� ����� ������� �� ��� ����"
                });
                await context.Response.WriteAsync(result);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception in OnAuthenticationFailed: " + ex.Message);
            }
        }
    };
});



 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EcommerceApp", Version = "v1" });

    // ��� ���� ��� ��� JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "���� ������ �����: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = 500,
            message = "��� ��� ����� �� ������",
            details = ex.Message
        });
        await context.Response.WriteAsync(result);
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.Run();

















/*������ ���������� */
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

   
    context.Database.EnsureCreated();

    if (!context.Users.Any()) 
    {
        var users = new List<User>
        {
            new User { FullName = "��� �����", Email = "ali@example.com", UserName = "aliothman", Phone = "0995555555", IsAdmin = false, Password = PasswordHasher.HashPassword("1234561") },
            new User { FullName = "��� ���", Email = "nada@example.com", UserName = "nadahasan", Phone = "0996666666", IsAdmin = false, Password = PasswordHasher.HashPassword("1234562") },
            new User { FullName = "��� �����", Email = "taim@example.com", UserName = "taim99", Phone = "0997777777", IsAdmin = false, Password = PasswordHasher.HashPassword("1234563") },
            new User { FullName = "���� ����", Email = "rawan@example.com", UserName = "rawans", Phone = "0998888888", IsAdmin = false, Password = PasswordHasher.HashPassword("1234564") },
            new User { FullName = "���� �����", Email = "basel@example.com", UserName = "baselq", Phone = "0999999999", IsAdmin = false, Password = PasswordHasher.HashPassword("1234565") },
            new User { FullName = "���� �����", Email = "ahmad@example.com", UserName = "ahmadali", Phone = "0991111111", IsAdmin = false, Password = PasswordHasher.HashPassword("123456") },
            new User { FullName = "���� ����", Email = "sara@example.com", UserName = "saramurad", Phone = "0992222222", IsAdmin = false, Password = PasswordHasher.HashPassword("1234567") },
            new User { FullName = "���� ����", Email = "khaled@example.com", UserName = "mkhaled", Phone = "0993333333", IsAdmin = false, Password = PasswordHasher.HashPassword("1234568") },
            new User { FullName = "��� �����", Email = "reem@example.com", UserName = "reem01", Phone = "0994444444", IsAdmin = false, Password = PasswordHasher.HashPassword("1234569") },
            new User { FullName = "��� ����� ���", Email = "admin@example.com", UserName = "adminuser", Phone = "0988888888", IsAdmin = true, Password = PasswordHasher.HashPassword("admin123") }
        };

        context.Users.AddRange(users);
        context.SaveChanges();
    }
}*/
/*
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Ensure database is created
    context.Database.EnsureCreated();

    // Seed Categories
   var categories = new List<Category>
    {
        new Category { Name = "�����", Description = "����� ������ �������" },
        new Category { Name = "����������", Description = "����� ������ ����" },
        new Category { Name = "����", Description = "���� ����� ������" },
        new Category { Name = "�����", Description = "����� ����� ��������" },
        new Category { Name = "���", Description = "������ ������ �� �����" },
    };
    context.Categories.AddRange(categories);*/

// Seed DiscountSettings
/*var discounts = new List<DiscountSetting>
{
    new DiscountSetting { DiscountPercentage = 10, StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(10) },
    new DiscountSetting { DiscountPercentage = 15, StartDate = DateTime.Now.AddDays(-5), EndDate = DateTime.Now.AddDays(15) },
    new DiscountSetting { DiscountPercentage = 20, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(30) },
    new DiscountSetting { DiscountPercentage = 5, StartDate = DateTime.Now.AddDays(-2), EndDate = DateTime.Now.AddDays(2) },
    new DiscountSetting { DiscountPercentage = 25, StartDate = DateTime.Now, EndDate = DateTime.Now.AddDays(5) },
};
context.DiscountSettings.AddRange(discounts);

// Seed Products
var products = new List<Product>
{
    new Product { Name = "���� �����", Description = "���� ���� ����", Price = 100, Quantity = 50, Category = categories[0], DiscountSetting = discounts[0], Rating = 4, ImageUrl = "shirt.jpg" },
    new Product { Name = "���� ���", Description = "���� ���� ������ ������", Price = 1200, Quantity = 20, Category = categories[1], DiscountSetting = discounts[1], Rating = 5, ImageUrl = "phone.jpg" },
    new Product { Name = "���� �����", Description = "���� ���� ������", Price = 300, Quantity = 15, Category = categories[2], DiscountSetting = discounts[2], Rating = 4, ImageUrl = "chair.jpg" },
    new Product { Name = "���� �������", Description = "������ �������", Price = 50, Quantity = 100, Category = categories[3], DiscountSetting = discounts[3], Rating = 3, ImageUrl = "toy.jpg" },
    new Product { Name = "����� �����", Description = "����� ����� ����� �������", Price = 40, Quantity = 70, Category = categories[4], DiscountSetting = discounts[4], Rating = 5, ImageUrl = "book.jpg" },
};
context.Products.AddRange(products);

// Seed Shipping Companies

var shippingCompanies = new List<ShippingCompany>
{
    new ShippingCompany { Name = "���� ������", PhoneNumber = "0999990001" },
    new ShippingCompany { Name = "���� DHL", PhoneNumber = "0999990002" },
    new ShippingCompany { Name = "���� ����", PhoneNumber = "0999990003" },
    new ShippingCompany { Name = "���� UPS", PhoneNumber = "0999990004" },
    new ShippingCompany { Name = "���� FedEx", PhoneNumber = "0999990005" },
};
context.ShippingCompanies.AddRange(shippingCompanies);

//Add sample users
// var users = context.Users.Take(5).ToList();

// Seed Orders
var orders = new List<Order>
{
    new Order { User = users[0], TotalPrice = 200, ShippingCompany = shippingCompanies[0], CreateAt = DateTime.Now },
    new Order { User = users[1], TotalPrice = 150, ShippingCompany = shippingCompanies[1], CreateAt = DateTime.Now },
    new Order { User = users[2], TotalPrice = 100, ShippingCompany = shippingCompanies[2], CreateAt = DateTime.Now },
    new Order { User = users[3], TotalPrice = 250, ShippingCompany = shippingCompanies[3], CreateAt = DateTime.Now },
    new Order { User = users[4], TotalPrice = 180, ShippingCompany = shippingCompanies[4], CreateAt = DateTime.Now },
};
context.Orders.AddRange(orders);

// Seed Payments
var payments = orders.Select(o => new Payment
{
    Order = o,
    Amount = o.TotalPrice,
    PaymentMethod = "�����",
    PaymentDate = DateTime.Now
}).ToList();
context.Payments.AddRange(payments);

// Seed OrderItems

var orderItems = new List<OrderItem>
{
    new OrderItem { Order = orders[0], Product = products[0], Quantity = 1, Price = products[0].Price, Size = "M", Color = "����" },
    new OrderItem { Order = orders[1], Product = products[1], Quantity = 1, Price = products[1].Price, Size = "L", Color = "����" },
    new OrderItem { Order = orders[2], Product = products[2], Quantity = 1, Price = products[2].Price, Size = "XL", Color = "�����" },
    new OrderItem { Order = orders[3], Product = products[3], Quantity = 2, Price = products[3].Price, Size = "M", Color = "����" },
    new OrderItem { Order = orders[4], Product = products[4], Quantity = 3, Price = products[4].Price, Size = "S", Color = "����" },
};
context.OrderItems.AddRange(orderItems);

// Seed Carts
var carts = users.Select(u => new Cart { User = u }).ToList();
context.Carts.AddRange(carts);
// Seed CartItems
/*var cartItems = new List<CartItem>
{
    new CartItem { Cart = carts[0], Product = products[0], Quantity = 1, Price = products[0].Price, Size = "M", Color = "����" },
    new CartItem { Cart = carts[1], Product = products[1], Quantity = 2, Price = products[1].Price, Size = "L", Color = "����" },
    new CartItem { Cart = carts[2], Product = products[2], Quantity = 3, Price = products[2].Price, Size = "XL", Color = "�����" },
    new CartItem { Cart = carts[3], Product = products[3], Quantity = 1, Price = products[3].Price, Size = "S", Color = "����" },
    new CartItem { Cart = carts[4], Product = products[4], Quantity = 1, Price = products[4].Price, Size = "M", Color = "����" },
};
context.CartItems.AddRange(cartItems);

// Seed Favorites

var favorites = new List<Favorite>
{
    new Favorite { User = users[0], Product = products[0] },
    new Favorite { User = users[1], Product = products[1] },
    new Favorite { User = users[2], Product = products[2] },
    new Favorite { User = users[3], Product = products[3] },
    new Favorite { User = users[4], Product = products[4] },
};
context.Favorites.AddRange(favorites);*/

// Seed Comments
/* var comments = new List<Comment>
 {
     new Comment { User = users[0], Product = products[0], text = "���� �����", CreatedDate = DateTime.Now },
     new Comment { User = users[1], Product = products[1], text = "���� �����", CreatedDate = DateTime.Now },
     new Comment { User = users[2], Product = products[2], text = "��� ���", CreatedDate = DateTime.Now },
     new Comment { User = users[3], Product = products[3], text = "���� ��", CreatedDate = DateTime.Now },
     new Comment { User = users[4], Product = products[4], text = "����� ����", CreatedDate = DateTime.Now },
 };
 context.Comments.AddRange(comments);

 //Seed SearchHistory
 var searchHistories = new List<SearchHistory>
 {
     new SearchHistory { User = users[0], SearchQuery = "����� ����" },
     new SearchHistory { User = users[1], SearchQuery = "���� �����" },
     new SearchHistory { User = users[2], SearchQuery = "����� �����" },
     new SearchHistory { User = users[3], SearchQuery = "��� ������" },
     new SearchHistory { User = users[4], SearchQuery = "����� �����" },
 };
 context.SearchHistories.AddRange(searchHistories);

 context.SaveChanges();
}*/




