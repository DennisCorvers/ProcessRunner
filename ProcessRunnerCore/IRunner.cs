using System;
using System.Threading.Tasks;

namespace ProcessRunner
{
    public interface IRunner : IDisposable
    {
        Task<bool> Start();

        Task Stop();

        Task Restart();
    }
}
