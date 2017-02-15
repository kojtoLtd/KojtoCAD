using System;
using System.Runtime.Serialization;

namespace KojtoCAD.Utilities.ErrorReporting.Exceptions
{
    [Serializable]
    public class IconProviderException : ApplicationException
    {
        public IconProviderException()
            : base() { }

        public IconProviderException(string message)
            : base(message) { }

        public IconProviderException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public IconProviderException(string message, Exception innerException)
            : base(message, innerException) { }

        public IconProviderException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }

        protected IconProviderException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
