using NCollections.Internal;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeQueue<TUnmanaged> : IEquatable<NativeQueue<TUnmanaged>>, IDisposable
        where TUnmanaged : unmanaged
    {
        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _capacity;
        private int _count;
        private int _startIndex;
        private int _endIndex;

        public static NativeQueue<TUnmanaged> Void { get; } = new();

        public readonly int Capacity => _capacity;

        public readonly int Count => _count;

        internal readonly int StartIndex => _startIndex;

        internal readonly int EndIndex => _endIndex;

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

        public NativeQueue(in ReadOnlySpan<TUnmanaged> span)
        {
            if (span.Length == 0)
            {
                this = Void;

                return;
            }

            unsafe
            {
                var length = span.Length;

                _buffer = (TUnmanaged*)NativeMemory.AllocZeroed((nuint)length, (nuint)Unsafe.SizeOf<TUnmanaged>());

                fixed (TUnmanaged* pointer = span)
                {
                    Unsafe.CopyBlock(_buffer, pointer, (uint)(length * Unsafe.SizeOf<TUnmanaged>()));
                }

                _capacity = _count = length;
                _startIndex = 0;
                _endIndex = length - 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeQueue(int capacity)
        {
            if (capacity <= 0)
            {
                this = Void;

                return;
            }

            unsafe
            {
                _buffer = (TUnmanaged*)NativeMemory.AllocZeroed((nuint)capacity, (nuint)Unsafe.SizeOf<TUnmanaged>());
                _capacity = capacity;
                _count = _startIndex = 0;
                _endIndex = -1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeQueue()
        {
            unsafe
            {
                _buffer = (TUnmanaged*)Unsafe.AsPointer(ref Unsafe.NullRef<TUnmanaged>());
                _capacity = _count = _startIndex = 0;
                _endIndex = -1;
            }
        }

        public void Enqueue(in TUnmanaged item)
        {
            if ((uint)_count >= (uint)_capacity)
                ThrowHelpers.IndexOutOfRangeException();

            unsafe
            {
                _buffer[_count] = item;
                _count += 1;

                _endIndex = (uint)_endIndex >= (uint)_capacity - 1 ? 0 : _endIndex + 1;
            }
        }

        public bool TryEnqueue(in TUnmanaged item)
        {
            if ((uint)_count >= (uint)_capacity)
                return false;

            unsafe
            {
                _buffer[_count] = item;
                _count += 1;

                _endIndex = (uint)_endIndex >= (uint)_capacity - 1 ? 0 : _endIndex + 1;

                return true;
            }
        }

        public TUnmanaged Dequeue()
        {
            if (_count == 0)
                ThrowHelpers.InvalidOperationException();

            unsafe
            {
                var output = _buffer[_startIndex];
                _buffer[_startIndex] = default;

                _startIndex = (uint)_startIndex >= (uint)_capacity - 1 ? 0 : _startIndex + 1;
                _count--;

                return output;
            }
        }

        public bool TryDequeue(out TUnmanaged item)
        {
            if (_count == 0)
            {
                item = default;

                return false;
            }

            unsafe
            {
                item = _buffer[_startIndex];
                _buffer[_startIndex] = default;
                
                _startIndex = (uint)_startIndex >= (uint)_capacity - 1 ? 0 : _startIndex + 1;
                _count--;

                return true;
            }
        }

        public readonly ref TUnmanaged Peek()
        {
            {
                if (_count == 0)
                    ThrowHelpers.InvalidOperationException();

                unsafe
                {
                    return ref _buffer[_startIndex];
                }
            }
        }

        public readonly bool TryPeek(out TUnmanaged item)
        {
            {
                if (_count == 0)
                {
                    item = default;

                    return false;
                }

                unsafe
                {
                    item = _buffer[_startIndex];

                    return true;
                }
            }
        }

        public void Calibrate()
        {
            if (_count == 0 || _startIndex == 0)
                return;

            unsafe
            {
                if (_startIndex == _endIndex)
                {
                    _buffer[0] = _buffer[_startIndex];
                }
                else if (_startIndex < _endIndex)
                {
                    Unsafe.CopyBlock(_buffer, &_buffer[_startIndex], (uint)(_count * Unsafe.SizeOf<TUnmanaged>()));
                }

                _startIndex = 0;
                _endIndex = _count - 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeReadOnlyCollection<TUnmanaged> AsReadOnly()
        {
            Calibrate();

            unsafe
            {
                return new NativeReadOnlyCollection<TUnmanaged>(in _buffer, in _count);
            }
        }

        public NativeEnumerator<TUnmanaged> GetEnumerator()
        {
            Calibrate();
            
            unsafe
            {
                return new NativeEnumerator<TUnmanaged>(in _buffer, in _count);
            }
        }

        public readonly ref TUnmanaged GetPinnableReference()
        {
            if (_count != 0)
            {
                unsafe { return ref _buffer[_startIndex]; }
            }

            return ref Unsafe.NullRef<TUnmanaged>();
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

        public bool Equals(NativeQueue<TUnmanaged> other)
        {
            unsafe
            {
                return _buffer == other._buffer
                       && _capacity == other._capacity
                       && _count == other._count
                       && _startIndex == other._startIndex
                       && _endIndex == other._endIndex
                       && GetHashCode() == other.GetHashCode();
            }
        }

        public override bool Equals(object? obj) => obj is NativeQueue<TUnmanaged> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                var hash = hashingBase;

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(NativeQueue<TUnmanaged>))
                           ? typeof(NativeQueue<TUnmanaged>).GetHashCode()
                           : 0);

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(TUnmanaged)) ? typeof(TUnmanaged).GetHashCode() : 0);

                unsafe
                {
                    hash = (hash * hashingMultiplier) ^ new IntPtr(_buffer).GetHashCode();
                }

                hash = (hash * hashingMultiplier) ^ _capacity.GetHashCode();
                hash = (hash * hashingMultiplier) ^ _count.GetHashCode();
                hash = (hash * hashingMultiplier) ^ _startIndex.GetHashCode();
                hash = (hash * hashingMultiplier) ^ _endIndex.GetHashCode();

                return hash;
            }
        }

        public readonly override string ToString() =>
            $"{nameof(NativeQueue<TUnmanaged>)}<{typeof(TUnmanaged).Name}>[Count: {_count} | Capacity: {_capacity}]";

        public static bool operator ==(NativeQueue<TUnmanaged> lhs, NativeQueue<TUnmanaged> rhs) => lhs.Equals(rhs);

        public static bool operator !=(NativeQueue<TUnmanaged> lhs, NativeQueue<TUnmanaged> rhs) => !(lhs == rhs);
    }
}