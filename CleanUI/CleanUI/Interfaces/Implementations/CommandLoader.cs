using System;
using System.Collections.Generic;
using System.IO;
using CleanUI.Settings;

namespace CleanUI.Interfaces.Implementations
{
    public class CommandLoader : ICommandLoader
    {
        public IEnumerable<Command> GetCommands(UiSettings settings)
        {
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
                    var toAdd = new Command()
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

            foreach (var cmd in settings.Commands)
            {
                yield return cmd;
            }
        }
    }
}