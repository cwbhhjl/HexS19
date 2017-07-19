using System;
using System.Runtime.Serialization;

namespace MiUtil.HexS19
{
    [Serializable]
    public sealed class BadChecksumException : ApplicationException
    {
        public BadChecksumException() : base() { }

        public BadChecksumException(string msg) : base(msg) { }

        public BadChecksumException(string msg, Exception inner) : base(msg, inner) { }

        private BadChecksumException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
