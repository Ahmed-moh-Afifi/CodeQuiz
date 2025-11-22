using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models.Authentication
{
    public class LoginModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
