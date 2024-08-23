
// (c) 2024 Kazuki Kohzuki

using System.Text;

namespace SXConverter.Ufs;

/// <summary>
/// Represents an fs-TAS spectra data.
/// </summary>
public class SpectraData
{
    protected double[] times = [];
    protected double[] wavelengths = [];
    protected double[][] spectra = [];

    /// <summary>
    /// Gets the version of the <see cref="SpectraData"/> format.
    /// </summary>
    public static int Version { get; protected set; } = 2;

    protected static readonly int[] supported_version = [Version];

    /// <summary>
    /// Gets the time axis information.
    /// </summary>
    public AxisInfo TimeAxis { get; } = new("Time", "ps");

    /// <summary>
    /// Gets the wavelength axis information.
    /// </summary>
    public AxisInfo WavelengthAxis { get; } = new("Wavelength", "nm");

    /// <summary>
    /// Gets the number of time points.
    /// </summary>
    public int TimeCount => this.times.Length;

    /// <summary>
    /// Gets the minimum value of the time axis.
    /// </summary>
    public double TimeMin => this.times.Min();

    /// <summary>
    /// Gets the maximum value of the time axis.
    /// </summary>
    public double TimeMax => this.times.Max();

    /// <summary>
    /// Gets the number of wavelength points.
    /// </summary>
    public int WavelengthCount => this.wavelengths.Length;

    /// <summary>
    /// Gets the minimum value of the wavelength axis.
    /// </summary>
    public double WavelengthMin => this.wavelengths.Min();

    /// <summary>
    /// gets the maximum value of the wavelength axis.
    /// </summary>
    public double WavelengthMax => this.wavelengths.Max();

    public string Metadata { get; set; } = string.Empty;

    #region read

    /// <summary>
    /// Reads a <see cref="SpectraData"/> from a CSV file.
    /// </summary>
    /// <param name="path">The path of the CSV file.</param>
    /// <returns>The <see cref="SpectraData"/> read from the CSV file.</returns>
    public static SpectraData ReadFromCsv(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return ReadFromCsv(stream);
    } // public static SpectraData ReadFromCsv (string)

    /// <summary>
    /// Reads a <see cref="SpectraData"/> from a CSV data.
    /// </summary>
    /// <param name="stream">The stream of the CSV file.</param>
    /// <returns>The <see cref="SpectraData"/> read from the CSV data.</returns>
    public static SpectraData ReadFromCsv(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var data = new SpectraData();

        var n = ReadValues(reader, out var header);
        if (n == 0) throw new IOException("No header found.");
        data.times = header.Skip(1).ToArray();

        var wavelength = new List<double>();
        var spectra = new List<double[]>();
        while (true)
        {
            var values = ReadValues(reader, out var row);
            if (values == 0) break;
            wavelength.Add(row.First());
            spectra.Add(row.Skip(1).ToArray());
        } // while (true)

        data.wavelengths = [.. wavelength];
        data.spectra = [.. spectra];

        var metadata = reader.ReadToEnd();
        data.Metadata = "file info\r" + metadata;

        return data;
    } // public static SpectraData ReadFromCsv (Stream)

    private static int ReadValues(StreamReader stream, out IEnumerable<double> values)
    {
        var line = stream.ReadLine();
        if (string.IsNullOrEmpty(line))
        {
            values = [];
            return 0;
        }

        var vals = line.Split(',');
        if (!vals.All(v => double.TryParse(v, out _)))
        {
            values = [];
            return 0;
        }
        values = vals.Select(v => double.Parse(v));
        return vals.Length;
    } // private static IEnumerable<double> ReadValues (StreamReader)

    /// <summary>
    /// Reads a <see cref="SpectraData"/> from an Ultrafast Systems (UFS) file.
    /// </summary>
    /// <param name="path">The path of the UFS file.</param>
    /// <returns>The <see cref="SpectraData"/> read from the UFS file.</returns>
    public static SpectraData ReadFromUfs(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return ReadFromUfs(stream);
    } // public static SpectraData ReadFromUfs (string)

