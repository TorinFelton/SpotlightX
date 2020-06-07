using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanUI
{
    class Settings
    {
        public string TextCol { get; set; }
        public string Opacity { get; set; }
        public List<String> Gradient1 { get; set; }
        public List<String> Gradient2 { get; set; }
        public List<Command> Commands { get; set; }
        public List<String> AppFolders { get; set; }
    }

    class Command
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public List<Dictionary<String, String>> Actions { get; set; }
    }


}
