using NCollections.Core;

using System;
using System.Linq;

using Xunit;
using Xunit.Abstractions;

namespace NCollections.Tests.Core
{
    public sealed class NativeQueueTests : BaseTests
    {
        private const int Limit = 1000;

        private NativeQueue<int> _sut;

        private readonly ITestOutputHelper _output;

        public NativeQueueTests(ITestOutputHelper output) => _output = output;

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
                Assert.Equal(0, _sut.Count);
                Assert.True(_sut.IsEmpty);
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

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void Dequeue_RemoveOneItem_ShouldReturnItemAndQueueCountMinusOne(int[] array)
        {
            _sut = new NativeQueue<int>(array);

            if (_sut.IsEmpty)
            {
                Assert.Throws<InvalidOperationException>(() => _sut.Dequeue());
            }
            else
            {
                Assert.Equal(array.Length, _sut.Count);
                Assert.Equal(array[0], _sut.Dequeue());
                Assert.Equal(array.Length - 1, _sut.Count);
            }
        }

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryDequeue_RemoveOneItem_ShouldReturnBooleanAndOutItemAndQueueCountMinusOne(int[] array)
        {
            _sut = new NativeQueue<int>(array);

            if (_sut.IsEmpty)
            {
                Assert.False(_sut.TryDequeue(out var item));
                Assert.Equal(default, item);
            }
            else
            {
                Assert.Equal(array.Length, _sut.Count);
                Assert.True(_sut.TryDequeue(out var item));
                Assert.Equal(array[0], item);
                Assert.Equal(array.Length - 1, _sut.Count);
            }
        }

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void Peek_CheckFirstElement_ShouldReturnElementAndQueueCountIntact(int[] array)
        {
            _sut = new NativeQueue<int>(array);

            if (_sut.IsEmpty)
            {
                Assert.Throws<InvalidOperationException>(() => _sut.Peek());
            }
            else
            {
                Assert.Equal(array.Length, _sut.Count);
                Assert.Equal(array[0], _sut.Peek());
                Assert.Equal(array.Length, _sut.Count);
            }
        }

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryPeek_CheckFirstElement_ShouldReturnBooleanAndOutItemAndQueueCountIntact(int[] array)
        {
            _sut = new NativeQueue<int>(array);

            if (_sut.IsEmpty)
            {
                Assert.False(_sut.TryPeek(out var item));
                Assert.Equal(default, item);
            }
            else
            {
                Assert.Equal(array.Length, _sut.Count);
                Assert.True(_sut.TryPeek(out var item));
                Assert.Equal(array[0], item);
                Assert.Equal(array.Length, _sut.Count);
            }
        }

        [Fact]
        public void Calibrate_UseEmptyQueue_ReturnTheSameQueue()
        {
            _sut = new NativeQueue<int>();
            var sutCopy = _sut;

            sutCopy.Calibrate();

            Assert.Equal(_sut, sutCopy);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void Calibrate_UseQueueWithOneElementAtRandomIndex_ReturnTheSameQueue(int capacity)
        {
            _sut = new NativeQueue<int>(capacity);

            var enqueueCount = DataGenerator.GetRandomNumber(1, capacity - 1);

            for (var i = 0; i < enqueueCount - 1; i++)
            {
                _sut.Enqueue(DataGenerator.GetRandomNumber());
            }

            var lastAddition = DataGenerator.GetRandomNumber();
            _sut.Enqueue(lastAddition);

            var dequeueCount = enqueueCount - 1;

            for (var i = 0; i < dequeueCount; i++)
            {
                _sut.Dequeue();
            }

            Assert.Equal(1, _sut.Count);

            _sut.Calibrate();

            Assert.Equal(1, _sut.Count);
            Assert.Equal(lastAddition, _sut.Peek());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void Calibrate_UseQueueWithManyElementsAtRandomIndexAndStartIndexLessThanEndIndex_ReturnTheSameQueue(
            int capacity)
        {
            _sut = new NativeQueue<int>(capacity);

            var lastAdditions = DataGenerator.GenerateRandomArray(DataGenerator.GetRandomNumber(1, capacity - 2));
            var initialCount = DataGenerator.GetRandomNumber(1, capacity - lastAdditions.Length);

            for (var i = 0; i < initialCount; i++)
            {
                _sut.Enqueue(DataGenerator.GetRandomNumber());
            }

            foreach (var lastAddition in lastAdditions)
            {
                _sut.Enqueue(lastAddition);
            }

            for (var i = 0; i < initialCount; i++)
            {
                _sut.Dequeue();
            }

            Assert.Equal(lastAdditions.Length, _sut.Count);

            var count = 0;
            foreach (var item in _sut)
            {
                Assert.NotEqual(item, lastAdditions[count]);
                count++;
            }

            _sut.Calibrate();

            Assert.Equal(lastAdditions.Length, _sut.Count);

            count = 0;
            foreach (var item in _sut)
            {
                Assert.Equal(item, lastAdditions[count]);
                count++;
            }
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void Calibrate_UseQueueWithManyElementsAtRandomIndexAndStartIndexGreaterThanEndIndex_ReturnTheSameQueue(
            int capacity)
        {
            _sut = new NativeQueue<int>(capacity);
            var aboveMiddleBelowCapacity = DataGenerator.GetRandomNumber(
                (int)Math.Ceiling(capacity * 0.5f) + 1,
                capacity - 1);

            var items = DataGenerator.GenerateRandomArray(aboveMiddleBelowCapacity);

            foreach (var item in items)
            {
                _sut.Enqueue(item);
            }

            Assert.Equal(items.Length, _sut.Count);

            for (var i = 0; i < items.Length; i++)
            {
                _sut.Dequeue();
            }

            Assert.True(_sut.IsEmpty);

            foreach (var item in items)
            {
                _sut.Enqueue(item);
            }

            Assert.Equal(items.Length, _sut.Count);

            unsafe
            {
                fixed (int* ptr = _sut)
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        Assert.NotEqual(items[i], ptr[i]);
                    }
                }
            }

            _sut.Calibrate();

            Assert.Equal(items.Length, _sut.Count);

            unsafe
            {
                fixed (int* ptr = _sut)
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        Assert.Equal(items[i], ptr[i]);
                    }
                }
            }
        }

        [Fact]
        public void AsReadOnly_TransformNativeQueue_ReturnReadOnlyNativeCollection()
        {
            _sut = new NativeQueue<int>();

            Assert.Equal(typeof(NativeReadOnlyCollection<int>), _sut.AsReadOnly().GetType());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder(int[] array)
        {
            _sut = new NativeQueue<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));

            for (var i = 0; i < array.Length; i++)
            {
                _sut.TryEnqueue(array[i]);
            }

            var count = 0;

            foreach (var item in _sut)
            {
                Assert.Equal(array[count], item);
                count++;
            }

            Assert.Equal(count, _sut.Count);
            Assert.NotEqual(count, _sut.Capacity);
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
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void Equality_UseEqualsAndOperators_ShouldReturnExpectedResults(int[] array)
        {
            _sut = new NativeQueue<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));

            for (var i = 0; i < array.Length; i++)
            {
                _sut.TryEnqueue(array[i]);
            }

            var tempQueue = _sut;

            Assert.True(_sut == tempQueue);
            Assert.True(_sut.Equals(tempQueue));

            tempQueue.Enqueue(DataGenerator.GetRandomNumber());

            Assert.True(_sut != tempQueue);

            Assert.False(_sut.Equals(new object()));
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