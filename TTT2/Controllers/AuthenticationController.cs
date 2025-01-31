using Microsoft.AspNetCore.Mvc;
using Shared.Extensions.Controllers;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.Authentication;
using System.Net;
using Microsoft.AspNetCore.Authorization;

namespace TTT2.Controllers
{
    [ApiController]
    [Route("authentication")]
    public class AuthenticationController(IAuthenticationService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registrationDTO)
        {
            var result = await authService.RegisterUserAsync(registrationDTO);
            return this.ToActionResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await authService.LoginUserAsync(loginDTO);
            return this.ToActionResult(result);
        }
        
        [Authorize]
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDTO)
        {
            var result = await authService.UpdateUserAsync(updateUserDTO, User);
            return this.ToActionResult(result);
        }
    }
}