    /// <summary>
    /// Reads a <see cref="SpectraData"/> from an Ultrafast Systems (UFS) binary data.
    /// </summary>
    /// <param name="stream">The stream of the UFS file.</param>
    /// <returns>The <see cref="SpectraData"/> read from the UFS data.</returns>
    private static SpectraData ReadFromUfs(Stream stream)
    {
        var data = new SpectraData();

        var versionStr = ReadString(stream);
        if (!versionStr.StartsWith("Version")) throw new IOException("Invalid version string.");
        if (!int.TryParse(versionStr.AsSpan(7), out var version)) throw new IOException("Invalid version number.");
        if (!supported_version.Contains(version)) throw new IOException("Unsupported version.");
        
        var wavelengthName = ReadString(stream);
        var wavelengthUnit = ReadString(stream);
        data.WavelengthAxis.Name = wavelengthName;
        data.WavelengthAxis.Unit = wavelengthUnit;

        var wavelengthCount = ReadInt(stream);
        data.wavelengths = ReadDoubles(stream, wavelengthCount);

        var timeName = ReadString(stream);
        var timeUnit = ReadString(stream);
        data.TimeAxis.Name = timeName;
        data.TimeAxis.Unit = timeUnit;

        var timeCount = ReadInt(stream);
        data.times = ReadDoubles(stream, timeCount);

        var dataLabel = ReadString(stream);
        if (dataLabel != "DA") throw new IOException("Invalid data label.");

        var padding = ReadInt(stream);
        if (padding != 0) throw new IOException("Invalid padding.");

        var wc = ReadInt(stream); // wavelength count
        if (wc != wavelengthCount) throw new IOException("Invalid wavelength count.");
        var tc = ReadInt(stream); // time count
        if (tc != timeCount) throw new IOException("Invalid time count.");

        data.spectra = new double[data.WavelengthCount][];
        for (var i = 0; i < data.WavelengthCount; i++)
            data.spectra[i] = ReadDoubles(stream, data.TimeCount);

        data.Metadata = ReadString(stream);

        return data;
    } // private static SpectraData ReadFromUfs (Stream)

    private static int ReadInt(Stream stream)
    {
        var bytes = new byte[sizeof(int)];
        stream.Read(bytes, 0, bytes.Length);
        CorrectEndian(bytes);
        return BitConverter.ToInt32(bytes, 0);
    } // private static int ReadInt (Stream)

    private static double ReadDouble(Stream stream)
    {
        var bytes = new byte[sizeof(double)];
        stream.Read(bytes, 0, bytes.Length);
        CorrectEndian(bytes);
        return BitConverter.ToDouble(bytes, 0);
    } // private static double ReadDouble (Stream)

    protected static double[] ReadDoubles(Stream stream, int count)
    {
        var values = new double[count];
        for (var i = 0; i < count; i++)
            values[i] = ReadDouble(stream);
        return values;
    } // protected static double[] ReadDoubles (Stream, int)

    private static string ReadString(Stream stream)
    {
        var length = ReadInt(stream);
        if (length == 0) return string.Empty;

        var bytes = new byte[length];
        stream.Read(bytes, 0, bytes.Length);
        return Encoding.UTF8.GetString(bytes);
    } // private static string ReadString (Stream)

    #endregion read

    #region write

    /// <summary>
    /// Writes the <see cref="SpectraData"/> as a CSV file.
    /// </summary>
    /// <param name="path">The path of the CSV file to write.</param>
    public void WriteAsCsv(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteAsCsv(stream);
    } // public void WriteAsCsv (string)

    /// <summary>
    /// Writes the <see cref="SpectraData"/> as a CSV data.
    /// </summary>
    /// <param name="stream">The stream to write the CSV data.</param>
    public void WriteAsCsv(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        writer.Write($"0,{string.Join(',', this.times)}\r");
        for (var i = 0; i < this.WavelengthCount; i++)
            writer.Write($"{this.wavelengths[i]},{string.Join(',', this.spectra[i])}\r");
        writer.Write(this.Metadata);
    } // public void WriteAsCsv (Stream)

    /// <summary>
    /// Writes the <see cref="SpectraData"/> as an Ultrafast Systems (UFS) file.
    /// </summary>
    /// <param name="path">The path of the UFS file to write.</param>
    public void WriteAsUfs(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        WriteAsUfs(stream);
    } // public void WriteAsUfs (string)

