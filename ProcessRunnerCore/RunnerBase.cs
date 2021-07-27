using ProcessRunner.Diagnostics;
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
        private Logger m_logger;
        private StreamWriter m_processInput;
        private StreamReader m_processOutput;
        private readonly string m_fileName;
        private bool m_disposed = false;
        private int m_waitForExitTime;

        protected Process RunnerProcess
        { get; private set; }
        protected ProcessStartInfo StartInfo
        { get; private set; }
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

        public RunnerTime RunnerTimings
        { get; set; }
        public bool StopProcessWhenDispose
        { get; set; } = true;
        /// <summary>
        /// Determines if the process should restart when it's stopped without calling <see cref="Stop"/>.
        /// </summary>
        public bool RestartAfterUnexpectedShutdown
        { get; set; } = false;
        /// <summary>
        /// The name of the process that is running.
        /// </summary>
        public string ProcessName
        { get; private set; }
        /// <summary>
        /// The amount of time in ms that <see cref="Stop"/> waits before killing the process.
        /// </summary>
        public int WaitForExitTime
        {
            set => m_waitForExitTime = value < 0 ? throw new ArgumentOutOfRangeException() : value;
            get => m_waitForExitTime;
        }

        public RunnerBase(string fileName)
            : this(fileName, string.Empty)
        { }

        public RunnerBase(string fileName, string processName)
            : this(fileName, processName, new Logger())
        { }

        public RunnerBase(string fileName, string processName, Logger logger)
        {
            m_logger = logger;

            RunnerTimings = new RunnerTime();
            ProcessName = processName;
            m_fileName = fileName;

            StartInfo = new ProcessStartInfo(fileName)
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
                RunnerTimings.StartTime = DateTime.Now;

                m_processInput = RunnerProcess.StandardInput;
                m_processOutput = RunnerProcess.StandardOutput;

                m_logger.Info($"Process \"{ProcessName}\" has been started.");

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

            if (RunnerProcess.WaitForExit(WaitForExitTime))
            {
                m_logger.Info($"Process \"{ProcessName}\" gracefully stopped.");
            }
            else
            {
                RunnerProcess.Kill();
                m_logger.Warning($"Process \"{ProcessName}\" forcefully stopped.");
            }
        }

        public virtual void Restart()
        {
            m_logger.Info($"Restarting process: \"{ProcessName}\".");
            Stop();

            Start();
        }

        protected virtual void OnStop()
        { }

        protected virtual void OnProcessExit(object sender, EventArgs e)
        {
            if (RestartAfterUnexpectedShutdown)
            {
                m_logger.Warning($"Unexpected shutdown of process: \"{ProcessName}\".");
                m_logger.Info($"Attempting to restart \"{ProcessName}\"");
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

                        if (StopProcessWhenDispose)
                            RunnerProcess.Kill();

                        RunnerProcess.Dispose();
                    }
                }

                m_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
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
