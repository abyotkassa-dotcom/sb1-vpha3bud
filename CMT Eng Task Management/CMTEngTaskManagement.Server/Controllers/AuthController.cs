using Microsoft.AspNetCore.Mvc;
using CMTEngTaskManagement.Server.Services;
using CMTEngTaskManagement.Shared.DTOs;

namespace CMTEngTaskManagement.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                
                if (!response.Success)
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for user {Username}", request.Username);
                return StatusCode(500, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login."
                });
            }
        }

        [HttpPost("logout")]
        public ActionResult Logout()
        {
            // In a stateless JWT setup, logout is handled client-side
            // You could implement token blacklisting here if needed
            return Ok(new { Success = true, Message = "Logged out successfully" });
        }
    }
}