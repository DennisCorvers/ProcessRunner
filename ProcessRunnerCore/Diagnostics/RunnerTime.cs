using System;

namespace ProcessRunner.Diagnostics
{
    public class RunnerTime
    {
        private DateTime m_startTime;

        public bool ShouldAutoRestart
        { get; set; }
        public TimeSpan RunnerRestartTimeInterval
        { get; set; }
        public DateTime StartTime
        {
            get
            {
                return m_startTime;
            }
            set
            {
                m_startTime = DateTime.Now;
                NextRestartTime = GetNextRestartDate(RunnerRestartTimeInterval);
            }
        }
        public DateTime NextRestartTime
        { get; private set; }

        internal RunnerTime()
        {
            ShouldAutoRestart = false;
        }

        public RunnerTime(TimeSpan restartTime)
        {
            ShouldAutoRestart = true;
            RunnerRestartTimeInterval = restartTime;
            NextRestartTime = GetNextRestartDate(restartTime);
        }

        public bool ShouldRestart()
        {
            if (!ShouldAutoRestart)
                return false;

            return DateTime.Now >= NextRestartTime;
        }

        private static DateTime GetNextRestartDate(TimeSpan restartTime)
        {
            var now = DateTime.Now;
            var nextRestartDate = now.Date.Add(restartTime);

            if (nextRestartDate < now)
                nextRestartDate = nextRestartDate.AddDays(1);

            return nextRestartDate;
        }
    }
}
