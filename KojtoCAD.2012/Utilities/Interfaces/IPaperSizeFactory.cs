using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;

namespace KojtoCAD.Utilities.Interfaces
{
    interface IPaperSizeFactory
    {
        PaperSize CreateOrientedPaperSize(string paperSizeName, string paperOrientation);
    }
}
