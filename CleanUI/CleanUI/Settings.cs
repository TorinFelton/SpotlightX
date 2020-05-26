using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanUI
{
    class Settings
    {
        public List<Command> Commands { get; set; }
    }

    class Command
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public List<Dictionary<String, String>> Actions { get; set; }
    }


}
