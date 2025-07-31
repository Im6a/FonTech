using FonTech.Domain.Dto;
using FonTech.Domain.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace FonTech.Domain.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefrashToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);

        Task<BaseResult<TokenDto>> RefreshToken(TokenDto dto);

    }
}
