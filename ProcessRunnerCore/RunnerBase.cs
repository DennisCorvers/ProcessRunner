using ProcessRunner.Diagnostics;
using ProcessRunnerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProcessRunner
{
    public class RunnerBase : IRunner
    {
        private object m_syncroot = new object();
        private RunnerStatus m_status;
        private StreamWriter m_inputStream;
        private DateTime m_startTime;
        private bool m_disposed = false;

        private Process m_process;
        private ProcessStartInfo m_startInfo;

        protected bool HasExited
        {
            get
            {
                try
                {
                    return m_process.HasExited;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }
        protected Logger Logger
        { get; }

        public DateTime StartTime
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
        public DateTime NextRestartTime
        { get; private set; }

        /// <summary>
        /// Defines a user-specified name that represents the current <see cref="RunnerBase"/>.
        /// </summary>
        public string TargetName
            => RunnerInfo.TargetName;

        public event Action OnRunnerStarted;
        public event Action OnRunnerStopped;
        public event Action OnRunnerExited;
        public event Action<string> OnMessageReceived;

        public RunnerInfo RunnerInfo
        { get; }

        public RunnerBase(RunnerInfo info)
            : this(info, new Logger())
        { }

        public RunnerBase(RunnerInfo info, Logger logger)
        {
            m_status = RunnerStatus.Stopped;
            RunnerInfo = info ?? throw new ArgumentNullException(nameof(info));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            m_startInfo = new ProcessStartInfo(info.ProcessName)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            m_process = new Process()
            {
                StartInfo = m_startInfo,
                EnableRaisingEvents = true
            };

            m_process.OutputDataReceived += (obj, e) =>
            {
                OnMessageReceived?.Invoke(e.Data);
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

        public async Task Restart()
        {
            ThrowOnDisposed();

            Logger.Info($"Restarting process: \"{TargetName}\".");
            await Stop();

            await Start();
        }

        public async Task<bool> Start()
        {
            ThrowOnDisposed();

            lock (m_syncroot)
            {
                if (m_status != RunnerStatus.Stopped)
                    return false;

                m_status = RunnerStatus.Starting;
            }

            if (!HasExited)
                return false;

            await OnPreStart();
            var res = await Task.Run(() =>
            {
                if (!m_process.Start())
                    return false;

                m_process.Exited -= OnProcessExit;
                m_process.Exited += OnProcessExit;
                StartTime = DateTime.Now;

                m_process.BeginOutputReadLine();

                m_inputStream = m_process.StandardInput;

                return true;
            });

            if (res)
            {
                Logger.Info($"Process \"{TargetName}\" has been started.");

                lock (m_syncroot)
                {
                    m_status = RunnerStatus.Running;
                }

                await OnPostStart();
                OnRunnerStarted?.Invoke();

                return true;
            }

            return false;
        }

        public async Task Stop()
        {
            ThrowOnDisposed();

            lock (m_syncroot)
            {
                if (m_status != RunnerStatus.Running)
                    return;

                m_status = RunnerStatus.Stopping;
            }

            if (HasExited)
                return;

            // Remove OnExit event to prevent auto-restarting.
            m_process.Exited -= OnProcessExit;

            await OnPreStop();

            m_process.CancelOutputRead();

            if (await Task.Run(() => m_process.WaitForExit(RunnerInfo.WaitForExitTime)))
            {
                Logger.Info($"Process \"{TargetName}\" gracefully stopped.");
            }
            else
            {
                m_process.Kill();
                Logger.Warning($"Process \"{TargetName}\" forcefully stopped.");
            }

            lock (m_syncroot)
            {
                m_status = RunnerStatus.Stopped;
            }

            await OnPostStop();
            OnRunnerStopped?.Invoke();
        }

        public async Task SendMessageAsync(string message)
        {
            ThrowOnDisposed();

            lock (m_syncroot)
            {
                if (m_status != RunnerStatus.Running)
                    return;
            }

            if (!HasExited)
            {
                await m_inputStream.WriteLineAsync(message);
                await m_inputStream.FlushAsync();
            }
        }

        public void SendMessage(string message)
        {
            ThrowOnDisposed();

            lock (m_syncroot)
            {
                if (m_status != RunnerStatus.Running)
                    return;
            }

            if (!HasExited)
            {
                m_inputStream.WriteLine(message);
                m_inputStream.Flush();
            }
        }

#pragma warning disable CS1998
        /// <summary>
        /// Occurs just before starting the process.
        /// </summary>
        protected virtual async Task OnPreStart() { }
        /// <summary>
        /// Occurs just after starting the process.
        /// </summary>
        protected virtual async Task OnPostStart() { }

        /// <summary>
        /// Occurs just before stopping.
        /// </summary>
        protected virtual async Task OnPreStop()
        {
            // Gracefully closes the process provided it reads from the input stream.
            m_inputStream.Close();
        }
        /// <summary>
        /// Occurs just after stopping.
        /// </summary>
        protected virtual async Task OnPostStop()
        { }
#pragma warning restore CS1998

        protected virtual void OnProcessExit(object sender, EventArgs e)
        {
            //            m_process.CancelOutputRead();
            //            OnRunnerExited?.Invoke();

            //            if (RunnerInfo.RestartAfterUnexpectedShutdown)
            //            {
            //                Logger.Warning($"Unexpected shutdown of process: \"{TargetName}\".");
            //                Logger.Info($"Attempting to restart \"{TargetName}\"");
            //#pragma warning disable
            //                Start();
            //#pragma warning restore
            //            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                m_process.Dispose();

                m_disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ThrowOnDisposed()
        {
            if (m_disposed)
                throw new ObjectDisposedException(nameof(RunnerBase));
        }

        private enum RunnerStatus
        {
            Starting,
            Running,
            Stopping,
            Stopped
        }
    }
}
