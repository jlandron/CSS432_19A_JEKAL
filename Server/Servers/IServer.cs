using System.Threading.Tasks;

namespace Jekal.Servers
{
    interface IServer
    {
        Task<int> StartServer();
        void StopServer();
    }
}
