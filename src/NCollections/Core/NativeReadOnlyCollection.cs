using NCollections.Internal;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Core
{
    public readonly struct NativeReadOnlyCollection<TUnmanaged>
        : IEquatable<NativeReadOnlyCollection<TUnmanaged>>, IDisposable
        where TUnmanaged : unmanaged
    {
        public static NativeReadOnlyCollection<TUnmanaged> Void { get; } = new();

        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _count;

        public int Count => _count;

        public int ByteCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count * Unsafe.SizeOf<TUnmanaged>();
        }

        internal unsafe NativeReadOnlyCollection(in TUnmanaged* buffer, in int count)
        {
            if (count <= 0)
            {
                this = Void;

                return;
            }

            _buffer = buffer;
            _count = count;
        }

        internal unsafe NativeReadOnlyCollection(
            in TUnmanaged* buffer,
            in int count,
            in int startIndex,
            in int endIndex)
        {
            if (count <= 0)
            {
                this = Void;

                return;
            }

            _buffer = buffer;
            _count = count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReadOnlyCollection()
        {
            unsafe
            {
                _buffer = (TUnmanaged*)Unsafe.AsPointer(ref Unsafe.NullRef<TUnmanaged>());
                _count = 0;
            }
        }

        public ref TUnmanaged this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_count)
                {
                    ThrowHelpers.IndexOutOfRangeException();
                }

                unsafe
                {
                    return ref _buffer[index];
                }
            }
        }

        public bool TryGet(int index, out TUnmanaged item)
        {
            if ((uint)index >= (uint)_count)
            {
                item = default;

                return false;
            }

            unsafe
            {
                item = _buffer[index];

                return true;
            }
        }

        public bool TryGetRange(
            int startIndex,
            int length,
            out ReadOnlySpan<TUnmanaged> items,
            bool allOrNothing = true)
        {
            var availableRange = _count - startIndex;

            if ((uint)startIndex >= (uint)_count || (allOrNothing && availableRange != 0 && length > availableRange))
            {
                items = ReadOnlySpan<TUnmanaged>.Empty;

                return false;
            }

            unsafe
            {
                items = new ReadOnlySpan<TUnmanaged>(
                    &_buffer[startIndex],
                    !allOrNothing && length >= availableRange ? availableRange : length);

                return true;
            }
        }

        public void Dispose()
        {
            unsafe
            {
                if (Equals(Void))
                {
                    return;
                }

                NativeMemory.Free(_buffer);
            }
        }

        public NativeEnumerator<TUnmanaged> GetEnumerator()
        {
            unsafe { return new NativeEnumerator<TUnmanaged>(in _buffer, in _count); }
        }

        public ref TUnmanaged GetPinnableReference()
        {
            if (_count != 0)
            {
                unsafe { return ref _buffer[0]; }
            }

            return ref Unsafe.NullRef<TUnmanaged>();
        }

        public bool Equals(NativeReadOnlyCollection<TUnmanaged> other)
        {
            unsafe
            {
                return _buffer == other._buffer && _count == other._count && GetHashCode() == other.GetHashCode();
            }
        }

        public override bool Equals(object? obj) => obj is NativeReadOnlyCollection<TUnmanaged> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                var hash = hashingBase;

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(NativeReadOnlyCollection<TUnmanaged>))
                           ? typeof(NativeReadOnlyCollection<TUnmanaged>).GetHashCode()
                           : 0);

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(TUnmanaged)) ? typeof(TUnmanaged).GetHashCode() : 0);

                unsafe
                {
                    hash = (hash * hashingMultiplier) ^ new IntPtr(_buffer).GetHashCode();
                }

                hash = (hash * hashingMultiplier) ^ _count.GetHashCode();

                return hash;
            }
        }

        public override string ToString() =>
            $"{nameof(NativeReadOnlyCollection<TUnmanaged>)}<{typeof(TUnmanaged).Name}>[Count: {_count}]";

        public static bool operator ==(
            NativeReadOnlyCollection<TUnmanaged> lhs,
            NativeReadOnlyCollection<TUnmanaged> rhs) =>
            lhs.Equals(rhs);

        public static bool operator !=(
            NativeReadOnlyCollection<TUnmanaged> lhs,
            NativeReadOnlyCollection<TUnmanaged> rhs) =>
            !(lhs == rhs);

        public static implicit operator NativeReadOnlyCollection<TUnmanaged>(NativeList<TUnmanaged> nativeList)
        {
            unsafe
            {
                fixed (TUnmanaged* pointer = nativeList)
                {
                    return new NativeReadOnlyCollection<TUnmanaged>(pointer, nativeList.Count);
                }
            }
        }
    }
}