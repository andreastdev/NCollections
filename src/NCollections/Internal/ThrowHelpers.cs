using System;
using System.Diagnostics.CodeAnalysis;

namespace NCollections.Internal
{
    internal static class ThrowHelpers
    {
        [DoesNotReturn]
        internal static void IndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }
        
        [DoesNotReturn]
        internal static void InvalidOperationException()
        {
            throw new InvalidOperationException();
        }
    }
}