using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using KojtoCAD.Updater.Interfaces;
using KojtoCAD.Utilities.Interfaces;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

using Exception = System.Exception;

#if !bcad
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
#else
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#endif

namespace KojtoCAD.Utilities
{
    public class AiUsageTracker : IWebTracker
    {
        /*
         * In order to enable MS Application Insights telemetry : 
         * 1. Insert the Instrumentation Key below.
         * 2. Switch WebTrackerIsEnabled setting to True
         * 
         * otherwise it will simply not work.
         */
        private const string instrumentationKey = "";
        private readonly IAppConfigurationProvider _configurationProvider;
        private readonly TelemetryClient telemetryClient = new TelemetryClient();

        private readonly List<string> commandMethods = new List<string>();

        public AiUsageTracker(IAppConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;

#if !DEBUG
            TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;

            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                telemetryClient.Context.User.Id = Environment.UserName;
                telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
                telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();

                var methods = Assembly.GetExecutingAssembly().GetTypes()
                          .SelectMany(t => t.GetMethods())
                          .Where(m => m.GetCustomAttributes(typeof(CommandMethodAttribute), false).Length > 0)
                          .ToArray();

                foreach (var method in methods)
                {
                    var cms = method.GetCustomAttributes(typeof(CommandMethodAttribute), true) as CommandMethodAttribute[];
                    commandMethods.AddRange(cms.Select(cm => cm.GlobalName));
                }
            }
#endif
        }
        public void TrackCommandUsage(object sender, CommandEventArgs args)
        {
            if (!this.TelemetryIsEnabled())
                return;

            if (!commandMethods.Contains(args.GlobalCommandName))
                return;

            telemetryClient.TrackEvent(args.GlobalCommandName);
        }

        public void TrackException(Exception exception)
        {
            if (!this.TelemetryIsEnabled())
                return;

            telemetryClient.TrackException(exception);
        }

        private bool TelemetryIsEnabled()
        {
            return _configurationProvider.WebTrackerIsEnabled() &&
                telemetryClient != null &&
                !string.IsNullOrEmpty(telemetryClient.InstrumentationKey) &&
                telemetryClient.IsEnabled();
        }
    }
}