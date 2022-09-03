using NCollections.Internal;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeList<TUnmanaged> : IEquatable<NativeList<TUnmanaged>>, IDisposable
        where TUnmanaged : unmanaged
    {
        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _capacity;
        private int _count;

        public static NativeList<TUnmanaged> Void { get; } = new();

        public readonly int Capacity => _capacity;

        public readonly int Count => _count;

        public readonly int ByteCapacity
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _capacity * Unsafe.SizeOf<TUnmanaged>();
        }

        public readonly int CurrentByteCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _count * Unsafe.SizeOf<TUnmanaged>();
        }

        public readonly bool IsEmpty => _count == 0;

        public readonly bool IsFull => _capacity == _count;

        public NativeList(in ReadOnlySpan<TUnmanaged> span)
        {
            if (span.Length == 0)
            {
                this = Void;

                return;
            }

            unsafe
            {
                var length = span.Length;

                _buffer = (TUnmanaged*)NativeMemory.Alloc((nuint)length, (nuint)Unsafe.SizeOf<TUnmanaged>());

                fixed (TUnmanaged* pointer = span)
                {
                    Unsafe.CopyBlock(_buffer, pointer, (uint)(length * Unsafe.SizeOf<TUnmanaged>()));
                }

                _capacity = _count = length;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList(int capacity)
        {
            if (capacity <= 0)
            {
                this = Void;

                return;
            }

            unsafe
            {
                _buffer = (TUnmanaged*)NativeMemory.Alloc((nuint)capacity, (nuint)Unsafe.SizeOf<TUnmanaged>());
                _capacity = capacity;
                _count = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeList()
        {
            unsafe
            {
                _buffer = (TUnmanaged*)Unsafe.AsPointer(ref Unsafe.NullRef<TUnmanaged>());
                _capacity = _count = 0;
            }
        }

        public readonly TUnmanaged this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_capacity)
                    ThrowHelpers.IndexOutOfRangeException();

                unsafe
                {
                    return _buffer[index];
                }
            }
            set
            {
                if ((uint)index >= (uint)_capacity)
                    ThrowHelpers.IndexOutOfRangeException();
                
                unsafe
                {
                    _buffer[index] = value;
                }
            }
        }

        public void Add(in TUnmanaged item)
        {
            if ((uint)_count >= (uint)_capacity)
                ThrowHelpers.IndexOutOfRangeException();

            unsafe
            {
                _buffer[_count] = item;
                _count += 1;
            }
        }

        public bool TryAdd(in TUnmanaged item)
        {
            if ((uint)_count >= (uint)_capacity)
                return false;

            unsafe
            {
                _buffer[_count] = item;
                _count += 1;

                return true;
            }
        }

        public bool TryAddRange(in ReadOnlySpan<TUnmanaged> items, bool allOrNothing = true)
        {
            if (items.Length == 0)
                return false;

            var itemsLength = items.Length;
            var emptySpace = _capacity - _count;

            if ((uint)_count >= (uint)_capacity || (allOrNothing && itemsLength > emptySpace))
                return false;

            if (!allOrNothing && itemsLength >= emptySpace)
            {
                itemsLength = emptySpace;
            }

            unsafe
            {
                fixed (TUnmanaged* pointer = items)
                {
                    Unsafe.CopyBlock(&_buffer[_count], pointer, (uint)(itemsLength * Unsafe.SizeOf<TUnmanaged>()));
                }

                _count += itemsLength;

                return true;
            }
        }

        public readonly bool TryGet(int index, out TUnmanaged item)
        {
            if ((uint)index >= (uint)_capacity)
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

        public readonly bool TryGetRange(
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeReadOnlyCollection<TUnmanaged> AsReadOnly()
        {
            unsafe { return new NativeReadOnlyCollection<TUnmanaged>(in _buffer, in _count); }
        }

        public void Dispose()
        {
            unsafe
            {
                if (Equals(Void))
                    return;

                NativeMemory.Free(_buffer);
                this = Void;
            }
        }

        public readonly NativeEnumerator<TUnmanaged> GetEnumerator()
        {
            unsafe { return new NativeEnumerator<TUnmanaged>(in _buffer, in _count); }
        }

        public readonly ref TUnmanaged GetPinnableReference()
        {
            if (_count != 0)
            {
                unsafe { return ref _buffer[0]; }
            }

            return ref Unsafe.NullRef<TUnmanaged>();
        }

        public bool Equals(NativeList<TUnmanaged> other)
        {
            unsafe
            {
                return _buffer == other._buffer
                       && _capacity == other._capacity
                       && _count == other._count
                       && GetHashCode() == other.GetHashCode();
            }
        }

        public override bool Equals(object? obj) => obj is NativeList<TUnmanaged> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                var hash = hashingBase;

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(NativeList<TUnmanaged>))
                           ? typeof(NativeList<TUnmanaged>).GetHashCode()
                           : 0);

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(TUnmanaged)) ? typeof(TUnmanaged).GetHashCode() : 0);

                unsafe
                {
                    hash = (hash * hashingMultiplier) ^ new IntPtr(_buffer).GetHashCode();
                }

                hash = (hash * hashingMultiplier) ^ _capacity.GetHashCode();
                hash = (hash * hashingMultiplier) ^ _count.GetHashCode();

                return hash;
            }
        }

        public readonly override string ToString() =>
            $"{nameof(NativeList<TUnmanaged>)}<{typeof(TUnmanaged).Name}>[Count: {_count} | Capacity: {_capacity}]";

        public static bool operator ==(NativeList<TUnmanaged> lhs, NativeList<TUnmanaged> rhs) => lhs.Equals(rhs);

        public static bool operator !=(NativeList<TUnmanaged> lhs, NativeList<TUnmanaged> rhs) => !(lhs == rhs);
    }
}