using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication;
using MMPD.Data.Context;
using MMPD.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MMPD.Web.Services
{
    public interface IAuthService
    {
        Task<AuthenticateResult> LoginAsync(string username, string password);
        Task LogoutAsync(HttpContext context);
        Task<UserAccount> GetUserAsync(string username);
        Task<bool> IsInRoleAsync(UserAccount user, string role);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public UserAccount? User { get; set; }

    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public AuthService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<AuthenticateResult> LoginAsync(string username, string password)
        {
            var user = await _context.UserAccounts.Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Username == username && u.Active == true);
            if (user == null)
            {
                return AuthenticateResult.Fail("Invalid username or password.");
            }
            if (user.Password != password) // In production, use hashed passwords
            {
                return AuthenticateResult.Fail("Invalid username or password.");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.UserRole?.Role ?? "Viewer")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
            }

            return AuthenticateResult.Success(new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme));
        }


        public async Task LogoutAsync(HttpContext context)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        public async Task<UserAccount> GetUserAsync(string username)
        {
            return await _context.UserAccounts.Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Username == username);
        }
        public async Task<bool> IsInRoleAsync(UserAccount user, string role)
        {
            var appUser = await GetUserAsync(user.Username);
            return appUser?.UserRole?.Role == role;
        }
    }
}
