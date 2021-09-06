using System;

namespace Trx.Viewer.Core.Business.Trx.Exception
{

    [Serializable]
    public class TrxSchemaValidationException : System.Exception
    {
        public TrxSchemaValidationException() { }
        public TrxSchemaValidationException(string message) : base(message) { }
        public TrxSchemaValidationException(string message, System.Exception inner) : base(message, inner) { }
        protected TrxSchemaValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
