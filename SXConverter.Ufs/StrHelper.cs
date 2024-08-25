
// (c) 2024 Kazuki Kohzuki

namespace SXConverter.Ufs;

internal static class StrHelper
{
    internal static string NormNewLineInput(this string s)
    {
        s = s.Replace("\r\n", "\n");
        s = s.Replace("\r", "\n");
        return s.Replace("\n", "\r\n");
    } // internal static string NormNewLineInput (this string)

    internal static string NormalizeNewLineOutUfs(this string s)
        => s.Replace("\r\n", "\n");

    internal static string NormalizeNewLineOutCsv(this string s)
        => s.Replace("\r\n", "\r");
} // internal static class StrHelper
