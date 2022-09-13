using NCollections.Core;

using System;

using Xunit;

namespace NCollections.Tests.Core
{
    public sealed class NativeStackTests : BaseTests
    {
        private const int Limit = 1000;

        private NativeStack<int> _sut;

        public override void Dispose()
        {
            base.Dispose();

            _sut.Dispose();
        }

        [Fact]
        public void Constructor_InitialiseDefault_ShouldBeVoid()
        {
            _sut = new NativeStack<int>();

            Assert.Equal(NativeStack<int>.Void, _sut);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Constructor_InitialiseWithCapacity_ShouldBeEmptyOrVoid(int capacity)
        {
            _sut = new NativeStack<int>(capacity);

            if (capacity <= 0)
            {
                Assert.True(_sut.Equals(NativeStack<int>.Void));
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
            _sut = new NativeStack<int>(array);

            if (array is null)
            {
                Assert.True(_sut.IsEmpty);
                Assert.Equal(0, _sut.Capacity);
                Assert.Equal(0, _sut.Count);
            }
            else
            {
                Assert.True(_sut.IsFull);
                
                for (var i = array.Length - 1; i >= 0; i--)
                {
                    Assert.Equal(array[i], _sut.Pop());
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

            _sut = new NativeStack<int>(capacity);

            for (var i = 0; i < count; i++)
            {
                _sut.Push(DataGenerator.GetRandomNumber());
            }

            Assert.Equal(count * sizeof(int), _sut.CurrentByteCount);
            Assert.Equal(capacity * sizeof(int), _sut.ByteCapacity);
        }
        
         [Fact]
        public void AsReadOnly_TransformNativeStack_ReturnReadOnlyNativeCollection()
        {
            _sut = new NativeStack<int>();

            Assert.Equal(typeof(NativeReadOnlyCollection<int>), _sut.AsReadOnly().GetType());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder(int[] array)
        {
            _sut = new NativeStack<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));

            for (var i = 0; i < array.Length; i++)
            {
                _sut.TryPush(array[i]);
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
        public void GetPinnableReference_FixedBlockForFullNativeStack_ShouldGetRefToFirstElement(int[] array)
        {
            _sut = new NativeStack<int>(array);

            unsafe
            {
                fixed (int* pointer = _sut)
                {
                    Assert.Equal(*pointer, _sut.Peek());
                }
            }
        }

        [Fact]
        public void GetPinnableReference_FixedBlockForVoidNativeStack_ShouldReturnZeroPointer()
        {
            _sut = NativeStack<int>.Void;

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
            _sut = new NativeStack<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));

            for (var i = 0; i < array.Length; i++)
            {
                _sut.TryPush(array[i]);
            }

            var tempStack = _sut;

            Assert.True(_sut == tempStack);
            Assert.True(_sut.Equals(tempStack));

            tempStack.Push(DataGenerator.GetRandomNumber());

            Assert.True(_sut != tempStack);

            Assert.False(_sut.Equals(new object()));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void ToString_CreateNativeStack_ShouldContainNameGenericTypeCountAndCapacity(int capacity)
        {
            _sut = new NativeStack<int>(capacity);
            _sut.Push(DataGenerator.GetRandomNumber());

            var toString = _sut.ToString();

            Assert.Contains(nameof(NativeStack<int>), toString);
            Assert.Contains(nameof(Int32), toString);
            Assert.Contains(_sut.Count.ToString(), toString);
            Assert.Contains(_sut.Capacity.ToString(), toString);
        }
    }
}