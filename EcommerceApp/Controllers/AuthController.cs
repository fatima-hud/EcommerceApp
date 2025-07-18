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

namespace EcommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
   
        private readonly IEmailSender _emailSender;

        

        public AuthController(AppDbContext context, IAuthService authService,IEmailSender emailSender)
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
            var Finduser = await _context.Users.FirstOrDefaultAsync(e => e.Email == dto.Email);
          
            if (Finduser != null)
                return BadRequest("Email is already registered.");


            var user = new User
            {
                Email = dto.Email,
                Password = PasswordHasher.HashPassword(dto.Password),
                UserName = dto.UserName,
                FullName = dto.FullName,
                Phone = dto.Phone


            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "User registered successfully." });
        }

     
        [HttpPost("SendOtp")]
        public async Task<IActionResult> FSendOtp([FromBody] SendOtpRequest dto)
        { 
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var Finduser = _context.Users.FirstOrDefault(e => e.Email == dto.Email);

            var Otp = RandomNumberGenerator.GetInt32(1000,9999).ToString();
           
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
                var existingOtp=_context.OtpCodes.FirstOrDefault(e => e.Email==dto.Email);
                if (existingOtp == null)
                {
                    _context.OtpCodes.Add(otp);
                }
                else
                {
                    existingOtp.Code = otp.Code;
                    existingOtp.ExpirationTime= DateTime.UtcNow.AddMinutes(5);
                }
                    await _context.SaveChangesAsync();

                await _emailSender.SendEmailAsync(
               dto.Email,
               "Your OTP Code",
               $"Your verification code is: {Otp}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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

        [Authorize]
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
        


    }


}
    


