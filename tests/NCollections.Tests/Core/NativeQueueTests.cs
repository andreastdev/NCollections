using NCollections.Core;

using System;

using Xunit;

namespace NCollections.Tests.Core
{
    public sealed class NativeQueueTests : BaseTests
    {
        private const int Limit = 1000;

        private NativeQueue<int> _sut;

        public override void Dispose()
        {
            base.Dispose();

            _sut.Dispose();
        }

        [Fact]
        public void Constructor_InitialiseDefault_ShouldBeVoid()
        {
            _sut = new NativeQueue<int>();

            Assert.Equal(NativeQueue<int>.Void, _sut);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Constructor_InitialiseWithCapacity_ShouldBeEmptyOrVoid(int capacity)
        {
            _sut = new NativeQueue<int>(capacity);

            if (capacity <= 0)
            {
                Assert.True(_sut.Equals(NativeQueue<int>.Void));
            }
            else
            {
                Assert.Equal(capacity, _sut.Capacity);
                Assert.Equal(0, _sut.Count);
                Assert.True(_sut.IsEmpty);
            }
        }

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void Constructor_InitialiseWithArrays_ShouldBeAbleToCreateNativeList(int[]? array)
        {
            _sut = new NativeQueue<int>(array);

            if (array is null)
            {
                Assert.True(_sut.IsEmpty);
                Assert.Equal(0, _sut.Capacity);
                Assert.Equal(0, _sut.Count);
            }
            else
            {
                for (var i = 0; i < array.Length; i++)
                {
                    Assert.Equal(array[i], _sut.Dequeue());
                }

                Assert.Equal(array.Length, _sut.Capacity);
                Assert.Equal(array.Length, _sut.Count);
                Assert.True(_sut.IsFull);
            }
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void ByteSize_AddLessItemsThanCapacity_ShouldReturnCurrentByteCountAndByteCapacity(int capacity)
        {
            var count = DataGenerator.GetRandomNumber(0, capacity);

            _sut = new NativeQueue<int>(capacity);

            for (var i = 0; i < count; i++)
            {
                _sut.Enqueue(DataGenerator.GetRandomNumber());
            }

            Assert.Equal(count * sizeof(int), _sut.CurrentByteCount);
            Assert.Equal(capacity * sizeof(int), _sut.ByteCapacity);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void Enqueue_FullNativeQueue_ShouldThrow(int capacity)
        {
            _sut = new NativeQueue<int>(capacity);

            for (var i = 0; i < capacity; i++)
            {
                _sut.Enqueue(DataGenerator.GetRandomNumber());
            }

            Assert.True(_sut.IsFull);
            Assert.Throws<IndexOutOfRangeException>(() => _sut.Enqueue(DataGenerator.GetRandomNumber()));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void TryEnqueue_AddItemToNativeQueue_ShouldReturnTrueIfNotFullOtherwiseFalse(int capacity)
        {
            _sut = new NativeQueue<int>(capacity);

            for (var i = 0; i < capacity; i++)
            {
                Assert.True(_sut.TryEnqueue(DataGenerator.GetRandomNumber()));
            }

            Assert.True(_sut.IsFull);
            Assert.False(_sut.TryEnqueue(DataGenerator.GetRandomNumber()));
        }

        [Fact]
        public void AsReadOnly_TransformNativeQueue_ReturnReadOnlyNativeCollection()
        {
            _sut = new NativeQueue<int>();

            Assert.Equal(typeof(NativeReadOnlyCollection<int>), _sut.AsReadOnly().GetType());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetPinnableReference_FixedBlockForFullNativeQueue_ShouldGetRefToFirstElement(int[] array)
        {
            _sut = new NativeQueue<int>(array);

            unsafe
            {
                fixed (int* pointer = _sut)
                {
                    Assert.Equal(*pointer, _sut.Peek());
                }
            }
        }

        [Fact]
        public void GetPinnableReference_FixedBlockForVoidNativeQueue_ShouldReturnZeroPointer()
        {
            _sut = NativeQueue<int>.Void;

            unsafe
            {
                fixed (int* pointer = _sut)
                {
                    Assert.True(pointer == IntPtr.Zero.ToPointer());
                }
            }
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void ToString_CreateNativeQueue_ShouldContainNameGenericTypeCountAndCapacity(int capacity)
        {
            _sut = new NativeQueue<int>(capacity);
            _sut.Enqueue(DataGenerator.GetRandomNumber());

            var toString = _sut.ToString();

            Assert.Contains(nameof(NativeQueue<int>), toString);
            Assert.Contains(nameof(Int32), toString);
            Assert.Contains(_sut.Count.ToString(), toString);
            Assert.Contains(_sut.Capacity.ToString(), toString);
        }
    }
}