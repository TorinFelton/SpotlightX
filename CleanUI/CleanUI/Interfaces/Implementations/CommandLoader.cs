using System;
using System.Collections.Generic;
using System.IO;
using CleanUI.Exceptions;
using CleanUI.Settings;

namespace CleanUI.Interfaces.Implementations
{
    public class CommandLoader : ICommandLoader
    {
        public IEnumerable<Command> GetCommands(UiSettings settings)
        {
            if (settings.AppFolders == null) settings.AppFolders = new List<string>();
            foreach (var path in settings.AppFolders)
            {
                string[] files;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (Exception e)
                {
                    throw new CommandLoadException(e);
                }

                foreach (var file in files)
                {
                    var toAdd = new Command
                    {
                        Name = Path.GetFileNameWithoutExtension(file),
                        Actions = new List<Dictionary<string, string>>
                        {
                            new Dictionary<string, string>
                            {
                                {
                                    "PROCESS", file
                                }
                            }
                        }
                    };
                    yield return toAdd;
                }
            }

            if (settings.Commands == null) settings.Commands = new List<Command>();
            foreach (var cmd in settings.Commands)
            {
                yield return cmd;
            }
        }
    }
}