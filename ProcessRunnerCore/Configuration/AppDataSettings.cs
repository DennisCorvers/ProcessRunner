using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace ProcessRunnerCore.Configuration
{
    public class AppDataSettings
    {
        private readonly static string directory;
        private const string ProjectName = "ProcessRunner";

        static AppDataSettings()
        {
            var localDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //var projName = Assembly.GetCallingAssembly().GetName().Name;

            directory = $"{localDir}\\{ProjectName}";

            Directory.CreateDirectory(directory);
        }

        public static void SaveSettings<T>(T settings, string filename)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException(nameof(filename));

            using (StreamWriter file = File.CreateText($"{directory}\\{filename}"))
            {
                JsonSerializer serializer = new JsonSerializer();
                try
                {
                    serializer.Serialize(file, settings);
                }
                catch
                { }
            }
        }

        public static bool LoadSettings<T>(ref T settings, string filename)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentException(nameof(filename));

            try
            {
                using (StreamReader reader = File.OpenText($"{directory}\\{filename}"))
                {
                    settings = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
                    return true;
                }
            }
            catch
            {
                // If loading fails, try saving a new settings file to ensure the file exists with the correct format.
                SaveSettings(settings, filename);
                return false;
            }
        }
    }
}
