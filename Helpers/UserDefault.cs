using System;
using aspnetIdentity.Authentication;
using aspnetIdentity.Helpers.Enums;
using Microsoft.AspNetCore.Identity;

namespace aspnetIdentity.Helpers
{
    public class UserDefault
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserDefault(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void Initialize()
        {
           
                if (!_roleManager.RoleExistsAsync(Role.ADMINISTRADOR.ToString()).Result)
                {
                    var result = _roleManager.CreateAsync(new IdentityRole(Role.ADMINISTRADOR.ToString())).Result;
                    if (!result.Succeeded) throw new Exception($"Erro durante a criação da role {Role.ADMINISTRADOR}");
                }

                if (!_roleManager.RoleExistsAsync(Role.USUARIO.ToString()).Result)
                {
                    var result = _roleManager.CreateAsync(new IdentityRole(Role.USUARIO.ToString())).Result;
                    if (!result.Succeeded) throw new Exception($"Erro durante a criação da role {Role.ADMINISTRADOR}");
                }

                CreateUser(
                    new ApplicationUser()
                    {
                        UserName = "adm@email.com",
                        Email = "adm@email.com",
                        EmailConfirmed = true
                    }, "P@ulo01", Role.ADMINISTRADOR.ToString());

                CreateUser(
                    new ApplicationUser()
                    {
                        UserName = "user@email.com",
                        Email = "user@email.com",
                        EmailConfirmed = true
                    }, "P@ulo01", Role.USUARIO.ToString());

            
        }


        private void CreateUser(ApplicationUser user, string password, string initialRole = null)
        {
            if(_userManager.FindByNameAsync(user.UserName).Result == null)
            {
                var result = _userManager.CreateAsync(user, password).Result;

                if (result.Succeeded && !string.IsNullOrWhiteSpace(initialRole)) 
                    _userManager.AddToRoleAsync(user, initialRole).Wait();
            }
        }
    }
}