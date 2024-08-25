
// (c) 2024 Kazuki Kohzuki

namespace SXConverter.Ufs;

/// <summary>
/// Provides methods to write data to a stream.
/// </summary>
internal sealed class UfsWriter : UfsIOHelper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UfsWriter"/> class with the specified stream.
    /// </summary>
    /// <param name="stream">The stream to write data to.</param>
    internal UfsWriter(Stream stream) : base(stream) { }

    /// <summary>
    /// Writes a 32-bit signed integer to the stream.
    /// </summary>
    /// <param name="value">The 32-bit signed integer to write to the stream.</param>
    internal void WriteInt32(int value)
    {
        var bytes = BytesConverter.ToBytes(value);
        this._stream.Write(bytes, 0, bytes.Length);
    } // internal void WriteInt32 (int)

    /// <summary>
    /// Writes a 64-bit floating-point number to the stream.
    /// </summary>
    /// <param name="value">The 64-bit floating-point number to write to the stream.</param>
    internal void WriteDouble(double value)
    {
        var bytes = BytesConverter.ToBytes(value);
        this._stream.Write(bytes, 0, bytes.Length);
    } // internal void WriteDouble (double)

    /// <summary>
    /// Writes a collection of 64-bit floating-point numbers to the stream.
    /// </summary>
    /// <param name="values">The collection of 64-bit floating-point numbers to write to the stream.</param>
    internal void WriteDoubles(IEnumerable<double> values)
    {
        foreach (var value in values)
            WriteDouble(value);
    } // internal void WriteDoubles (IEnumerable<double>)

    /// <summary>
    /// Writes a string to the stream.
    /// </summary>
    /// <param name="value">The string to write to the stream.</param>
    internal void WriteString(string value)
    {
        /*
         * String data are stored with a 32-bit integer indicating the length of the string in bytes,
         * followed by the string data itself.
         */

        if (string.IsNullOrEmpty(value))
        {
            WriteInt32(0);
            return;
        }

        var bytes = BytesConverter.ToBytes(value.NormalizeNewLineOutUfs());
        WriteInt32(bytes.Length);
        this._stream.Write(bytes, 0, bytes.Length);
    } // internal void WriteString (string)
} // internal sealed class UfsWriter : UfsIOHelper
