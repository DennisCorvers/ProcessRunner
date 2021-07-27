using System;
using System.Threading.Tasks;

namespace ProcessRunner
{
    public interface IRunner : IDisposable
    {
        bool Start();
        Task<bool> StartAsync();

        void Stop();
        Task StopAsync();

        void Restart();
        Task RestartAsync();
    }
}
