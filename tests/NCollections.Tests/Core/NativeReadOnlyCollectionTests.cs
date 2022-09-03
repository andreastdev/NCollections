using NCollections.Core;

using System;

using Xunit;

namespace NCollections.Tests.Core
{
    public sealed class NativeReadOnlyCollectionTests : BaseTests
    {
        private const int Limit = 1000;

        private NativeReadOnlyCollection<int> _sut;

        public override void Dispose()
        {
            base.Dispose();

            _sut.Dispose();
        }

        [Fact]
        public void Constructor_InitialiseDefault_ShouldBeVoid()
        {
            _sut = new NativeReadOnlyCollection<int>();

            Assert.Equal(NativeReadOnlyCollection<int>.Void, _sut);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Constructor_InitialiseFromNativeListWithCapacity_ShouldBeEmptyOrVoid(int capacity)
        {
            _sut = new NativeList<int>(capacity).AsReadOnly();

            if (capacity <= 0)
            {
                Assert.True(_sut.Equals(NativeReadOnlyCollection<int>.Void));
            }
            else
            {
                Assert.Equal(0, _sut.Count);
            }
        }

        [Theory]
        [InlineData(null)]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void Constructor_InitialiseFromNativeListWithArrays_ShouldBeAbleToCreateReadOnlyNativeList(int[]? array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            if (array is null)
            {
                Assert.True(_sut.Equals(NativeReadOnlyCollection<int>.Void));
                Assert.Equal(0, _sut.Count);
            }
            else
            {
                for (var i = 0; i < array.Length; i++)
                {
                    Assert.Equal(array[i], _sut[i]);
                }

                Assert.Equal(array.Length, _sut.Count);
            }
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void ByteSize_ArrayOfInts_ShouldReturnCurrentByteCount(int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            Assert.Equal(_sut.Count * sizeof(int), _sut.ByteCount);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetPositiveNumbers), Limit, MemberType = typeof(DataGenerator))]
        public void Indexer_UseIndexGreaterThanCapacity_ShouldThrow(int capacity)
        {
            var additional = DataGenerator.GetRandomNumber(1, int.MaxValue - capacity);

            _sut = new NativeList<int>(capacity).AsReadOnly();

            Assert.Throws<IndexOutOfRangeException>(() => _sut[capacity + additional]);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryGet_GetItemFromValidAndInvalidIndices_ShouldReturnCorrectValue(int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            var validIndex = DataGenerator.GetRandomNumber(0, array.Length);
            var invalidIndex = array.Length + DataGenerator.GetRandomNumber(0, int.MaxValue - array.Length);

            Assert.True(_sut.TryGet(validIndex, out var item1));
            Assert.Equal(item1, _sut[validIndex]);

            Assert.False(_sut.TryGet(invalidIndex, out var item2));
            Assert.Equal(default, item2);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void TryGetRange_GetValidRangeOfItemsFromSomeIndexOfCollection_ShouldReturnCorrectBooleanAndSpan(
            int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

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
        public void TryGetRange_GetLargerRangeOfItemsFromSomeIndexOfCollection_ShouldReturnCorrectBooleanAndSpan(
            int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            var randomIndex = DataGenerator.GetRandomNumber(1, _sut.Count);
            var startIndex = _sut.Count / randomIndex;
            var lengthLargerThanCollectionCount = randomIndex + array.Length;

            Assert.False(_sut.TryGetRange(startIndex, lengthLargerThanCollectionCount, out var result));
            Assert.True(result == ReadOnlySpan<int>.Empty);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void
            TryGetRange_PartiallyGetLargerRangeOfItemsFromSomeIndexOfCollection_ShouldReturnCorrectBooleanAndSpan(
                int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            var randomIndex = DataGenerator.GetRandomNumber(1, _sut.Count);
            var startIndex = _sut.Count / randomIndex;
            var lengthLargerThanCollectionCount = randomIndex + array.Length;

            if (array.Length == 1)
            {
                Assert.False(_sut.TryGetRange(startIndex, lengthLargerThanCollectionCount, out var result, false));
                Assert.True(result == ReadOnlySpan<int>.Empty);
            }
            else
            {
                Assert.True(_sut.TryGetRange(startIndex, lengthLargerThanCollectionCount, out var result, false));

                Assert.Equal(array.Length - startIndex, result.Length);

                for (var i = 0; i < result.Length; i++)
                {
                    Assert.Equal(array[startIndex + i], result[i]);
                }
            }
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder(int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            var count = 0;

            foreach (var item in _sut)
            {
                Assert.Equal(_sut[count], item);
                count++;
            }

            Assert.Equal(count, _sut.Count);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void GetPinnableReference_FixedBlockForFullNativeList_ShouldGetRefToFirstElement(int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

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
            _sut = NativeReadOnlyCollection<int>.Void;

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
            var randomNumber = DataGenerator.GetRandomNumber(1, Limit);
            NativeList<int> temp1 = new(array.Length + randomNumber);
            temp1.TryAddRange(array);

            _sut = temp1.AsReadOnly();
            var tempReadOnly1 = _sut;

            Assert.True(_sut == tempReadOnly1);
            Assert.True(_sut.Equals(tempReadOnly1));

            NativeList<int> temp2 = new(array.Length + randomNumber);
            temp2.TryAddRange(array);

            var tempReadOnly2 = temp2.AsReadOnly();

            Assert.True(_sut != tempReadOnly2);

            Assert.False(_sut.Equals(new object()));
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void ImplicitOperator_CreateReadOnlyCollectionFromNativeList_ShouldConvertProperly(int[] array)
        {
            NativeList<int> emptyList = new();
            NativeList<int> fullList = new(array);

            NativeList<int> partiallyFilledList =
                new(array.Length + DataGenerator.GetRandomNumber(1, int.MaxValue - array.Length));

            partiallyFilledList.TryAddRange(array);

            _sut = emptyList;
            Assert.Equal(NativeList<int>.Void, _sut);

            _sut = fullList;
            Assert.Equal(fullList, _sut);

            _sut = partiallyFilledList;
            Assert.Equal(partiallyFilledList, _sut);
        }

        [Theory]
        [MemberData(nameof(DataGenerator.GetRandomArrays), Limit, MemberType = typeof(DataGenerator))]
        public void ToString_CreateNativeList_ShouldContainNameGenericTypeCountAndCapacity(int[] array)
        {
            _sut = new NativeList<int>(array).AsReadOnly();

            var toString = _sut.ToString();

            Assert.Contains(nameof(NativeReadOnlyCollection<int>), toString);
            Assert.Contains(nameof(Int32), toString);
            Assert.Contains(_sut.Count.ToString(), toString);
        }
    }
}