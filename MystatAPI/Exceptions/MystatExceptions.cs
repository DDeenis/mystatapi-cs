using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MystatAPI.Entity;

namespace MystatAPI.Exceptions
{
    public class MystatAuthException : Exception
    {
        public string Field { get; set; }

        public MystatAuthException(string message, string field) : base(message)
        {
            Field = field;
        }

        public MystatAuthException(MystatAuthError authError) : this(authError.Message, authError.Field) { }
    }

    public class MystatException : Exception { }
}
