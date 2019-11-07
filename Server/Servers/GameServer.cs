using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    internal class GameServer : IServer
    {
        public Task<int> StartServer(CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public void StopServer()
        {
            throw new System.NotImplementedException();
        }
    }
}
