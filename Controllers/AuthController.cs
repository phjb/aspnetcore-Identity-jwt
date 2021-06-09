using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using aspnetIdentity.Authentication;
using aspnetIdentity.Extensions;
using aspnetIdentity.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace aspnetIdentity.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JWTSettings _jwtSettings;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IOptions<JWTSettings> jwtSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("createUser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateUser([FromBody] UserViewModel model)
        {
            if (await _userManager.FindByNameAsync(model.UserName) == null)
            {
                var user = new ApplicationUser {UserName = model.UserName, Email = model.Email,};
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _userManager.AddToRoleAsync(user, model.Role.ToString()).Wait();
                    return StatusCode(StatusCodes.Status201Created);
                }

                return StatusCode(StatusCodes.Status400BadRequest);
            }

            return new BadRequestObjectResult(
                "Não foi possível inserir esse usuário. Cadastro já existe na base de dados.");
        }
        
        [HttpGet("findUserByUserName")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FindUserByUserName([FromQuery] string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                return new OkObjectResult(user);
            }

            return new BadRequestObjectResult("Usuário não encontrado.");
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            // {
            //     "email": "paulo@paulo.com",
            //     "password": "Admin@1"
            // }
            
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))  
            {
                return new OkObjectResult(await GenerateToken(user));
            }

            return new BadRequestObjectResult("Não foi possível realizar o login.");
        }

        private async Task<TokenViewModel> GenerateToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.FinalExpiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new TokenViewModel
            {
                Token = tokenHandler.WriteToken(token),
                Expiration = token.ValidTo.AddHours(-3)
            };
        }
    }
}