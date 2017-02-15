using System;
using System.Runtime.Serialization;

namespace KojtoCAD.Utilities.ErrorReporting.Exceptions
{
    [Serializable]
    class UiGeneratorException : ApplicationException
    {
        public UiGeneratorException()
            : base() { }

        public UiGeneratorException(string message)
            : base(message) { }

        public UiGeneratorException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public UiGeneratorException(string message, Exception innerException)
            : base(message, innerException) { }

        public UiGeneratorException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected UiGeneratorException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
