using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    internal class Wiadomosc
    {

        public Wiadomosc(string nadawca, string mailnadawcy, string temat, string tresctekst, string treschtml)
        {
            Nadawca = nadawca;
            MailNadawcy = mailnadawcy;
            Temat = temat;
            TrescWiadomosciTekst = tresctekst;
            TrescWiadomosciHTML = treschtml;
        }

        public string Nadawca { get; set; }
        public string MailNadawcy { get; set; }
        public string Temat { get; set; }
        public string TrescWiadomosciTekst { get; set; }
        public string TrescWiadomosciHTML { get; set; }
    }
}