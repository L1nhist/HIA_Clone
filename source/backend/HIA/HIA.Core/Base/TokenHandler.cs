using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HIA.Core.Models.Base;
using HIA.Core.Models.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HIA.Core.Base;

public class TokenHandler
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TokenHandler(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public TokenResponseModel BuildToken(TokenRequestModel request)
    {
        var claimList = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(request.Id))
            claimList.Add(new Claim(ClaimTypes.Sid, request.Id));
        if (!string.IsNullOrWhiteSpace(request.Email))
            claimList.Add(new Claim(ClaimTypes.NameIdentifier, request.Email));
        if (!string.IsNullOrWhiteSpace(request.RoleId))
            claimList.Add(new Claim(ClaimTypes.Role, request.RoleId));
        if (!string.IsNullOrWhiteSpace(request.RoleName))
            claimList.Add(new Claim(ClaimKey.ROLE_NAME, request.RoleName));
        if (!string.IsNullOrWhiteSpace(request.FullName))
            claimList.Add(new Claim(ClaimKey.FULL_NAME, request.FullName));
        if (!string.IsNullOrWhiteSpace(request.HospitalId))
            claimList.Add(new Claim(ClaimKey.HOSPITAL_ID, request.HospitalId));
        if (!string.IsNullOrWhiteSpace(request.HospitalName))
            claimList.Add(new Claim(ClaimKey.HOSPITAL_NAME, request.HospitalName));
        if (!string.IsNullOrWhiteSpace(request.MA_CSKCB))
            claimList.Add(new Claim(ClaimKey.MA_CSKCB, request.MA_CSKCB));
        if (!string.IsNullOrWhiteSpace(request.Type))
            claimList.Add(new Claim(ClaimKey.TYPE, request.Type));

        claimList.Add(new Claim(ClaimKey.REFRESH, ClaimKey.REFRESH_CODE));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Issuer"];
        var expireTime = int.Parse(_configuration["Jwt:ExpireTime"]!);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claimList,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expireTime),
            signingCredentials: creds
        );

        var output = new TokenResponseModel
        {
            ClaimList = claimList,
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpireAt = token.ValidTo
        };
        return output;
    }

    public async Task<TokenResponseModel> CreateAndSignin(TokenRequestModel input)
    {
        var expireTime = int.Parse(_configuration["Jwt:ExpireTime"]!);

        var token = BuildToken(input);

        var userIdentity = new ClaimsIdentity(token.ClaimList, "signin");
        ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(userIdentity);

        var authenticationProperties = new AuthenticationProperties
        {
            AllowRefresh = true,
            ExpiresUtc = DateTime.UtcNow.AddMinutes(expireTime),
            IsPersistent = true,
        };

        await _httpContextAccessor.HttpContext!.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authenticationProperties);

        var output = new TokenResponseModel
        {
            Token = token.Token,
            ExpireAt = token.ExpireAt,
        };
        return output;
    }

    public ResponseModel<TokenRequestModel> IsTokenValid(string token)
    {
        var res = new ResponseModel<TokenRequestModel>(System.Net.HttpStatusCode.OK, "", null);
        var mySecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var jwtToken = tokenHandler.ValidateToken(token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Issuer"],
                    IssuerSigningKey = mySecurityKey,
                }, out SecurityToken validatedToken);
            var id = jwtToken.FindFirst(ClaimTypes.Sid)?.Value;
            var type = jwtToken.FindFirst(ClaimKey.TYPE)?.Value;
            var MA_CSKCB = jwtToken.FindFirst(ClaimKey.MA_CSKCB)?.Value;
            var hospitalId = jwtToken.FindFirst(ClaimKey.HOSPITAL_ID)?.Value;
            var tenBv = jwtToken.FindFirst(ClaimKey.HOSPITAL_NAME)?.Value;

            var email = jwtToken.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = jwtToken.FindFirst(ClaimTypes.Email)?.Value;
            }

            var name = jwtToken.FindFirst(ClaimKey.FULL_NAME)?.Value;

            //Linhist added 20/06/23
            var roleId = jwtToken.FindFirst(ClaimTypes.Role)?.Value;
            var roleName = jwtToken.FindFirst(ClaimKey.ROLE_NAME)?.Value;

            var refresh = jwtToken.FindFirst(ClaimKey.REFRESH);
            if (!ClaimKey.REFRESH_CODE.Equals(refresh?.Value))
            {
                res.Code = System.Net.HttpStatusCode.NotFound;
                return res;
            }

            //Linhist added 13/01/26
            if (email == "donhuquynh2503@gmail.com")
            {
                res.Code = System.Net.HttpStatusCode.NotFound;
                return res;
            }

            //Linhist added 30/03/26
            if (validatedToken.ValidTo < DateTime.UtcNow)
            {
                res.Code = System.Net.HttpStatusCode.Forbidden;
                return res;
            }

            res.Data = new TokenRequestModel
            {
                Id = id,
                Type = type,
                HospitalId = hospitalId,
                HospitalName = tenBv,
                MA_CSKCB = MA_CSKCB,
                Email = email,
                FullName = name,
                //Linhist added 20/06/23
                RoleId = roleId,
                RoleName = roleName,
                ExpireAt = validatedToken.ValidTo,
            };
        }
        catch
        {
            res.Code = System.Net.HttpStatusCode.NotFound;
        }
        return res;
    }

    public async Task Signout()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync();
    }
}
