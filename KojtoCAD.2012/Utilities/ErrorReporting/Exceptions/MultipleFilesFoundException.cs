using System;
using System.Runtime.Serialization;

namespace KojtoCAD.Utilities.ErrorReporting.Exceptions
{
    [Serializable]
    public class MultipleFilesFoundException : Exception
    {
        public MultipleFilesFoundException()
        {
        }

        public MultipleFilesFoundException(string message)
            : base(message)
        {
        }

        public MultipleFilesFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MultipleFilesFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
