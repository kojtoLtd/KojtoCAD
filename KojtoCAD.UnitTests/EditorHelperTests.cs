using System;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.Interfaces;
using MbUnit.Framework;
using Autodesk.AutoCAD.ApplicationServices;

namespace KojtoCAD.UnitTests
{
    public class When_working_with_IEditorHelper
    {
        protected static readonly Document _document = Application.DocumentManager.MdiActiveDocument;
        protected static readonly Editor _editor = _document.Editor;
        protected IEditorHelper _editorHelper = new EditorHelper(_document);
        protected Exception _resultException;
    }
    public class and_prompt_for_integer_and_insert_integer_plus_enter_key : When_working_with_IEditorHelper
    {
        private PromptIntegerResult _promptIntResult;
        
        [Test]
        public void then_valid_integer_result_and_prompt_status_OK_should_be_returned()
        {
            _editor.PromptingForInteger += _editor_PromptingForInteger;
            try
            {
                _promptIntResult = _editorHelper.PromptForInteger("Insert INTEGER");
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForInteger -= _editor_PromptingForInteger;

            Assert.AreEqual(null,_resultException);
            Assert.AreEqual(6,_promptIntResult.Value);
            Assert.AreEqual(PromptStatus.OK,_promptIntResult.Status);
        }

        private void _editor_PromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            _document.SendStringToExecute("6 ", true, false, true);
        }
    }

    public class and_prompt_for_integer_end_cancel_the_operation : When_working_with_IEditorHelper
    {
        private PromptIntegerResult _promptIntResult;

        [Test]
        public void then_null_result_and_prompt_status_cancel_should_be_returned()
        {
            _editor.PromptingForInteger += _editor_PromptingForInteger;
            try
            {
                _promptIntResult = _editorHelper.PromptForInteger("");
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForInteger -= _editor_PromptingForInteger;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual(0, _promptIntResult.Value);
            Assert.AreEqual(PromptStatus.Cancel, _promptIntResult.Status);
        }

        private void _editor_PromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            _document.SendStringToExecute("\x1b", true, false, true);
        }
    }

    public class and_prompt_for_double_and_insert_double_plus_enter_key : When_working_with_IEditorHelper
    {
        private PromptDoubleResult _promptDoubleResult;

        [Test]
        public void then_valid_double_result_and_prompt_status_OK_should_be_returned()
        {
            _editor.PromptingForDouble += _editor_PromptingForDouble;
            try
            {
                _promptDoubleResult = _editorHelper.PromptForDouble("",1);
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForDouble -= _editor_PromptingForDouble;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual(6.0,_promptDoubleResult.Value);
            Assert.AreEqual(PromptStatus.OK, _promptDoubleResult.Status);
        }

        private void _editor_PromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            _document.SendStringToExecute("6.0 ", true, false, true);
        }
    }

    public class and_prompt_for_double_end_cancel_the_operation : When_working_with_IEditorHelper
    {
        private PromptDoubleResult _promptDoubleResult;

        [Test]
        public void then_null_result_and_prompt_status_cancel_should_be_returned()
        {
            _editor.PromptingForDouble += _editor_PromptingForDouble;
            try
            {
                _promptDoubleResult = _editorHelper.PromptForDouble("",0.0);
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForDouble -= _editor_PromptingForDouble;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual(0, _promptDoubleResult.Value);
            Assert.AreEqual(PromptStatus.Cancel, _promptDoubleResult.Status);
        }

        private void _editor_PromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            _document.SendStringToExecute("\x1b", true, false, true);
        }
    }

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //public class and_prompt_for_object_and_insert_entity_from_the_allowed_type : When_working_with_IEditorHelper
    //{
    //    private PromptEntityResult _promptEntityResult;

    //    [Test]
    //    public void then_valid_object_result_and_prompt_status_OK_should_be_returned()
    //    {
    //        _editor.PromptingForEntity += _editor_PromptingForEntity;
    //        try
    //        {
    //            _promptEntityResult = _editorHelper.PromptForObject("",typeof(Line),true);
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForEntity -= _editor_PromptingForEntity;

    //        Assert.AreEqual(null, _resultException);
    //        Assert.IsInstanceOfType<Double>(_promptEntityResult.Value);
    //        Assert.AreEqual(PromptStatus.OK, _promptEntityResult.Status);
    //    }

    //    private void _editor_PromptingForEntity(object sender, PromptDoubleOptionsEventArgs e)
    //    {
    //        _document.("6.0 ", true, false, true);
    //    }
    //}

    //public class and_prompt_for_object_end_cancel_the_operation : When_working_with_IEditorHelper
    //{
    //    private PromptDoubleResult _promptDoubleResult;

    //    [Test]
    //    public void then_null_result_and_prompt_status_cancel_should_be_returned()
    //    {
    //        _editor.PromptingForDouble += _editor_PromptingForDouble;
    //        try
    //        {
    //            _promptDoubleResult = _editorHelper.PromptForDouble("", 0.0);
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForDouble -= _editor_PromptingForDouble;

