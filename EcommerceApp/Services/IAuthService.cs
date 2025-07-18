using EcommerceApp.Entities;
using EcommerceApp.Models;

namespace EcommerceApp.Services
{
    public interface IAuthService
    {
        Task<string> GetTokenAsync(User user);
       
    }
}
