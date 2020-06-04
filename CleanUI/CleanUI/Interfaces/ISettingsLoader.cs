using System;
using CleanUI.Settings;

namespace CleanUI.Interfaces
{
    public class CommandLoadException : ApplicationException
    {
        public CommandLoadException(Exception exception):base("Unable to load command",exception)
        {
        }
    }

    public interface ISettingsLoader
    {
        UiSettings LoadSettings();
    }
}