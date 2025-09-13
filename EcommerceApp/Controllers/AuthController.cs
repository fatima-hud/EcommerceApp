using EcommerceApp.Entities;
using EcommerceApp.Models;
using EcommerceApp.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;
using System.Net;
using EcommerceApp.Helper;
using Microsoft.Identity.Client;
using Azure.Core;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Generators;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace EcommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        private readonly IEmailSender _emailSender;



        public AuthController(AppDbContext context, IAuthService authService, IEmailSender emailSender)
        {
            _context = context;
            _authService = authService;

            _emailSender = emailSender;

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] DtoLogin dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string hashpassword = PasswordHasher.HashPassword(dto.Password);
            var user = await _context.Users.FirstOrDefaultAsync(e =>
                e.Email == dto.Email &&
                e.Password == hashpassword);
            if (user == null)
            {
                return Unauthorized("Email or Password is incorrect");
            }
            var accessToken = await _authService.GetTokenAsync(user);
            var refreshToken = Guid.NewGuid().ToString();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();
            var result = new
            {
                user = new
                {
                    Id = user.Id,
                    IsAdmin = user.IsAdmin,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    Phone = user.Phone,

                },
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(result);



        }
        [HttpPost("Refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] TokenRequestDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == dto.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newAccessToken = await _authService.GetTokenAsync(user);
            var newRefreshToken = Guid.NewGuid().ToString();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] DtoSignUp dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email is already registered.");

            if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
                return BadRequest("Username is already registered.");

            if (await _context.Users.AnyAsync(u => u.Phone == dto.Phone))
                return BadRequest("Phone number is already registered.");

            var user = new User
            {
                Email = dto.Email,
                Password = PasswordHasher.HashPassword(dto.Password),
                UserName = dto.UserName,
                FullName = dto.FullName,
                Phone = dto.Phone
            };

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return Ok(new { message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Database error",
                    details = ex.InnerException?.Message ?? ex.Message
                });
            }

        }
      

        [HttpPost("SendOtp")]
        public async Task<IActionResult> FSendOtp([FromBody] SendOtpRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                return BadRequest("Email is required.");
            }

            var Finduser = _context.Users.FirstOrDefault(e => e.Email == dto.Email);

            var Otp = RandomNumberGenerator.GetInt32(1000, 9999).ToString();

            if (Finduser == null)
            {
                return BadRequest("Invalid email or request.");
            }



            try
            {
                OtpCode otp = new OtpCode
                {
                    Email = dto.Email,
                    Code = Otp,
                    ExpirationTime = DateTime.UtcNow.AddMinutes(5)

                };
                var existingOtp = _context.OtpCodes.FirstOrDefault(e => e.Email == dto.Email);
                if (existingOtp == null)
                {
                    _context.OtpCodes.Add(otp);
                }
                else
                {
                    existingOtp.Code = otp.Code;
                    existingOtp.ExpirationTime = DateTime.UtcNow.AddMinutes(5);
                }
                await _context.SaveChangesAsync();

                await _emailSender.SendEmailAsync(
               dto.Email,
               "Your OTP Code",
               $"Your verification code is: {Otp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("StackTrace: " + ex.StackTrace);

                return StatusCode(500, new
                {
                    success = false,
                    message = "حدث خطأ داخلي في الخادم",
                    details = ex.Message,
                    stack = ex.StackTrace
                });
            }


            return Ok(new { success = true, message = "OTP sent to email." });
        }
        [HttpPost("VerifyOtp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VertifyOtpRequest dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var otpEntry = await _context.OtpCodes
                .FirstOrDefaultAsync(o => o.Email == dto.Email && o.Code == dto.Code);

            if (otpEntry == null)
            {
                return BadRequest(new { success = false, message = "Invalid OTP code." });
            }

            if (otpEntry.ExpirationTime < DateTime.UtcNow)
            {
                return BadRequest(new { success = false, message = "OTP code has expired." });
            }


            _context.OtpCodes.Remove(otpEntry);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "OTP verified successfully." });
        }


        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (request.NewPassword != request.ConfirmPassword)
                    return BadRequest("Passwords do not match.");


                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null)
                    return NotFound("User not found.");

                // تحديث كلمة المرور بعد التحقق
                user.Password = PasswordHasher.HashPassword(request.NewPassword);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(new { success = true, message = "Password updated successfully." });

        }
        [Authorize]
        [HttpPost("UpdateImageUrl")]
        public async Task<IActionResult> UpdateImageUrl([FromBody] DtoImage dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
                return Unauthorized("Invalid user");
            if (string.IsNullOrEmpty(dto.ImageUrl))
            { return NotFound("Not Found Image"); }
            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == Id);
            user.UserImage = dto.ImageUrl;
            await _context.SaveChangesAsync();
            return Ok("Image Update Successfully");



        }
       
        [HttpPost("UploadUserImage")]
        // إذا بدك فقط المستخدمين المسجلين يرفعوا صور
        public async Task<IActionResult> UploadUserImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("لم يتم اختيار ملف");
            }

            // الامتدادات المسموحة
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest("صيغة الملف غير مسموحة");
            }

            // اسم فريد للملف (لتجنب التكرار)
            var fileName = Guid.NewGuid().ToString() + extension;
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);

            // حفظ الملف
            using (var stream = new FileStream(uploadPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // رابط الوصول للصورة
            var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(new { Message = "تم رفع الصورة بنجاح", Url = imageUrl });
        }
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] DtoChangeProfile dto)
        {
            // استخراج الـ Id من الـ JWT claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int Id))
                return Unauthorized("Invalid user");

            var user = await _context.Users.FirstOrDefaultAsync(e => e.Id == Id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            // تحديث الاسم الكامل
            if (!string.IsNullOrWhiteSpace(dto.FullName) && dto.FullName != user.FullName)
            {
                user.FullName = dto.FullName;
            }

            // تحديث اسم المستخدم
            if (!string.IsNullOrWhiteSpace(dto.UserName) && dto.UserName != user.UserName)
            {
                var existing = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

                if (existing != null && existing.Id != user.Id)
                    return BadRequest(new { field = "UserName", message = "Username already taken." });

                user.UserName = dto.UserName;
            }

            // تحديث الايميل
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (existingEmail != null && existingEmail.Id != user.Id)
                    return BadRequest(new { field = "Email", message = "Email already in use." });

                user.Email = dto.Email;
            }

            // تحديث رقم الهاتف
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && dto.PhoneNumber != user.Phone)
            {
                var existingPhone = await _context.Users
                    .FirstOrDefaultAsync(u => u.Phone == dto.PhoneNumber);

                if (existingPhone != null && existingPhone.Id != user.Id)
                    return BadRequest(new { field = "PhoneNumber", message = "PhoneNumber already in use." });

                user.Phone = dto.PhoneNumber;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }
    }
}




    





