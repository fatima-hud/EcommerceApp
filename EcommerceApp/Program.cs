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
using EcommerceApp.Seed;
using System.Text.Json;


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


    //  Œ’Ì’ —”«∆· «·Œÿ√
    o.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            //  Ã«Ê“ «·—”«·… «·«› —«÷Ì…
            context.HandleResponse();
            
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var result = System.Text.Json.JsonSerializer.Serialize(new
            {
                status = 401,
                message = "(€Ì— „’—Õ ·ﬂ »«·Ê’Ê· - «· Êﬂ‰ €Ì— ’«·Õ √Ê €Ì— „ÊÃÊœ(ÌÃ» ⁄·Ìﬂ  ”ÃÌ· œŒÊ· "
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
                    message = "›‘· «· Õﬁﬁ „‰ «· Êﬂ‰ - —»„« «‰ Â  ’·«ÕÌ Â √Ê €Ì— ’ÕÌÕ"
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("IsAdmin", "True"));
});




builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EcommerceApp", Version = "v1" });

    // Â‰« ‰÷Ì› œ⁄„ «·‹ JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "«œŒ· «· Êﬂ‰ »’Ì€…: Bearer {token}"
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
//if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
////{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

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
            message = "ÕœÀ Œÿ√ œ«Œ·Ì ›Ì «·Œ«œ„",
            details = ex.Message
        });
        await context.Response.WriteAsync(result);
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();


app.MapControllers();
app.MapGet("/", () => "„—Õ»«° «· ÿ»Ìﬁ ‘€«·!");

await SeedData.Seed(app);


app.Run();

























