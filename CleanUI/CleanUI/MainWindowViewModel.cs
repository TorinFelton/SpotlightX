using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanUI
{
    class MainWindowViewModel
    {
        public string TextCol { get; set; }
        public string Opacity { get; set; }
        public string G1Col { get; set; }
        public string G2Col { get; set; }
        public string G1Offset { get; set; }
        public string G2Offset { get; set; }

        public MainWindowViewModel(Settings FSettings)
        {
            G1Col = FSettings.Gradient1[0];
            G2Col = FSettings.Gradient2[0];
            G1Offset = FSettings.Gradient1[1];
            G2Offset = FSettings.Gradient2[1];
            Opacity = FSettings.Opacity;
            TextCol = FSettings.TextCol;
        }
    }
}
