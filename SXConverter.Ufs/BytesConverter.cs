
// (c) 2024 Kazuki Kohzuki

using System.Text;

namespace SXConverter.Ufs;

/// <summary>
/// Converts bytes to various types and vice versa.
/// The endian is automatically corrected to big-endian.
/// </summary>
internal static class BytesConverter
{
    private static byte[] CorrectEndian(byte[] bytes)
    {
        if (BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return bytes;
    } // private static byte[] CorrectEndian (byte[])

    /// <summary>
    /// Converts the specified byte array to a 32-bit signed integer.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The 32-bit signed integer converted from the byte array.</returns>
    internal static int ToInt32(byte[] bytes)
        => BitConverter.ToInt32(CorrectEndian(bytes), 0);

    /// <summary>
    /// Converts the specified byte array to a 64-bit floating-point number.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The 64-bit floating-point number converted from the byte array.</returns>
    internal static double ToDouble(byte[] bytes)
        => BitConverter.ToDouble(CorrectEndian(bytes), 0);

    /// <summary>
    /// Converts the specified byte array to a string.
    /// </summary>
    /// <param name="bytes">The byte array to convert.</param>
    /// <returns>The string converted from the byte array.</returns>
    internal static string ToString(byte[] bytes)
        => Encoding.UTF8.GetString(bytes).TrimEnd('\0');

    /// <summary>
    /// Converts the specified 32-bit signed integer to a byte array.
    /// </summary>
    /// <param name="value">The 32-bit signed integer to convert.</param>
    /// <returns>The byte array converted from the 32-bit signed integer.</returns>
    internal static byte[] ToBytes(int value)
        => CorrectEndian(BitConverter.GetBytes(value));

    /// <summary>
    /// Converts the specified 64-bit floating-point number to a byte array.
    /// </summary>
    /// <param name="value">The 64-bit floating-point number to convert.</param>
    /// <returns>The byte array converted from the 64-bit floating-point number.</returns>
    internal static byte[] ToBytes(double value)
        => CorrectEndian(BitConverter.GetBytes(value));

    /// <summary>
    /// Converts the specified string to a byte array.
    /// </summary>
    /// <param name="value">The string to convert.</param>
    /// <returns>The byte array converted from the string.</returns>
    internal static byte[] ToBytes(string value)
        => Encoding.UTF8.GetBytes(value);
} // internal static class BytesConverter
