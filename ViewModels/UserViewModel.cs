using System;
using System.Collections.Generic;
using aspnetIdentity.Helpers.Enums;

namespace aspnetIdentity.ViewModels
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class UserViewModel : LoginViewModel
    {
        public string UserName { get; set; }
        public IEnumerable<Role> Role { get; set; }
    }

    public class TokenViewModel
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}