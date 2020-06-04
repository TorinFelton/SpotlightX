using System.Collections.Generic;
using CleanUI.Settings;

namespace CleanUI.Interfaces
{
    public interface ICommandLoader
    {
        IEnumerable<Command> GetCommands(UiSettings settings);
    }

    public interface IProgramListLoader
    {
        IEnumerable<string> GetProgramList();
    }
}