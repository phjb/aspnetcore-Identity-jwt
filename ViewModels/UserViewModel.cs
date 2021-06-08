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
        public Role Role { get; set; }
    }
}