    /// <summary>
    /// Writes the <see cref="SpectraData"/> as an Ultrafast Systems (UFS) binary data.
    /// </summary>
    /// <param name="stream">The stream to write the UFS data.</param>
    public void WriteAsUfs(Stream stream)
    {
        WriteString(stream, GetVersinString());

        WriteAxisInfo(stream, this.WavelengthAxis);
        WriteAxisData(stream, this.wavelengths);
        WriteAxisInfo(stream, this.TimeAxis);
        WriteAxisData(stream, this.times);

        WriteString(stream, GetDataLabel());
        WriteInt(stream, 0);
        WriteInt(stream, this.WavelengthCount);
        WriteInt(stream, this.TimeCount);
        for (var i = 0; i < this.WavelengthCount; i++)
            WriteDoubles(stream, this.spectra[i]);

        WriteString(stream, this.Metadata);
    } // public void WriteAsUfs (Stream)

    private static void WriteString(Stream stream, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var length = GetBytes(text.Length);
        var bytes = GetBytes(text);

        stream.Write(length, 0, length.Length);
        stream.Write(bytes, 0, bytes.Length);
    } // private static void WriteString (Stream, string)

    private static void WriteInt(Stream stream, int value)
    {
        var bytes = GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    } // private static void WriteInt (Stream, int)

    private static void WriteDouble(Stream stream, double value)
    {
        var bytes = GetBytes(value);
        stream.Write(bytes, 0, bytes.Length);
    } // private static void WriteDouble (Stream, double)

    private static void WriteDoubles(Stream stream, IEnumerable<double> values)
    {
        foreach (var value in values)
            WriteDouble(stream, value);
    } // private static void WriteDoubles (Stream, IEnumerable<double>)

    private static void WriteAxisInfo(Stream stream, AxisInfo axis)
    {
        WriteString(stream, axis.Name);
        WriteString(stream, axis.Unit);
    } // private static void WriteAxisInfo (Stream, AxisInfo)

    private static void WriteAxisData(Stream stream, double[] data)
    {
        WriteInt(stream, data.Length);
        WriteDoubles(stream, data);
    } // private static void WriteAxisData (Stream, double[])

    #region get text

    protected virtual string GetVersinString() => $"Version{Version}";

    protected virtual string GetDataLabel() => "DA";

    #endregion get text

    protected static byte[] CorrectEndian(byte[] bytes)
    {
        if (!BitConverter.IsLittleEndian) return bytes;
        Array.Reverse(bytes);
        return bytes;
    } // protected static byte[] CorrectEndian (byte[])

    protected static byte[] GetBytes(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        return CorrectEndian(bytes);
    } // protected byte[] GetBytes (int)

    protected static byte[] GetBytes(double value)
    {
        /*
         * this implementation returns negative-nan if the value is a nan.
         * Maybe the most significant bit must be changed
         * if the SpactraXplorer app does not work well with negative-nan.
         * i.e., `if (double.IsNaN) bytes[0] ^= 0x80;`
         */
        var bytes = BitConverter.GetBytes(value);
        return CorrectEndian(bytes);
    } // protected byte[] GetBytes (double

    protected static byte[] GetBytes(string value)
    {
        if (string.IsNullOrEmpty(value)) return [];
        return Encoding.UTF8.GetBytes(value);
    } // protected byte[] GetBytes (string)

    #endregion write

    #region trim

    /// <summary>
    /// Trims the time axis.
    /// </summary>
    /// <param name="min">The minimum value of the time axis.</param>
    /// <param name="max">The maximum value of the time axis.</param>
    public void TrimTime(double min, double max)
    {
        var minIndex = Array.BinarySearch(this.times, min);
        if (minIndex < 0) minIndex = ~minIndex;
        var maxIndex = Array.BinarySearch(this.times, max);
        if (maxIndex < 0) maxIndex = ~maxIndex;

        this.times = this.times[minIndex..maxIndex];
        this.spectra = this.spectra.Select(d => d[minIndex..maxIndex]).ToArray();
    } // public void TrimTime (double, double)

    /// <summary>
    /// Trims the wavelength axis.
    /// </summary>
    /// <param name="min">The minimum value of the wavelength axis.</param>
    /// <param name="max">The maximum value of the wavelength axis.</param>
    public void TrimWavelength(double min, double max)
    {
        var minIndex = Array.BinarySearch(this.wavelengths, min);
        if (minIndex < 0) minIndex = ~minIndex;
        var maxIndex = Array.BinarySearch(this.wavelengths, max);
        if (maxIndex < 0) maxIndex = ~maxIndex;

        this.wavelengths = this.wavelengths[minIndex..maxIndex];
        this.spectra = this.spectra[minIndex..maxIndex];
    } // public void TrimWavelength (double, double)

    #endregion trim
} // public class SpectraData
