using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeQuizDesktop.Models
{
    public class User
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public required DateTime JoinDate { get; set; }
        public string? ProfilePicture { get; set; }
        public string FullName
        {
            get { return $"{FirstName} {LastName}"; }
        }
        public char FirstCharInName
        {
            get { return FirstName[0]; }
        }

    }
}
