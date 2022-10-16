using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MystatAPI.Entity
{
    public class UserLoginData
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public UserLoginData(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public UserLoginData()
        {

        }
    }
}
