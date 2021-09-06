using System;

namespace Trx.Viewer.Core.Domain
{
    public struct TestErrorInfo
    {
        public string Message { get; private set; }
        public string StackTrace { get; private set; }

        public TestErrorInfo(string message, string stackTrace)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            StackTrace = stackTrace ?? throw new ArgumentNullException(nameof(stackTrace));
        }
    }
}
