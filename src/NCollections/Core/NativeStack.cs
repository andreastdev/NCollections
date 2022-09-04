using NCollections.Internal;

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NCollections.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NativeStack<TUnmanaged> : IEquatable<NativeStack<TUnmanaged>>, IDisposable
        where TUnmanaged : unmanaged
    {
        private readonly unsafe TUnmanaged* _buffer;
        private readonly int _capacity;
        private int _count;

        public static NativeStack<TUnmanaged> Void { get; } = new();

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

        public NativeStack(in ReadOnlySpan<TUnmanaged> span)
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
                    Unsafe.CopyBlock(
                        _buffer, 
                        pointer,
                        (uint)(length * Unsafe.SizeOf<TUnmanaged>()));
                }

                _capacity = _count = length;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStack(int capacity)
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
                _count = 0;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NativeStack()
        {
            unsafe
            {
                _buffer = (TUnmanaged*)Unsafe.AsPointer(ref Unsafe.NullRef<TUnmanaged>());
                _capacity = _count = 0;
            }
        }

        public void Push(in TUnmanaged item)
        {
            if ((uint)_count >= (uint)_capacity)
                ThrowHelpers.IndexOutOfRangeException();

            unsafe
            {
                _buffer[_count] = item;
                _count += 1;
            }
        }

        public bool TryPush(in TUnmanaged item)
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
        
        public ref TUnmanaged Pop()
        {
            if (_count == 0)
                ThrowHelpers.InvalidOperationException();

            unsafe
            {
                var popIndex = _count - 1;
                _count--;
                
                return ref _buffer[popIndex];
            }
        }

        public bool TryPop(out TUnmanaged item)
        {
            if (_count == 0)
            {
                item = default;

                return false;
            }

            unsafe
            {
                item = _buffer[_count - 1];
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
                    return ref _buffer[_count - 1];
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
                    item = _buffer[_count - 1];

                    return true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly NativeReadOnlyCollection<TUnmanaged> AsReadOnly()
        {
            unsafe { return new NativeReadOnlyCollection<TUnmanaged>(in _buffer, in _count); }
        }
        
        public readonly NativeEnumerator<TUnmanaged> GetEnumerator()
        {
            unsafe { return new NativeEnumerator<TUnmanaged>(in _buffer, in _count); }
        }

        public readonly ref TUnmanaged GetPinnableReference()
        {
            if (_count != 0)
            {
                unsafe { return ref _buffer[0];}
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

        public bool Equals(NativeStack<TUnmanaged> other)
        {
            unsafe
            {
                return _buffer == other._buffer
                       && _capacity == other._capacity
                       && _count == other._count
                       && GetHashCode() == other.GetHashCode();
            }
        }
        
        public override bool Equals(object? obj) => obj is NativeStack<TUnmanaged> other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                const int hashingBase = (int)2166136261;
                const int hashingMultiplier = 16777619;

                var hash = hashingBase;

                hash = (hash * hashingMultiplier)
                       ^ (!ReferenceEquals(null, typeof(NativeStack<TUnmanaged>)) ? typeof(NativeStack<TUnmanaged>).GetHashCode() : 0);

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
            $"{nameof(NativeStack<TUnmanaged>)}<{typeof(TUnmanaged).Name}>[Count: {_count} | Capacity: {_capacity}]";
        
        public static bool operator ==(NativeStack<TUnmanaged> lhs, NativeStack<TUnmanaged> rhs) => lhs.Equals(rhs);

        public static bool operator !=(NativeStack<TUnmanaged> lhs, NativeStack<TUnmanaged> rhs) => !(lhs == rhs);
    }
}