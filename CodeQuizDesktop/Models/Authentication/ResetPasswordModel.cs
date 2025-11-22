using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models.Authentication
{
    public class ResetPasswordModel
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string NewPassword { get; set; }
    }
}
