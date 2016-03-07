using System;

namespace FileFind
{
    [Serializable]
    public class FileFindException : Exception
    {
        public FileFindException() { }
        public FileFindException(string message) : base(message) { }
        public FileFindException(string message, Exception inner) : base(message, inner) { }
        protected FileFindException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
