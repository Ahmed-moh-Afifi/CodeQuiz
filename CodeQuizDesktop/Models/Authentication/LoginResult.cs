using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models.Authentication
{
    public class LoginResult
    {
        public required User User { get; set; }
        public required TokenModel TokenModel { get; set; }
    }
}
