using System;
using System.Collections.Generic;

namespace ProcessRunnerCore.Configuration
{
    public sealed class GlobalConfig
    {
        private const string fileName = "global.config";
        private readonly Config m_instance = new Config();

        public IReadOnlyDictionary<string, string> Configurations
            => m_instance.RunnerConfigurations;

        public GlobalConfig()
        {
            AppDataSettings.LoadSettings(ref m_instance, fileName);
        }

        public bool AddConfig(RunnerConfig runnerConfig)
        {
            if (runnerConfig == null)
                throw new ArgumentNullException(nameof(runnerConfig));

            string runnerName = runnerConfig.RunnerName.ToLower();

            if (m_instance.RunnerConfigurations.ContainsKey(runnerName))
                return false;

            m_instance.RunnerConfigurations.Add(runnerName, runnerConfig.ConfigFileName);
            return true;
        }

        public void Save()
        {
            AppDataSettings.SaveSettings(m_instance, fileName);
        }


        private sealed class Config
        {
            public Dictionary<string, string> RunnerConfigurations = new Dictionary<string, string>();
        }
    }
}
