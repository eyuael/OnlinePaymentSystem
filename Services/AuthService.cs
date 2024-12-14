using Microsoft.IdentityModel.Tokens;
using OnlinePaymentSystem.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using OnlinePaymentSystem.Models;
using BCrypt.Net;
using ECommerceAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace OnlinePaymentSystem.Services
{
    public interface IAuthService
    {
        Task<UserDto> Register(RegisterDto dto);
        Task<string> Login(LoginDto dto);
        Task<UserDto> GetCurrentUser(string userId);
    }

    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<UserDto> Register(RegisterDto dto)
        {
            if (await EmailExistsAsync(dto.Email))
                throw new Exception("Email already exists");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Address = dto.Address
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<string> Login(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Invalid credentials");

            var token = GenerateJwtToken(user);
            return token;
        }

        public async Task<UserDto> GetCurrentUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user == null ? null : MapToDto(user);
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserDto MapToDto(User user) =>
            new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address
            };
    }
}
