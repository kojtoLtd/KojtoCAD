using System;
using System.Collections.Generic;
#if !bcad
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Matrix3d = Autodesk.AutoCAD.Geometry.Matrix3d;
using PromptEntityResult = Autodesk.AutoCAD.EditorInput.PromptEntityResult;
#else
using Teigha.Geometry;
using Bricscad.EditorInput;
using Matrix3d = Teigha.Geometry.Matrix3d;
using PromptEntityResult = Bricscad.EditorInput.PromptEntityResult;
#endif

namespace KojtoCAD.Utilities.Interfaces
{
    public interface IEditorHelper
    {
        Matrix3d CurrentUcs { get; set; }

        PromptIntegerResult PromptForInteger(string promptMessage);
        PromptDoubleResult PromptForDouble(string promptMessage, double defaultValue);
        PromptResult PromptForKeywordSelection(string promptMessage, IEnumerable<string> keywords, bool allowNone, string defaultKeyword = "");
        PromptPointResult PromptForPoint(string promptMessage, bool useDashedLine = false, bool useBasePoint = false, Point3d basePoint = new Point3d(),bool allowNone = true);
        PromptEntityResult PromptForObject(string promptMessage, Type allowedType, bool exactMatchOfAllowedType);
        Point3dCollection PromptForRectangle(out PromptStatus status, string promptMessage);
        PromptSelectionResult PromptForSelection(string promptMessage = null, SelectionFilter filter = null);
        void WriteMessage(string message);
        void DrawVector(Point3d from, Point3d to, int color, bool drawHighlighted);

    }
}
