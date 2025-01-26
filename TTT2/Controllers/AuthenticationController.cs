using Microsoft.AspNetCore.Mvc;
using Shared.Extensions.Controllers;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.Authentication;
using System.Net;

namespace TTT2.Controllers
{
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthenticationController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registrationDTO)
        {
            var result = await _authService.RegisterUserAsync(registrationDTO);
            return this.ToActionResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await _authService.LoginUserAsync(loginDTO);
            return this.ToActionResult(result);
        }
    }
}