// Project Models
using beltexam.ViewModels;

namespace beltexam.Models
{
    public class Wrapper
    {
        public Login Login { get; set; }
        public User User { get; set; }
        public Wrapper(User user, Login login)
        {
            Login = login;
            User = user;
        }
    }

}