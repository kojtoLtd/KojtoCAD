using Castle.Core.Logging;
using KojtoCAD.BlockItems.Interfaces;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;
using System.Collections;
using System.Drawing.Printing;
using System.Reflection;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
#else
using Bricscad.ApplicationServices;
using Teigha.Geometry;
using Teigha.Runtime;
using Bricscad.EditorInput;
#endif

[assembly: CommandClass(typeof(KojtoCAD.BlockItems.DrawingFrame))]

namespace KojtoCAD.BlockItems
{
    public class DrawingFrame
    {
        private Point3d _insertionPoint;
        private readonly EditorHelper _editorHelper;
        private readonly DocumentHelper _drawingHelper;
        private readonly IPaperSizeFactory _paperSizeFactory;
        private readonly IBlockDrawingProvider _blockDrawingProvider;


        private string _selectedPaperSize;
        private string _selectedPaperOrientation;

        private readonly ILogger _logger = NullLogger.Instance;

        public DrawingFrame()
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            _paperSizeFactory = IoC.ContainerRegistrar.Container.Resolve<IPaperSizeFactory>();
            _blockDrawingProvider = IoC.ContainerRegistrar.Container.Resolve<IBlockDrawingProvider>();
            _editorHelper = new EditorHelper(Application.DocumentManager.MdiActiveDocument);
            _drawingHelper = new DocumentHelper(Application.DocumentManager.MdiActiveDocument);
        }

        /// <summary>
        /// Insert drawing frame
        /// </summary>
        [CommandMethod("df")]
        public void DrawingFrameStart()
        {
            var selectedPaperSizeResult = _editorHelper.PromptForKeywordSelection(
                "Select paper size : ", new[] { "A4", "A3", "A2", "A1", "A0" }, false);
            if (selectedPaperSizeResult.Status != PromptStatus.OK)
            {
                return;
            }
            _selectedPaperSize = selectedPaperSizeResult.StringResult;

            var selectedPaperOrientationResult = _editorHelper.PromptForKeywordSelection(
                "Select paper orientation : ", new[] { "Landscape", "Portrait" }, false);
            if (selectedPaperOrientationResult.Status != PromptStatus.OK)
            {
                return;
            }
            _selectedPaperOrientation = selectedPaperOrientationResult.StringResult;

            var insertionPointResult = _editorHelper.PromptForPoint("Pick the lower right point of the frame : ");
            if (insertionPointResult.Status != PromptStatus.OK)
            {
                return;
            }
            _insertionPoint = insertionPointResult.Value;
            PaperSize orientedPaperSize = _paperSizeFactory.CreateOrientedPaperSize(
                _selectedPaperSize, _selectedPaperOrientation);


            if (_selectedPaperOrientation == "Portrait")
            {
                orientedPaperSize.PaperName += "r";
            }

            Hashtable dynamicBlockProperties = new Hashtable();
            dynamicBlockProperties.Add("Formats", orientedPaperSize.PaperName);

            var dynamicBlockPath = _blockDrawingProvider.GetBlockFile(MethodBase.GetCurrentMethod().DeclaringType.Name);
            if (dynamicBlockPath == null)
            {
                _editorHelper.WriteMessage("Dynamic block DrawingFrame.dwg does not exist.");
                return;
            }
            _drawingHelper.ImportDynamicBlockAndFillItsProperties(
                dynamicBlockPath, _insertionPoint, dynamicBlockProperties, new Hashtable());

            _logger.Info(MethodBase.GetCurrentMethod().Name);
        }
    }
}
