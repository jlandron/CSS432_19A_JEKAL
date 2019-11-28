using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.UI
{
    public class QueuedMessage
    {
        public string playerName;
        public string message;

        public QueuedMessage(string p, string m)
        {
            playerName = p;
            message = m;
        }
    }
}
