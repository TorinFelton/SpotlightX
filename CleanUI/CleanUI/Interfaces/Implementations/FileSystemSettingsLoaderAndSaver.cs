using System;
using System.IO;
using CleanUI.Exceptions;
using CleanUI.Settings;
using Newtonsoft.Json;

namespace CleanUI.Interfaces.Implementations
{
    public class FileSystemSettingsLoaderAndSaver : ISettingsLoader, ISettingsSaver
    {
        private readonly string _settingsPath;

        public FileSystemSettingsLoaderAndSaver(string settingsPath)
        {
            _settingsPath = settingsPath ?? throw new ArgumentNullException(nameof(settingsPath));
        }

        public UiSettings LoadSettings()
        {
            try
            {
                return File.Exists(_settingsPath) ? JsonConvert.DeserializeObject<UiSettings>(File.ReadAllText(_settingsPath)) : new UiSettings();
            }
            catch (Exception e)
            {
                throw new SettingsLoadException(e);
            }
        }

        public void SaveSettings(UiSettings fSettings)
        {
            File.WriteAllText(_settingsPath,
                JsonConvert.SerializeObject(fSettings, Formatting.Indented)); // Append path to settings.json
        }
    }
}