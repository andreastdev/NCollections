using System.Runtime.CompilerServices;

namespace NCollections.Core
{
    public ref struct NativeEnumerator<TUnmanaged>
        where TUnmanaged : unmanaged
    {
        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _count;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeEnumerator(in TUnmanaged* buffer, in int count)
        {
            _buffer = buffer;
            _count = count;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe NativeEnumerator(
            in TUnmanaged* buffer,
            in int count,
            in int startIndex,
            in int endIndex)
        {
            _buffer = buffer;
            _count = count;
            _index = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            var index = _index + 1;

            if ((uint)index >= (uint)_count) { return false; }

            _index = index;
            return true;
        }

        public readonly ref TUnmanaged Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                unsafe { return ref _buffer[_index]; }
            }
        }
    }
}