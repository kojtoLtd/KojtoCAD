using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using System.Collections;
using System.Reflection;
using KojtoCAD.Mathematics.Geometry;
#if !bcad
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#else
using Teigha.Runtime;
using Bricscad.EditorInput;
using Application = Bricscad.ApplicationServices.Application;
#endif

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.Marks.WeldingMark))]

namespace KojtoCAD.BlockItems.Marks
{
    public class WeldingMark
    {
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly ILogger _logger = NullLogger.Instance;
        private readonly IBlockDrawingProvider _blockDrawingProvider;

        public WeldingMark()
        {
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Welding Mark
        /// </summary>
        [CommandMethod("wm")]
        public void WeldingMarkStart()
        {
            var arrowPointResult = _editorHelper.PromptForPoint("Pick arrow point :");
            if (arrowPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            var secondPointResult = _editorHelper.PromptForPoint("Pick base point :");
            if (secondPointResult.Status != PromptStatus.OK)
            {
                return;
            }

            Hashtable dynamicBlockProperties = new Hashtable();
            Hashtable dynamicBlockAttributes = new Hashtable();
            dynamicBlockProperties.Add("Distance", secondPointResult.Value.DistanceTo(arrowPointResult.Value));

            dynamicBlockProperties.Add("Base Point", arrowPointResult);
            dynamicBlockProperties.Add("Angle", GeometryUtility.GetAngleFromXAxis(secondPointResult.Value, arrowPointResult.Value));

             string dynamicBlockFullName = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
             if (dynamicBlockFullName == null)
             {
                 _editorHelper.WriteMessage("Dynamic block WeldingMark.dwg does not exist.");
                 return;
             }
            _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockFullName,
                 arrowPointResult.Value,
                 dynamicBlockProperties,
                 dynamicBlockAttributes);

            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
