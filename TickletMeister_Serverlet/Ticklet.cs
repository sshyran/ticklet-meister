using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TickletMeister_Serverlet
{
    class Ticklet
    {
        private int id;
        private String connectionString;

        public Ticklet(int id, String conString)
        {
            this.id = id;
            this.connectionString = conString;
        }

        public int getID()
        {
            return id;
        }

        public String getConnectionString()
        {
            return connectionString;
        }
    }
}