    //        Assert.AreEqual(null, _resultException);
    //        Assert.AreEqual(0, _promptDoubleResult.Value);
    //        Assert.AreEqual(PromptStatus.Cancel, _promptDoubleResult.Status);
    //    }

    //    private void _editor_PromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
    //    {
    //        _document.SendStringToExecute("\x1b", true, false, true);
    //    }
    //}
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public class and_prompt_for_keyword_selection_and_insert_availeble_keyword_plus_enter_key : When_working_with_IEditorHelper
    {
        private PromptResult _promptStringResult;

        [Test]
        public void then_valid_string_result_and_prompt_status_OK_should_be_returned()
        {
            _editor.PromptingForKeyword += _editor_PromptingForKeyword;
            string[] keywords = {"first", "second", "third"};
            try
            {
                _promptStringResult = _editorHelper.PromptForKeywordSelection("", keywords, false);
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForKeyword -= _editor_PromptingForKeyword;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual(keywords[0],_promptStringResult.StringResult);
            Assert.AreEqual(PromptStatus.OK, _promptStringResult.Status);
        }

        private void _editor_PromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            _document.SendStringToExecute("first ", true, false, true);
        }
    }

    public class and_prompt_for_keyword_selection_end_cancel_the_operation : When_working_with_IEditorHelper
    {
        private PromptResult _promptStringResult;

        [Test]
        public void then_null_result_and_prompt_status_cancel_should_be_returned()
        {
            _editor.PromptingForKeyword += _editor_PromptingForKeyword;
            string[] keywords = { "first", "second", "third" };
            try
            {
                _promptStringResult = _editorHelper.PromptForKeywordSelection("", keywords, false);
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForKeyword -= _editor_PromptingForKeyword;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual("", _promptStringResult.StringResult);
            Assert.AreEqual(PromptStatus.Cancel, _promptStringResult.Status);
        }

        private void _editor_PromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            _document.SendStringToExecute("\x1b", true, false, true);
        }
    }
    public class and_prompt_for_keyword_selection_and_allowNone_is_true_and_enter_enpty_string : When_working_with_IEditorHelper
    {
        private PromptResult _promptStringResult;

        [Test]
        public void then_empty_string_result_and_prompt_status_NONE_should_be_returned()
        {
            _editor.PromptingForKeyword += _editor_PromptingForKeyword;
            string[] keywords = { "first", "second", "third" };
            try
            {
                _promptStringResult = _editorHelper.PromptForKeywordSelection("", keywords, true);
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForKeyword -= _editor_PromptingForKeyword;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual("", _promptStringResult.StringResult);
            Assert.AreEqual(PromptStatus.None, _promptStringResult.Status);
        }

        private void _editor_PromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            _document.SendStringToExecute(" ", true, false, true);
        }
    }
    
    public class and_prompt_for_point_and_insert_correct_point : When_working_with_IEditorHelper
    {
        private PromptPointResult _promptPointResult;

        [Test]
        public void then_valid_point_and_prompt_status_OK_should_be_returned()
        {
            _editor.PromptingForPoint += _editor_PromptingForPoint;
            try
            {
                _promptPointResult = _editorHelper.PromptForPoint("");
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForPoint -= _editor_PromptingForPoint;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual(new Point3d(6,0,0), _promptPointResult.Value);
            Assert.AreEqual(PromptStatus.OK, _promptPointResult.Status);
        }

        private void _editor_PromptingForPoint(object sender, PromptPointOptionsEventArgs e)
        {
            _document.SendStringToExecute("6,0,0 ", true, false, true);
        }
    }

    public class and_prompt_for_point_end_cancel_the_operation : When_working_with_IEditorHelper
    {
        private PromptPointResult _promptPointResult;

        [Test]
        public void then_point_0_0_0_result_and_prompt_status_cancel_should_be_returned()
        {
            _editor.PromptingForPoint += _editor_PromptingForPoint;
            try
            {
                _promptPointResult = _editorHelper.PromptForPoint("");
            }
            catch (Exception promptException)
            {
                _resultException = promptException;
            }
            _editor.PromptingForPoint -= _editor_PromptingForPoint;

            Assert.AreEqual(null, _resultException);
            Assert.AreEqual(new Point3d(0, 0, 0), _promptPointResult.Value);
            Assert.AreEqual(PromptStatus.Cancel, _promptPointResult.Status);
        }

