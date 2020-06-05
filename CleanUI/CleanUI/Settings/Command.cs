using System.Collections.Generic;

namespace CleanUI.Settings
{
    public class Command
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public List<Dictionary<string, string>> Actions { get; set; }
    }
}