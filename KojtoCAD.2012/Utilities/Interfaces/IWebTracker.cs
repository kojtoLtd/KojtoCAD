using System;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
#else
using Bricscad.ApplicationServices;
#endif

namespace KojtoCAD.Utilities.Interfaces
{
    interface IWebTracker
    {
        void TrackCommandUsage(object sender, CommandEventArgs args);

        void TrackException(Exception exception);
    }
}
