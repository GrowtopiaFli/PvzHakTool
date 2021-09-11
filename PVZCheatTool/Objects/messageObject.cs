using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvzHakTool.Objects
{
    class messageObject
    {
        public string id, message;

        public messageObject(string id, string message)
        {
            this.id = id;
            this.message = message;
        }
    }
}
