using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp7
{
    public class MyDBContext : DbContext
    {
        public MyDBContext(): base("Context")
        {

        }

        public virtual DbSet<Wiadomosc> Wiadomosci { get; set; }
    }
}
