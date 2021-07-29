using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessRunnerCore
{
    public class RunnerInfo
    {
        private int m_waitForExitTime = 100;

        /// <summary>
        /// The (local)time at which the process should restart. 18 hours = 6 PM.
        /// </summary>
        public TimeSpan RestartInterval
        { get; set; } = new TimeSpan(4, 0, 0);
        /// <summary>
        /// TRUE if the process should automatically restart based on a time interval.
        /// </summary>
        public bool ShouldAutoRestart
        { get; set; } = false;
        /// <summary>
        /// TRUE if the process should automatically restart if it has been stopped outside of the ProcessRunner service.
        /// </summary>
        public bool RestartAfterUnexpectedShutdown
        { get; set; } = true;
        /// <summary>
        /// The name of the process to execute.
        /// </summary>
        public string ProcessName
        { get; private set; }
        /// <summary>
        /// A custom name that represents the process in a user-friendly way.
        /// </summary>
        public string TargetName
        { get; private set; }
        /// <summary>
        /// The amount of time (in ms) to wait for a graceful exit.
        /// </summary>
        public int WaitForExitTime
        {
            set => m_waitForExitTime = value < 100 ? 100 : value;
            get => m_waitForExitTime;
        }

        public RunnerInfo(string processName, string targetName)
        {
            ProcessName = processName;
            TargetName = targetName;
        }
    }
}
