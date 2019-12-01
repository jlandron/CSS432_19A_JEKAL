using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkGame.UI
{
    public class QueuedMessage
    {
        public string playerName;
        public string message;
        public Type type = Type.NULL;
        public enum Type
        {
            NULL = -1,
            TEAM = 1,
            PRIVATE,
            SYSTEM,
        }

        public QueuedMessage(string p, string m)
        {
            playerName = p;
            message = m;
        }
        public QueuedMessage(string p, string m, Type type)
        {
            playerName = p;
            message = m;
            this.type = type;
        }
    }
}
