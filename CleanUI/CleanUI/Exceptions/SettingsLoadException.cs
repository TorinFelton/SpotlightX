using System;

namespace CleanUI.Exceptions
{
    public class SettingsLoadException : ApplicationException
    {
        public SettingsLoadException(Exception e) : base(
            "Couldn't load the config/settings.json file, is it valid JSON? " +
            "Re-download it or fix any JSON formatting errors.",
            e)
        {
        }
    }
}