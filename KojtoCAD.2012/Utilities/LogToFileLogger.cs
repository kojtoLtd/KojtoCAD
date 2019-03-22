using KojtoCAD.Utilities.Interfaces;
using System.IO;
using System;
#if !bcad
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
#else
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#endif

namespace KojtoCAD.Utilities
{
    public class LogToFileLogger : IWebTracker
    {
        public void TrackCommandUsage(object sender, CommandEventArgs args)
        {
        }

        public void TrackException(System.Exception exception)
        {
            using (var sw = new StreamWriter($"{DateTime.Now:HH.mm.ss.fff}_log.txt"))
            {
                sw.WriteLine(exception.Message);
                sw.WriteLine(exception.StackTrace);
                sw.WriteLine(Environment.StackTrace);
                sw.WriteLine(); 
            }
        }
    }
}
