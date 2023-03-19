using System;

namespace HWM.Parser.Helpers
{
    public static class DataTypeConvertor<T>
    {
        // Compose 2D array of generic datatype from two 1D arrays
        public static T[,] ConvertTo2DArray(T[] arrayOne, T[] arrayTwo)
        {
            if (arrayOne == null || arrayTwo == null)
            {
                throw new ArgumentNullException("Arrays must be non-nullable");
            }

            if (arrayOne.Length != arrayTwo.Length)
            {
                throw new ArgumentException("Arrays must be equal size");
            }

            T[,] combinedArray = new T[arrayOne.Length, 2];

            for (var i = 0; i < arrayOne.Length; i++)
            {
                combinedArray[i, 0] = arrayOne[i];
                combinedArray[i, 1] = arrayTwo[i];
            }

            return combinedArray;
        }
    }
}
