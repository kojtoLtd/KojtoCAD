using System;
using System.Runtime.Serialization;

namespace KojtoCAD.Utilities.ErrorReporting.Exceptions
{
    [Serializable]
    class MenuSchemaProviderException : ApplicationException
    {
        public MenuSchemaProviderException()
            : base() { }

        public MenuSchemaProviderException(string message)
            : base(message) { }

        public MenuSchemaProviderException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public MenuSchemaProviderException(string message, Exception innerException)
            : base(message, innerException) { }

        public MenuSchemaProviderException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected MenuSchemaProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
