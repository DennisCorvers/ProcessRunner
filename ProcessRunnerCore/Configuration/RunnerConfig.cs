namespace ProcessRunnerCore.Configuration
{
    public class RunnerConfig
    {
        public string RunnerName
        { get; private set; }
        public string ConfigFileName
            => RunnerName.ToLower() + ".config";
        public bool IsActive
        { get; private set; }

        private RunnerConfig() { }

        public RunnerConfig(string runnerName)
        {
            RunnerName = runnerName;
        }

        public static bool TryLoad<T>(string fileName, out T runnerConfig)
            where T : RunnerConfig, new()
        {
            runnerConfig = new T();
            return AppDataSettings.LoadSettings(ref runnerConfig, fileName);
        }

        public void Save()
        {
            AppDataSettings.SaveSettings(this, ConfigFileName);
        }
    }
}
