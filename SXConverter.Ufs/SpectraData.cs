
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

        /*
         * The first line of the metadata is already read from the stream
         * while reading the spectra data section.
         * It is difficult to pass the last line of the spectra data section to the metadata.
         * The metadata always starts with "file info\r",
         * and it seems that SurfaceXplorer app recognizes the metadata with this prefix.
         * Therefore, "file info\r" is inserted at the beginning of the metadata.
         */
        var metadata = reader.ReadToEnd();
        data.Metadata = ("file info\r" + metadata).NormNewLineInput();

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
        var reader = new UfsReader(stream);

        var versionStr = reader.ReadString();
        if (!versionStr.StartsWith("Version")) throw new IOException("Invalid version string.");
        if (!int.TryParse(versionStr.AsSpan(7), out var version)) throw new IOException("Invalid version number.");
        if (!supported_version.Contains(version)) throw new IOException("Unsupported version.");
        
        var wavelengthName = reader.ReadString();
        var wavelengthUnit = reader.ReadString();
        data.WavelengthAxis.Name = wavelengthName;
        data.WavelengthAxis.Unit = wavelengthUnit;

        var wavelengthCount = reader.ReadInt32();
        data.wavelengths = reader.ReadDoubles(wavelengthCount);

        var timeName = reader.ReadString();
        var timeUnit = reader.ReadString();
        data.TimeAxis.Name = timeName;
        data.TimeAxis.Unit = timeUnit;

        var timeCount = reader.ReadInt32();
        data.times = reader.ReadDoubles(timeCount);

        var dataLabel = reader.ReadString();
        if (dataLabel != "DA") throw new IOException("Invalid data label.");

        var padding = reader.ReadInt32();
        if (padding != 0) throw new IOException("Invalid padding.");

        var wc = reader.ReadInt32(); // wavelength count
        if (wc != wavelengthCount) throw new IOException("Invalid wavelength count.");
        var tc = reader.ReadInt32(); // time count
        if (tc != timeCount) throw new IOException("Invalid time count.");

        data.spectra = new double[data.WavelengthCount][];
        for (var i = 0; i < data.WavelengthCount; i++)
            data.spectra[i] = reader.ReadDoubles(data.TimeCount);

        data.Metadata = reader.ReadString();

        return data;
    } // private static SpectraData ReadFromUfs (Stream)

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
        writer.Write(this.Metadata.NormalizeNewLineOutCsv());
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
        var writer = new UfsWriter(stream);

        writer.WriteString(GetVersinString());

        WriteAxisInfo(writer, this.WavelengthAxis);
        WriteAxisData(writer, this.wavelengths);
        WriteAxisInfo(writer, this.TimeAxis);
        WriteAxisData(writer, this.times);

        writer.WriteString(GetDataLabel());
        writer.WriteInt32(0);
        writer.WriteInt32(this.WavelengthCount);
        writer.WriteInt32(this.TimeCount);
        for (var i = 0; i < this.WavelengthCount; i++)
            writer.WriteDoubles(this.spectra[i]);

        writer.WriteString(this.Metadata);
    } // public void WriteAsUfs (Stream)

    private static void WriteAxisInfo(UfsWriter writer, AxisInfo axis)
    {
        writer.WriteString(axis.Name);
        writer.WriteString(axis.Unit);
    } // private static void WriteAxisInfo (UfsWriter, AxisInfo)

    private static void WriteAxisData(UfsWriter writer, double[] data)
    {
        writer.WriteInt32(data.Length);
        writer.WriteDoubles(data);
    } // private static void WriteAxisData (UfsWriter, double[])

    #region get text

    protected virtual string GetVersinString() => $"Version{Version}";

    protected virtual string GetDataLabel() => "DA";

    #endregion get text

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
