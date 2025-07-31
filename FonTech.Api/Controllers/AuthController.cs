using Microsoft.AspNetCore.Mvc;
using FonTech.Domain.Dto.User;
using FonTech.Domain.Result;
using FonTech.Domain.Dto;
using FonTech.Domain.Interfaces.Services;
using FonTech.Application.Services;

namespace FonTech.Api.Controllers
{
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        /// <summary>
        /// Регистрация пользователя
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("refister")]
        public async Task<ActionResult<BaseResult>> Register([FromBody] RegisterUserDto dto)
        {
            var response = await _authService.Register(dto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        /// <summary>
        /// Логин пользователя
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<BaseResult<TokenDto>>> Login([FromBody] LoginUserDto dto)
        {
            var response = await _authService.Login(dto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

    }
}
