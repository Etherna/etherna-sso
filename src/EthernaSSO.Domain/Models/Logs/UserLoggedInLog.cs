using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.SSOServer.Domain.Models.Logs
{
    public class UserLoggedInLog : LogBase
    {
        // Constructors.
        public UserLoggedInLog(User user)
        {
            User = user;
        }
        protected UserLoggedInLog() { }

        // Properties.
        public virtual User User { get; protected set; } = default!;
    }
}
