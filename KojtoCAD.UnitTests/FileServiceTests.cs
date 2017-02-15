using System;
using System.IO;
using KojtoCAD.Utilities;
using KojtoCAD.Utilities.ErrorReporting.Exceptions;
using KojtoCAD.Utilities.Interfaces;
using NBehave.Spec.NUnit;
using NUnit.Framework;

namespace KojtoCAD.UnitTests
{
    public class When_working_with_IFileService : Specification
    {
        protected IFileService _fileService;
        protected Exception _expectedException;
        protected string _fileName;

        protected override void Establish_context()
        {
            base.Establish_context();
            _fileService = new FileService();
        }
    }

    public class and_making_request_with_nonexisting_directory : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.nonexistingRootDirectory,
                                                 Properties.Settings.Default.nonexistingFileNamePart);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_DirectoryNotFoundException_must_be_thrown()
        {
            _fileName.ShouldBeNull();
            _expectedException.ShouldBeInstanceOfType(typeof(DirectoryNotFoundException));
        }
    }

    public class and_making_request_with_nonexisting_file : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.nonexistingFileNamePart);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_fileName_must_be_Null()
        {
            _fileName.ShouldBeNull();
        }
    }

    public class and_making_request_with_proper_directory_and_proper_file_without_extension : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.singleFullFileName);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_valid_file_must_be_returned()
        {
            _expectedException.ShouldBeNull();
            File.Exists(_fileName).ShouldBeTrue();
        }
    }

    public class and_making_request_with_proper_directory_and_proper_file_and_proper_extension : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.multipleFileNamePart,
                                                 Properties.Settings.Default.properFileExtension);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_valid_file_must_be_returned()
        {
            _expectedException.ShouldBeNull();
            File.Exists(_fileName).ShouldBeTrue();
        }
    }

    public class and_making_request_with_not_accessible_subdirectory : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.notAccessibleSubDirectory,
                                                 Properties.Settings.Default.singleFileNamePart);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_UnauthorizedAccessException_must_be_thrown()
        {
            _fileName.ShouldBeNull();
            _expectedException.ShouldBeInstanceOfType(typeof(UnauthorizedAccessException));
        }
    }

    public class and_making_request_with_proper_directory_proper_file_and_unproper_extension : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.singleFileNamePart,
                                                 Properties.Settings.Default.unproperFileExtension);
            }
            catch (Exception resultException)
            {

                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_fileName_must_be_Null()
        {
            _fileName.ShouldBeNull();
        }
    }

    public class and_making_requeset_with_proper_directory_and_file_without_extension_and_there_are_some_files_with_same_name_and_different_types : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.multipleFileNamePart);
            }
            catch (Exception resultException)
            {

                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_MultipleFilesFoundException_must_be_thrown()
        {
            _fileName.ShouldBeNull();
            _expectedException.ShouldBeInstanceOfType(typeof(MultipleFilesFoundException));
        }
    }

    public class and_making_request_with_proper_directory_and_file_name_and_dont_search_in_subdirectories : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.singleFileNamePart,
                                                 Properties.Settings.Default.nullFileExtension,
                                                 Properties.Settings.Default.falseTraverseSubdirectories);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_valid_file_should_be_returned()
        {
            _expectedException.ShouldBeNull();
            File.Exists(_fileName).ShouldBeTrue();
        }
    }
    public class and_making_request_with_proper_directory_and_file_name_with_point_in_it_without_extension : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.fileNameWithPoint,
                                                 Properties.Settings.Default.nullFileExtension,
                                                 Properties.Settings.Default.trueTraverseSubDirectories);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_valid_file_should_be_returned()
        {
            _expectedException.ShouldBeNull();
            File.Exists(_fileName).ShouldBeTrue();
        }
    }
    public class and_making_request_with_proper_directory_and_exact_file_name_and_there_are_many_files_with_this_name : When_working_with_IFileService
    {
        protected override void Because_of()
        {
            try
            {
                _fileName = _fileService.GetFile(Properties.Settings.Default.existingRootDirectorty,
                                                 Properties.Settings.Default.exactFileName,
                                                 Properties.Settings.Default.nullFileExtension,
                                                 Properties.Settings.Default.falseTraverseSubdirectories);
            }
            catch (Exception resultException)
            {
                _expectedException = resultException.InnerException;
            }
        }

        [Test]
        public void then_valid_file_should_be_returned()
        {
            _expectedException.ShouldBeNull();
            File.Exists(_fileName).ShouldBeTrue();
        }
    }
}
