using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlinePaymentSystem.Models;
using OnlinePaymentSystem.Services;
using OnlinePaymentSystem.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlinePaymentSystem.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
    {
        try
        {
            var user = await _authService.Register(dto);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginDto dto)
    {
        try
        {
            var token = await _authService.Login(dto);
            return Ok(new { token });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var user = await _authService.GetCurrentUser(userId);
        return user == null ? NotFound() : Ok(user);
    }
}