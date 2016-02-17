using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDFPatcher
{
    [Serializable]
    public class BDFReadException : Exception
    {
        public BDFReadException() { }
        public BDFReadException(string message) : base(message) { }
        public BDFReadException(string message, Exception inner) : base(message, inner) { }
        protected BDFReadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BDFHeaderReadException : Exception
    {
        public BDFHeaderReadException() { }
        public BDFHeaderReadException(string message) : base(message) { }
        public BDFHeaderReadException(string message, Exception inner) : base(message, inner) { }
        protected BDFHeaderReadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BDFBodyReadException : Exception
    {
        public BDFBodyReadException() { }
        public BDFBodyReadException(string message) : base(message) { }
        public BDFBodyReadException(string message, Exception inner) : base(message, inner) { }
        protected BDFBodyReadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BDFPatchException : Exception
    {
        public BDFPatchException() { }
        public BDFPatchException(string message) : base(message) { }
        public BDFPatchException(string message, Exception inner) : base(message, inner) { }
        protected BDFPatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BDFIncompatibleFilesException : Exception
    {
        public BDFIncompatibleFilesException() { }
        public BDFIncompatibleFilesException(string message) : base(message) { }
        public BDFIncompatibleFilesException(string message, Exception inner) : base(message, inner) { }
        protected BDFIncompatibleFilesException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BDFSaveException : Exception
    {
        public BDFSaveException() { }
        public BDFSaveException(string message) : base(message) { }
        public BDFSaveException(string message, Exception inner) : base(message, inner) { }
        protected BDFSaveException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class BDFFileIsNotReadException : Exception
    {
        public BDFFileIsNotReadException() { }
        public BDFFileIsNotReadException(string message) : base(message) { }
        public BDFFileIsNotReadException(string message, Exception inner) : base(message, inner) { }
        protected BDFFileIsNotReadException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
