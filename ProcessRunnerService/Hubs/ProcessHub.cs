using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ProcessRunner;
using ProcessRunnerService.Processes;

namespace ProcessRunnerService.Hubs
{
    public class ProcessHub : Hub<IProcessHub>
    {
        private readonly IProcessStore m_processStore;

        public ProcessHub(IProcessStore processStore)
        {
            m_processStore = processStore;
        }

        public override Task OnConnectedAsync()
        {
            // Send information about existing processes.
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        // https://developer.okta.com/blog/2019/11/21/csharp-websockets-tutorial
    }
}
