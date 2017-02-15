using System;
using System.Runtime.Serialization;

namespace KojtoCAD.Utilities.ErrorReporting.Exceptions
{
    [Serializable]
    public class FileServiceException : ApplicationException
    {
        public FileServiceException()
            : base() { }

        public FileServiceException(string message)
            : base(message) { }

        public FileServiceException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public FileServiceException(string message, Exception innerException)
            : base(message, innerException) { }

        public FileServiceException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected FileServiceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
