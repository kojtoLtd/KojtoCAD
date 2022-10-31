using System.Collections.Generic;

namespace KojtoCAD.LayoutCommands
{
    public class SplitFileResult
    {
        public SplitFileResult(string fileName, string[] layouts)
        {
            FileName = fileName;
            Layouts = layouts;
        }
        public string FileName { get; }
        public ICollection<string> Layouts { get; } = new List<string>();
    }
}
