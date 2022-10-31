#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
#else
using Teigha.DatabaseServices;
#endif

using System.Collections.Generic;

namespace KojtoCAD.LayoutCommands
{
    public  class LayoutForSplitting
    {
        public LayoutForSplitting(string layoutName, ObjectId objectId, ICollection<ViewportInModelSpace> viewports)
        {
            LayoutName = layoutName;
            ObjectId = objectId;
            Viewports = viewports;
        }

        public string LayoutName { get; set; }
        public ObjectId ObjectId { get; set; }
        public ICollection<ViewportInModelSpace> Viewports { get; }
    }
}
