using System;
using System.Runtime.Serialization;

namespace KojtoCAD.Utilities.ErrorReporting.Exceptions
{
    [Serializable]
    public class LayerDoesNotExistException : ApplicationException
    {
        public LayerDoesNotExistException()
            : base() { }

        public LayerDoesNotExistException(string message)
            : base(message) { }

        public LayerDoesNotExistException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public LayerDoesNotExistException(string message, Exception innerException)
            : base(message, innerException) { }

        public LayerDoesNotExistException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected LayerDoesNotExistException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

}
