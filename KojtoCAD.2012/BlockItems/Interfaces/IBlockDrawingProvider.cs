using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KojtoCAD.BlockItems.Interfaces
{
    public interface IBlockDrawingProvider
    {
             string GetBlockFile(string blockName);
    }
}
