using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

using KojtoCAD.IoC;
using KojtoCAD.Ui.Interfaces;
using KojtoCAD.Updater.Interfaces;
using KojtoCAD.Utilities.Interfaces;

using Exception = System.Exception;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
#else
using Teigha.Runtime;
using Bricscad.ApplicationServices;
#endif

[assembly: ExtensionApplication(typeof(KojtoCAD.KojtoCadInitializer))]

namespace KojtoCAD
{
    public class KojtoCadInitializer : IExtensionApplication
    {
        private IWebTracker logger = null;
        void IExtensionApplication.Initialize()
        {
            ContainerRegistrar.SetupContainer();

            this.logger = ContainerRegistrar.Container.Resolve<IWebTracker>();
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomainOnFirstChanceException;

            Application.DocumentManager.MdiActiveDocument.CommandWillStart += logger.TrackCommandUsage;

            var uiGenerator = ContainerRegistrar.Container.Resolve<IUiGenerator>();

            try
            {
                uiGenerator.GenerateUi(false);
            }
            catch (Exception exception)
            {
                logger.TrackException(exception);
            }

            ContainerRegistrar.Container.Release(uiGenerator);

            IKojtoCadUpdater updater;
            try
            {
                updater = ContainerRegistrar.Container.Resolve<IKojtoCadUpdater>();
                updater.UpdateKojtoCad();
            }
            catch (Exception exception)
            {
                this.logger.TrackException(exception);
            }
        }

        private void CurrentDomainOnFirstChanceException(object sender,
            FirstChanceExceptionEventArgs firstChanceExceptionEventArgs)
        {
            var thisasm = Assembly.GetExecutingAssembly();
            if (thisasm.GetName().Name.ToLower().Contains("kojto"))
            {
                if (this.logger != null)
                {
                    logger.TrackException(firstChanceExceptionEventArgs.Exception);    
                }
                
            }
        }

        void IExtensionApplication.Terminate()
        {
            ContainerRegistrar.DisposeContainer();
        }
    }
}