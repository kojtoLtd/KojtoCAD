using System;
using Castle.Core.Logging;

namespace KojtoCAD.Utilities
{
    public class SmartLogger : ILogger
    {
        private readonly ILogger _logger;

        public SmartLogger()
        {
            _logger = new TraceLogger("KojtoCAD", LoggerLevel.Debug);
        }

        public ILogger CreateChildLogger(string loggerName)
        {
            return _logger.CreateChildLogger(loggerName);
        }

        public void Debug(Func<string> messageFactory)
        {
            _logger.Debug(messageFactory);
        }
        public void Debug(string message)
        {
            _logger.Debug(message);
        }
        public void Debug(string message, Exception exception)
        {
            _logger.Debug(message + exception.Message + exception.Source + exception.StackTrace, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _logger.DebugFormat(format, args);
        }
        public void DebugFormat(Exception exception, string format, params object[] args)
        {
            _logger.DebugFormat(exception, format, args);

        }
        public void DebugFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.DebugFormat(formatProvider, format, args);
        } 
        public void DebugFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.DebugFormat(exception, formatProvider, format, args);
        }

        public void Error(Func<string> messageFactory)
        {
            _logger.Error(messageFactory);
        }
        public void Error(string message)
        {
            _logger.Error(message);
        }
        public void Error(string message, Exception exception)
        {
            _logger.Error(message + exception.Message + exception.Source + exception.StackTrace, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _logger.ErrorFormat(format, args);
        }
        public void ErrorFormat(Exception exception, string format, params object[] args)
        {
            _logger.ErrorFormat(exception, format, args);
        }
        public void ErrorFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.ErrorFormat(formatProvider, format, args);
        }
        public void ErrorFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.ErrorFormat(exception, formatProvider, format, args);
        }

        public void Fatal(Func<string> messageFactory)
        {
            _logger.Fatal(messageFactory);
        }
        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }
        public void Fatal(string message, Exception exception)
        {
            _logger.Fatal(message + exception.Message + exception.Source + exception.StackTrace, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _logger.FatalFormat(format, args);
        }
        public void FatalFormat(Exception exception, string format, params object[] args)
        {
            _logger.FatalFormat(exception, format, args);
        }
        public void FatalFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.FatalFormat(formatProvider, format, args);
        }
        public void FatalFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.FatalFormat(exception, formatProvider, format, args);
        }

        public void Info(Func<string> messageFactory)
        {
            _logger.Info(messageFactory);
        }
        public void Info(string message)
        {
            _logger.Info(message);
        }
        public void Info(string message, Exception exception)
        {
            _logger.Info(message + exception.Message + exception.Source + exception.StackTrace, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _logger.InfoFormat(format, args);
        }
        public void InfoFormat(Exception exception, string format, params object[] args)
        {
            _logger.InfoFormat(exception, format, args);
        }
        public void InfoFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.InfoFormat(formatProvider, format, args);
        }
        public void InfoFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.InfoFormat(exception, formatProvider, format, args);
        }

        public void Warn(Func<string> messageFactory)
        {
            _logger.Warn(messageFactory);
        }
        public void Warn(string message)
        {
            _logger.Warn(message);
        }
        public void Warn(string message, Exception exception)
        {
            _logger.Warn(message + exception.Message + exception.Source + exception.StackTrace, exception);
        }
        
        public void WarnFormat(string format, params object[] args)
        {
            _logger.WarnFormat(format, args);
        }
        public void WarnFormat(Exception exception, string format, params object[] args)
        {
            _logger.WarnFormat(exception, format, args);

        }
        public void WarnFormat(IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.WarnFormat(formatProvider, format, args);
        }
        public void WarnFormat(Exception exception, IFormatProvider formatProvider, string format, params object[] args)
        {
            _logger.WarnFormat(exception, formatProvider, format, args);
        }

        public bool IsDebugEnabled => _logger.IsDebugEnabled;
        public bool IsErrorEnabled => _logger.IsErrorEnabled;
        public bool IsFatalEnabled => _logger.IsFatalEnabled;
        public bool IsInfoEnabled => _logger.IsInfoEnabled;

        bool ILogger.IsWarnEnabled => _logger.IsWarnEnabled;
    }
}
