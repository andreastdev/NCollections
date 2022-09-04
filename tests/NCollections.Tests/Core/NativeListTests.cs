using NCollections.Core;

using System;

using Xunit;

namespace NCollections.Tests.Core
{
    public sealed class NativeListTests : BaseTests
    {
        private const int Limit = 1000;

        private NativeList<int> _sut;

        public override void Dispose()
        {
            base.Dispose();

            _sut.Dispose();
        }

        [Fact]
        public void Constructor_InitialiseDefault_ShouldBeVoid()
        {
            _sut = new NativeList<int>();

            Assert.Equal(NativeList<int>.Void, _sut);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Constructor_InitialiseWithCapacity_ShouldBeEmptyOrVoid(int capacity)
        {
            _sut = new NativeList<int>(capacity);

            if (capacity <= 0)
            {
                Assert.True(_sut.Equals(NativeList<int>.Void));
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
            _sut = new NativeList<int>(array);

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
                    Assert.Equal(array[i], _sut[i]);
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

            _sut = new NativeList<int>(capacity);

            for (var i = 0; i < count; i++)
            {
                _sut.Add(DataGenerator.GetRandomNumber());
            }

            Assert.Equal(count * sizeof(int), _sut.CurrentByteCount);
            Assert.Equal(capacity * sizeof(int), _sut.ByteCapacity);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Indexer_GetIndexGreaterThanCapacity_ShouldThrow(int capacity)
        {
            var additional = DataGenerator.GetRandomNumber(1, int.MaxValue - capacity);

            _sut = new NativeList<int>(capacity);

            Assert.Throws<IndexOutOfRangeException>(() => _sut[capacity + additional]);
        }
        
        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void Indexer_SetIndexWithArrayValue_ShouldMatchArray(int[] array)
        {
            _sut = new NativeList<int>(array.Length);

            for (var i = 0; i < array.Length; i++)
            {
                _sut[i] = array[i];
            }
            
            for (var i = 0; i < array.Length; i++)
            {
                Assert.Equal(array[i], _sut[i]);
            }
        }
        
        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Indexer_SetIndexGreaterThanCapacity_ShouldThrow(int capacity)
        {
            var additional = DataGenerator.GetRandomNumber(1, int.MaxValue - capacity);

            _sut = new NativeList<int>(capacity);

            Assert.Throws<IndexOutOfRangeException>(() => _sut[capacity + additional] = DataGenerator.GetRandomNumber());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void Add_FullNativeList_ShouldThrow(int capacity)
        {
            _sut = new NativeList<int>(capacity);

            for (var i = 0; i < capacity; i++)
            {
                _sut.Add(DataGenerator.GetRandomNumber());
            }

            Assert.True(_sut.IsFull);
            Assert.Throws<IndexOutOfRangeException>(() => _sut.Add(DataGenerator.GetRandomNumber()));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void TryAdd_AddItemToNativeList_ShouldReturnTrueIfNotFullOtherwiseFalse(int capacity)
        {
            _sut = new NativeList<int>(capacity);

            for (var i = 0; i < capacity; i++)
            {
                Assert.True(_sut.TryAdd(DataGenerator.GetRandomNumber()));
            }

            Assert.True(_sut.IsFull);
            Assert.False(_sut.TryAdd(DataGenerator.GetRandomNumber()));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryAddRange_AddingItemsWhenEnoughSpace_ShouldAddAllAndReturnTrue(int[] array)
        {
            _sut = new NativeList<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));

            Assert.True(_sut.TryAddRange(array));

            for (var i = 0; i < _sut.Count; i++)
            {
                Assert.Equal(array[i], _sut[i]);
            }

            Assert.Equal(array.Length, _sut.Count);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryAddRange_AddingItemsWhenNotEnoughSpace_ShouldNotAddAndReturnFalse(int[] array)
        {
            _sut = new NativeList<int>(array.Length - DataGenerator.GetRandomNumber(1, array.Length));

            Assert.False(_sut.TryAddRange(array));
            Assert.Equal(0, _sut.Count);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryAddRange_PartiallyAddingItemsWhenNotEnoughSpace_ShouldAddSomeAndReturnTrue(int[] array)
        {
            _sut = new NativeList<int>(array.Length - DataGenerator.GetRandomNumber(1, array.Length));

            if (array.Length == 1)
            {
                Assert.False(_sut.TryAddRange(array, false));
            }
            else
            {
                Assert.True(_sut.TryAddRange(array, false));

                for (var i = 0; i < _sut.Count; i++)
                {
                    Assert.Equal(array[i], _sut[i]);
                }

                Assert.True(_sut.IsFull);
            }
        }

        [Fact]
        public void TryAddRange_AddingNullArray_ShouldReturn()
        {
            _sut = new NativeList<int>(DataGenerator.GetRandomNumber(1, Limit));

            Assert.False(_sut.TryAddRange(null));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryGet_GetItemFromValidAndInvalidIndices_ShouldReturnTrueAndCorrectValue(int[] array)
        {
            _sut = new NativeList<int>(array);

            var validIndex = DataGenerator.GetRandomNumber(0, array.Length);
            var invalidIndex = array.Length + DataGenerator.GetRandomNumber(0, int.MaxValue - array.Length);

            Assert.True(_sut.TryGet(validIndex, out var item1));
            Assert.Equal(item1, _sut[validIndex]);

            Assert.False(_sut.TryGet(invalidIndex, out var item2));
            Assert.Equal(default, item2);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryGetRange_GetValidRangeOfItemsFromSomeIndexOfFullList_ShouldReturnCorrectBooleanAndSpan(
            int[] array)
        {
            _sut = new NativeList<int>(array);

            var randomIndex = DataGenerator.GetRandomNumber(1, _sut.Count);
            var startIndex = _sut.Count / randomIndex;
            var length = randomIndex - startIndex;

            if (array.Length == 1)
            {
                Assert.False(_sut.TryGetRange(startIndex, length, out var result));
                Assert.True(result == ReadOnlySpan<int>.Empty);
            }
            else
            {
                Assert.True(_sut.TryGetRange(startIndex, length, out var result));
                Assert.Equal(length, result.Length);

                for (var i = 0; i < result.Length; i++)
                {
                    Assert.Equal(_sut[startIndex + i], result[i]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryGetRange_GetInvalidRangeOfItemsFromSomeIndexOfFullList_ShouldReturnFalseAndOutEmptyReadOnlySpan(
            int[] array)
        {
            _sut = new NativeList<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));
            _sut.TryAddRange(array);

            var randomIndex = DataGenerator.GetRandomNumber(1, _sut.Count);
            var startIndex = _sut.Count / randomIndex;
            var lengthLargerThanListCount = randomIndex + array.Length;

            Assert.False(_sut.TryGetRange(startIndex, lengthLargerThanListCount, out var result));
            Assert.True(result == ReadOnlySpan<int>.Empty);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryGetRange_PartiallyGetRangeOfItemsFromSomeIndexOfList_ShouldReturnCorrectBooleanAndSpan(
            int[] array)
        {
            _sut = new NativeList<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));
            _sut.TryAddRange(array);

            var randomIndex = DataGenerator.GetRandomNumber(1, _sut.Count);
            var startIndex = _sut.Count / randomIndex;
            var lengthLargerThanListCount = randomIndex + array.Length;

            if (array.Length == 1)
            {
                Assert.False(_sut.TryGetRange(startIndex, lengthLargerThanListCount, out var result, false));
                Assert.True(result == ReadOnlySpan<int>.Empty);
            }
            else
            {
                Assert.True(_sut.TryGetRange(startIndex, lengthLargerThanListCount, out var result, false));

                Assert.Equal(array.Length - startIndex, result.Length);

                for (var i = 0; i < result.Length; i++)
                {
                    Assert.Equal(array[startIndex + i], result[i]);
                }
            }
        }

        [Fact]
        public void AsReadOnly_TransformNativeList_ReturnReadOnlyNativeCollection()
        {
            _sut = new NativeList<int>();

            Assert.Equal(typeof(NativeReadOnlyCollection<int>), _sut.AsReadOnly().GetType());
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder(int[] array)
        {
            _sut = new NativeList<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));
            _sut.TryAddRange(array);

            var count = 0;

            foreach (var item in _sut)
            {
                Assert.Equal(_sut[count], item);
                count++;
            }

            Assert.Equal(count, _sut.Count);
            Assert.NotEqual(count, _sut.Capacity);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetPinnableReference_FixedBlockForFullNativeList_ShouldGetRefToFirstElement(int[] array)
        {
            _sut = new NativeList<int>(array);

            unsafe
            {
                fixed (int* pointer = _sut)
                {
                    Assert.Equal(*pointer, _sut[0]);
                }
            }
        }

        [Fact]
        public void GetPinnableReference_FixedBlockForVoidNativeList_ShouldReturnZeroPointer()
        {
            _sut = NativeList<int>.Void;

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
            _sut = new NativeList<int>(array.Length + DataGenerator.GetRandomNumber(1, Limit));
            _sut.TryAddRange(array);

            var tempList = _sut;

            Assert.True(_sut == tempList);
            Assert.True(_sut.Equals(tempList));

            tempList.Add(DataGenerator.GetRandomNumber());

            Assert.True(_sut != tempList);

            Assert.False(_sut.Equals(new object()));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbersNonZero), Limit, MemberType = typeof(DataGenerator))]
        public void ToString_CreateNativeList_ShouldContainNameGenericTypeCountAndCapacity(int capacity)
        {
            _sut = new NativeList<int>(capacity);
            _sut.Add(DataGenerator.GetRandomNumber());

            var toString = _sut.ToString();

            Assert.Contains(nameof(NativeList<int>), toString);
            Assert.Contains(nameof(Int32), toString);
            Assert.Contains(_sut.Count.ToString(), toString);
            Assert.Contains(_sut.Capacity.ToString(), toString);
        }
    }
}