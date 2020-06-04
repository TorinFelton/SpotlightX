using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CleanUI.Interfaces.Implementations
{
    public class StartMenuProgramListLoader : IProgramListLoader
    {
        public IEnumerable<string> GetProgramList()
        {
            string userStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
                                       @"\AppData\Roaming\Microsoft\Windows\Start Menu\Programs"; // Also automatically add all start menu programs
            string startMenuPath = Path.GetPathRoot(Environment.SystemDirectory) +
                                   @"ProgramData\Microsoft\Windows\Start Menu\Programs";

            foreach (var item in Directory.GetFiles(startMenuPath).Where(s => s.EndsWith(".lnk")))
            {
                yield return item;
            }

            foreach (var item in Directory.GetFiles(userStartMenuPath).Where(s => s.EndsWith(".lnk")))
            {
                yield return item;
            }

            foreach (var item in new DirectoryInfo(startMenuPath).GetDirectories()
                .Where(x => (x.Attributes & FileAttributes.Hidden) == 0)
                .SelectMany(d => Directory.GetFiles(d.FullName)
                    .Where(x => x.Split(' ').Length < 5 && x.EndsWith(".lnk"))))
            {
                yield return item;
            }

            foreach (var item in new DirectoryInfo(userStartMenuPath).GetDirectories()
                .Where(x => (x.Attributes & FileAttributes.Hidden) == 0)
                .SelectMany(d => Directory.GetFiles(d.FullName)
                    .Where(x => x.Split(' ').Length < 5 && x.EndsWith(".lnk"))))
            {
                yield return item;
            }
        }
    }
}