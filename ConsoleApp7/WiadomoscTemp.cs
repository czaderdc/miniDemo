using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp5
{
    internal class WiadomoscTemp
    {

        public WiadomoscTemp(string nadawca, string mailnadawcy, string temat, string tresctekst, string treschtml)
        {
            Nadawca = nadawca;
            MailNadawcy = mailnadawcy;
            Temat = temat;
            TrescWiadomoscTempiTekst = tresctekst;
            TrescWiadomoscTempiHTML = treschtml;
        }

        public string Nadawca { get; set; }
        public string MailNadawcy { get; set; }
        public string Temat { get; set; }
        public string TrescWiadomoscTempiTekst { get; set; }
        public string TrescWiadomoscTempiHTML { get; set; }
    }
}