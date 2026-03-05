using Microsoft.AspNetCore.Mvc;
using BookingSystem.Services;
using BookingSystem.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
	private readonly AuthService _authService;

	public AuthController(AuthService authService)
	{
		_authService = authService;
	}

	[HttpPost("login")]
	public IActionResult Login(LoginRequest request)
	{
		var token = _authService.Login(request.Email, request.Password);

		if (token == null)
			return Unauthorized("Invalid email or password");

		return Ok(new { token });
	}

	[HttpPost("register")]
	public IActionResult Register(RegisterRequest request)
	{
		_authService.Register(request);
		return Ok("Register success");
	}
}