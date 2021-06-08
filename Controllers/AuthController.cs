using System.Threading.Tasks;
using aspnetIdentity.Authentication;
using aspnetIdentity.Helpers.Enums;
using aspnetIdentity.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace aspnetIdentity.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthController(UserManager<ApplicationUser> userManager,SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email,
                model.Password, false, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                return new OkObjectResult("Login realizado com sucesso.");
            }

            return new BadRequestObjectResult("Não foi possível realizar o login.");
        }
        
    }
}