using ProcessRunner;
using System.Collections.Concurrent;

namespace ProcessRunnerService.Processes
{
    public class ProcessStore : IProcessStore
    {
        private ConcurrentDictionary<string, IRunner> m_hostedProcesses;

        public ProcessStore()
        {
            m_hostedProcesses = new ConcurrentDictionary<string, IRunner>();
        }

        public void Dispose()
        { }
    }
}
