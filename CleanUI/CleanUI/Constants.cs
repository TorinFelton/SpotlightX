using System;
using System.IO;
using System.Reflection;

namespace CleanUI
{
    static class Constants
    {
        public static readonly string UserConfigPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\AppData\Roaming\Microsoft\Windows\Start Menu\SpotlightXConfig\config\";

        public static string DefaultConfigPath
        {
            get
            {
                string startupPath = Assembly.GetExecutingAssembly().Location;
                string currentDirectory = Path.GetDirectoryName(startupPath);

                return Path.Combine(currentDirectory, "Config");
            }
        }
    }
}
