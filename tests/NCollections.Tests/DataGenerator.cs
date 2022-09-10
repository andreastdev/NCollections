using System;
using System.Collections.Generic;
using System.Linq;

namespace NCollections.Tests
{
    internal sealed class DataGenerator
    {
        public static IEnumerable<object[]> GetRandomNumbers(int upper) =>
            new List<object[]>
            {
                new object[] { 0 },
                new object[] { GetRandomNumber(1, upper) },
                new object[] { -GetRandomNumber(1, upper) }
            };
        
        public static IEnumerable<object[]> GetPositiveNumbers(int upper) =>
            new List<object[]>
            {
                new object[] { 0 },
                new object[] { GetRandomNumber(1, upper) },
                new object[] { GetRandomNumber(1, upper) }
            };

        public static IEnumerable<object[]> GetPositiveNumbersNonZero(int upper) =>
            new List<object[]>
            {
                new object[] { GetRandomNumber(1, upper) },
                new object[] { GetRandomNumber(1, upper) },
                new object[] { GetRandomNumber(1, upper) }
            };
        
        public static IEnumerable<object[]> GetRandomArrays(int upperLimit)
        {
            var randomMiddleLimit = new Random().Next(2, upperLimit);
            return new List<object[]>
            {
                new object[] { GenerateRandomArray(1) },
                new object[] { GenerateRandomArray(randomMiddleLimit) },
                new object[] { GenerateRandomArray(upperLimit) }
            };
        }

        public static int GetRandomNumber(int min = int.MinValue, int max = int.MaxValue) =>
            new Random().Next(min, max);

        public static int[] GenerateRandomArray(int length)
        {
            var temp = new int[length];

            for (var i = 0; i < length; i++)
            {
                var randomNumber = GetRandomNumber();

                while (!temp.Contains(randomNumber))
                {
                    temp[i] = randomNumber;
                }
            }

            return temp;
        }
    }
}