        private void _editor_PromptingForPoint(object sender, PromptPointOptionsEventArgs e)
        {
            _document.SendStringToExecute("\x1b", true, false, true);
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    
    //public class and_prompt_for_selection_with_string_message_and_choose_prope_objects : When_working_with_IEditorHelper
    //{
    //    private PromptSelectionResult _promptSelectionResult;

    //    [Test]
    //    public void then_valid_IDs_and_prompt_status_OK_should_be_returned()
    //    {
    //        _editor.PromptingForSelection += _editor_PromptingForSelection;
    //        var typedValues = new TypedValue[1];
    //        typedValues.SetValue(new TypedValue((int)DxfCode.Start, "XLINE"), 0);
    //        var filter = new SelectionFilter(typedValues);
    //        try
    //        {
    //            _promptSelectionResult = _editorHelper.PromptForSelection("Message", filter);
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForSelection -= _editor_PromptingForSelection;

    //        Assert.AreEqual(null, _resultException);
    //        //Assert.AreEqual(6.0, _promptSelectionResult.Value);
    //        Assert.AreEqual(PromptStatus.OK, _promptSelectionResult.Status);
    //    }

    //    private void _editor_PromptingForSelection(object sender, PromptSelectionOptionsEventArgs e)
    //    {
    //        _document.SendStringToExecute("6.0 ", true, false, true);
    //    }
    //}

    //public class and_prompt_for_selection_without_string_message : When_working_with_IEditorHelper
    //{
    //    private PromptSelectionResult _promptSelectionResult;

    //    [Test]
    //    public void then_valid_IDs_and_prompt_status_OK_should_be_returned()
    //    {
    //        _editor.PromptingForSelection += _editor_PromptingForSelection;
    //        var typedValues = new TypedValue[1];
    //        typedValues.SetValue(new TypedValue((int)DxfCode.Start, "XLINE"), 0);
    //        var filter = new SelectionFilter(typedValues);
    //        try
    //        {
    //            _promptSelectionResult = _editorHelper.PromptForSelection("", filter);
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForSelection -= _editor_PromptingForSelection;

    //        Assert.AreEqual(null, _resultException);
    //       // Assert.AreEqual(6.0, _promptSelectionResult.Value);
    //        Assert.AreEqual(PromptStatus.OK, _promptSelectionResult.Status);
    //    }

    //    private void _editor_PromptingForSelection(object sender, PromptSelectionOptionsEventArgs e)
    //    {
    //        _document.SendStringToExecute("6.0 ", true, false, true);
    //    }
    //}

    //public class and_prompt_for_selection_end_cancel_the_operation : When_working_with_IEditorHelper
    //{
    //    private PromptSelectionResult _promptSelectionResult;

    //    [Test]
    //    public void then_none_IDs_and_prompt_status_cancel_should_be_returned()
    //    {
    //        _editor.PromptingForSelection += _editor_PromptingForSelection;
    //        var typedValues = new TypedValue[1];
    //        typedValues.SetValue(new TypedValue((int)DxfCode.Start, "XLINE"), 0);
    //        var filter = new SelectionFilter(typedValues);
            
    //        try
    //        {
    //            _promptSelectionResult = _editorHelper.PromptForSelection("",filter);
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForSelection -= _editor_PromptingForSelection;

    //        Assert.AreEqual(null, _resultException);
    //       // Assert.AreEqual(0, _promptSelectionResult.Value);
    //        Assert.AreEqual(PromptStatus.Cancel, _promptSelectionResult.Status);
    //    }

    //    private void _editor_PromptingForSelection(object sender, PromptSelectionOptionsEventArgs e)
    //    {
    //        _document.SendStringToExecute("\x1b", true, false, true);
    //    }
    //}
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////

    //public class and_prompt_for_rectangle : When_working_with_IEditorHelper
    //{
    //    private Point3dCollection _pointCollection;
    //    private PromptStatus _promptRectangelStatus;

    //    [Test]
    //    public void then_valid_point_collection_and_prompt_status_OK_should_be_returned()
    //    {
    //        _editor.PromptingForPoint+=_editor_PromptingForPoint;
    //        try
    //        {
    //            _pointCollection = _editorHelper.PromptForRectangle(out _promptRectangelStatus, "");
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForPoint -= _editor_PromptingForPoint;
    //    }

    //    private void _editor_PromptingForPoint(object sender, PromptPointOptionsEventArgs e)
    //    {
    //        _document.SendStringToExecute("6,6 ", true, false, true);
    //    }
    //}

    //public class sdadsaaaa_operation : When_working_with_IEditorHelper
    //{
    //    private PromptDoubleResult _promptDoubleResult;

    //    [Test]
    //    public void then_null_result_and_prompt_status_cancel_should_be_returned()
    //    {
    //        _editor.PromptingForDouble += _editor_PromptingForDouble;
    //        try
    //        {
    //            _promptDoubleResult = _editorHelper.PromptForDouble("",0.0);
    //        }
    //        catch (Exception promptException)
    //        {
    //            _resultException = promptException;
    //        }
    //        _editor.PromptingForDouble -= _editor_PromptingForDouble;

    //        Assert.AreEqual(null, _resultException);
    //        Assert.AreEqual(0, _promptDoubleResult.Value);
    //        Assert.AreEqual(PromptStatus.Cancel, _promptDoubleResult.Status);
    //    }

    //    private void _editor_PromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
    //    {
    //        _document.SendStringToExecute("\x1b", true, false, true);
    //    }
}
