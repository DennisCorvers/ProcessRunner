using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProcessRunnerService.Hubs
{
    public class ProcessOverview : Hub<IProcessOverview>
    {
    }
}
