using Microsoft.AspNetCore.Mvc;
using Shared.Interfaces.Services;
using Shared.Models.DTOs.Authentication;
using Microsoft.AspNetCore.Authorization;
using Shared.Interfaces.Controllers;
using Shared.Models.Common;

namespace TTT2.Controllers
{
    [ApiController]
    [Route("authentication")]
    public class AuthenticationController(IAuthenticationService authService, IHttpResponseConverter responseConverter) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registrationDTO)
        {
            var result = await authService.RegisterUserAsync(registrationDTO);
            return responseConverter.Convert(HttpServiceResult<RegisterResponseDTO>.FromServiceResult(result));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var result = await authService.LoginUserAsync(loginDTO);
            return responseConverter.Convert(HttpServiceResult<LoginResponseDTO>.FromServiceResult(result));
        }
        
        [Authorize]
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDTO)
        {
            var result = await authService.UpdateUserAsync(updateUserDTO, User);
            return responseConverter.Convert(HttpServiceResult<UpdateUserResponseDTO>.FromServiceResult(result));
        }
    }
}