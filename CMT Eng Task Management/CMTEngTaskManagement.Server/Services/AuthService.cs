using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CMTEngTaskManagement.Server.Data;
using CMTEngTaskManagement.Shared.DTOs;
using CMTEngTaskManagement.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace CMTEngTaskManagement.Server.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<UserDto?> GetUserByIdAsync(int userId);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
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

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Shop)
                    .FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null || !VerifyPassword(request.Password, user.Password))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password."
                    };
                }

                if (user.Status != UserStatus.Active)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Account is inactive or suspended."
                    };
                }

                var token = GenerateJwtToken(user);
                var userDto = MapUserToDto(user);

                return new LoginResponse
                {
                    Success = true,
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login."
                };
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Shop)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            return user != null ? MapUserToDto(user) : null;
        }

        public string HashPassword(string password)
        {
            // Using SHA256 to match your PHP implementation
            // In production, consider using BCrypt or Argon2
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToHexString(bytes).ToLower();
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (user.ShopId.HasValue)
            {
                claims.Add(new Claim("ShopId", user.ShopId.Value.ToString()));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = jwtIssuer,
                Audience = jwtIssuer,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private UserDto MapUserToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                ProfilePicturePath = user.ProfilePicturePath,
                ShopId = user.ShopId,
                ShopName = user.Shop?.Name
            };
        }
    }
}