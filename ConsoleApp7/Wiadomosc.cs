using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp7
{
    public class Wiadomosc
    {
        public int WiadomoscID { get; set; }
        public string MessageID { get; set; }
        public string Nadawca { get; set; }
        public string MailNadawcy { get; set; }
        public string Temat { get; set; }
        public string TrescWiadomoscTempiTekst { get; set; }
        public string TrescWiadomoscTempiHTML { get; set; }
    }
}
