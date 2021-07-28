using System.Threading.Tasks;

namespace ProcessRunnerService.Hubs
{
    public interface IProcessHub
    {
        Task StartProcess(int pid);

        Task RestartProcess(int pid);

        Task StopProcess(int pid);
    }
}
