using System;
using System.Numerics;

namespace DungeonLife
{
    // Summary:
    //     Represents a pseudo-random number generator, which is an algorithm that produces
    //     a sequence of numbers that meet certain statistical requirements for randomness.
    public interface IRandom
    {
        // Summary:
        //     Returns a non-negative random integer.
        //
        // Returns:
        //     A 32-bit signed integer that is greater than or equal to 0 and less than System.Int32.MaxValue.
        int Next();

        // Summary:
        //     Returns a non-negative random integer that is less than the specified maximum.
        //
        // Parameters:
        //   maxValue:
        //     The exclusive upper bound of the random number to be generated. maxValue must
        //     be greater than or equal to 0.
        //
        // Returns:
        //     A 32-bit signed integer that is greater than or equal to 0, and less than maxValue;
        //     that is, the range of return values ordinarily includes 0 but not maxValue. However,
        //     if maxValue equals 0, maxValue is returned.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     maxValue is less than 0.
        int Next(int maxValue);

        // Summary:
        //     Returns a random integer that is within a specified range.
        //
        // Parameters:
        //   minValue:
        //     The inclusive lower bound of the random number returned.
        //
        //   maxValue:
        //     The exclusive upper bound of the random number returned. maxValue must be greater
        //     than or equal to minValue.
        //
        // Returns:
        //     A 32-bit signed integer greater than or equal to minValue and less than maxValue;
        //     that is, the range of return values includes minValue but not maxValue. If minValue
        //     equals maxValue, minValue is returned.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     minValue is greater than maxValue.
        int Next(int minValue, int maxValue);

        // Summary:
        //     Fills the elements of a specified array of bytes with random numbers.
        //
        // Parameters:
        //   buffer:
        //     The array to be filled with random numbers.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        void NextBytes(byte[] buffer);

        // Summary:
        //     Fills the elements of a specified span of bytes with random numbers.
        //
        // Parameters:
        //   buffer:
        //     The array to be filled with random numbers.
        void NextBytes(Span<byte> buffer);

        // Summary:
        //     Returns a random floating-point number that is greater than or equal to 0.0,
        //     and less than 1.0.
        //
        // Returns:
        //     A double-precision floating point number that is greater than or equal to 0.0,
        //     and less than 1.0.
        double NextDouble();

        Vector2 NextDirection();
    }
}
