using ProcessRunner.Diagnostics;
using ProcessRunnerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ProcessRunner
{
    public class RunnerBase : IRunner
    {
        private DateTime m_startTime;
        private Logger m_logger;
        private StreamWriter m_processInput;
        private StreamReader m_processOutput;
        private bool m_disposed = false;

        protected Process RunnerProcess
        { get; }
        protected ProcessStartInfo StartInfo
        { get; }
        protected bool HasExited
        {
            get
            {
                try
                {
                    return RunnerProcess.HasExited;
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
            }
        }

        protected DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = DateTime.Now;

                var now = DateTime.Now;
                var nextRestartDate = now.Date.Add(RunnerInfo.RestartInterval);

                if (nextRestartDate < now)
                    NextRestartTime = nextRestartDate.AddDays(1);
            }
        }
        protected DateTime NextRestartTime
        { get; private set; }

        /// <summary>
        /// Defines a user-specified name that represents the current <see cref="RunnerBase"/>.
        /// </summary>
        public string TargetName
            => RunnerInfo.TargetName;

        public RunnerInfo RunnerInfo
        { get; }

        public RunnerBase(RunnerInfo info)
            : this(info, new Logger())
        { }

        public RunnerBase(RunnerInfo info, Logger logger)
        {
            RunnerInfo = info ?? throw new ArgumentNullException(nameof(info));
            m_logger = logger ?? throw new ArgumentNullException(nameof(logger));

            StartInfo = new ProcessStartInfo(info.ProcessName)
            {
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            RunnerProcess = new Process()
            {
                StartInfo = StartInfo,
                EnableRaisingEvents = true,
            };
        }

        public RunnerBase(string processName, string targetName)
            : this(processName, targetName, new Logger())
        { }

        public RunnerBase(string processName, string targetName, Logger logger)
            : this(new RunnerInfo(processName, targetName), logger)
        { }

        ~RunnerBase()
        {
            Dispose(false);
        }

        public virtual bool Start()
        {
            if (HasExited)
            {
                if (!RunnerProcess.Start())
                {
                    throw new Exception("Process could not be started.");
                }

                RunnerProcess.Exited -= OnProcessExit;
                RunnerProcess.Exited += OnProcessExit;
                StartTime = DateTime.Now;

                m_processInput = RunnerProcess.StandardInput;
                m_processOutput = RunnerProcess.StandardOutput;

                m_logger.Info($"Process \"{TargetName}\" has been started.");

                return true;
            }

            return false;
        }

        public virtual void Stop()
        {
            if (HasExited)
                return;

            // Remove OnExit event to prevent auto-restarting.
            RunnerProcess.Exited -= OnProcessExit;

            OnStop();

            if (RunnerProcess.WaitForExit(RunnerInfo.WaitForExitTime))
            {
                m_logger.Info($"Process \"{TargetName}\" gracefully stopped.");
            }
            else
            {
                RunnerProcess.Kill();
                m_logger.Warning($"Process \"{TargetName}\" forcefully stopped.");
            }
        }

        public virtual void Restart()
        {
            m_logger.Info($"Restarting process: \"{TargetName}\".");
            Stop();

            Start();
        }

        protected virtual void OnStop()
        { }

        protected virtual void OnProcessExit(object sender, EventArgs e)
        {
            if (RunnerInfo.RestartAfterUnexpectedShutdown)
            {
                m_logger.Warning($"Unexpected shutdown of process: \"{TargetName}\".");
                m_logger.Info($"Attempting to restart \"{TargetName}\"");
                Start();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                if (disposing)
                {
                    if (RunnerProcess != null)
                    {
                        m_processInput.Close();
                        m_processInput.Dispose();
                        m_processOutput.Close();
                        m_processOutput.Dispose();
                    }
                }

                if (!HasExited)
                    RunnerProcess.Kill();

                RunnerProcess.Dispose();

                m_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        public Task<bool> StartAsync()
        {
            return Task.Run(() => Start());
        }

        public Task StopAsync()
        {
            return Task.Run(() => Stop());
        }

        public Task RestartAsync()
        {
            return Task.Run(() => Restart());
        }
    }
}
