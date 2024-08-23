
// (c) 2024 Kazuki Kohzuki

using System.Diagnostics;

namespace SXConverter.Ufs;

/// <summary>
/// Represents the information of an axis.
/// </summary>
/// <param name="name">The name of the axis.</param>
/// <param name="unit">The unit of the axis.</param>
[DebuggerDisplay("{Name} ({Unit})")]
public sealed class AxisInfo(string name, string unit)
{
    /// <summary>
    /// Gets or sets the name of the axis.
    /// </summary>
    public string Name { get; set; } = name;

    /// <summary>
    /// Gets or sets the unit of the axis.
    /// </summary>
    public string Unit { get; set; } = unit;
} // public sealed class AxisInfo
