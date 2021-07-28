using ProcessRunner;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessRunnerService.Processes
{
    public class ProcessStore : IProcessStore
    {
        private bool m_isDisposed;
        private ConcurrentDictionary<string, IRunner> m_hostedProcesses;

        public ProcessStore()
        {
            m_hostedProcesses = new ConcurrentDictionary<string, IRunner>();
        }

        ~ProcessStore()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // Cleanup running processes.

                m_isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
