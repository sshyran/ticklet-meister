using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TickletMeister_Viewportletlet
{
    class Ticklet
    {
        private String cs;
        private String cID;
        private int idNum;

        public Ticklet(String connectionString, int id, String clientIDName)
        {
            cs = connectionString;
            idNum = id;
            cID = clientIDName;
        }

        public String getConnectionString()
        {
            return cs;
        }

        public int getID()
        {
            return idNum;
        }

        public String getClientID()
        {
            return cID;
        }
    }
}
