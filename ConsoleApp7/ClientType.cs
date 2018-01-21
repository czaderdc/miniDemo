using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp7
{
    public static class ClientType
    {
        public static string POP3 { get; private set; } = "pop3".ToUpper();
        public static string IMAP { get; private set; } = "imap".ToUpper();
    }
}
