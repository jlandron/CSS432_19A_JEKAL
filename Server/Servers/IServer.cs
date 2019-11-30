using System.Threading;
using System.Threading.Tasks;

namespace Jekal.Servers
{
    interface IServer
    {
        Task<int> StartServer(CancellationToken token);
    }
}
