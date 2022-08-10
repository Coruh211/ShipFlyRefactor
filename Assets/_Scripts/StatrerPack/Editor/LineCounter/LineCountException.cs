using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace System
{
    [Serializable]
    public class LineCountException : Exception
    {
        public LineCountException() : base() { }
        public LineCountException(string message) : base(message) { }
        public LineCountException(string message, Exception inner) : base(message, inner) { }

        protected LineCountException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    }
}