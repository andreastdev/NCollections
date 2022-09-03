using System;

namespace NCollections.Tests
{
    public class BaseTests : IDisposable
    {
        protected BaseTests() { }
        
        public virtual void Dispose() { }
    }
}