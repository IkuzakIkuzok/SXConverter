
// (c) 2024 Kazuki Kohzuki

namespace SXConverter.Ufs;

internal abstract class UfsIOHelper : IDisposable
{
    protected readonly Stream _stream;
    private bool _disposed;

    internal UfsIOHelper(Stream stream)
    {
        this._stream = stream;
    } // internal UfsIOHelper (Stream)

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    } // public void Dispose ()

    private void Dispose(bool disposing)
    {
        if (this._disposed) return;
        if (disposing)
            this._stream.Dispose();
        this._disposed = true;
    } // private void Dispose (bool)
} // internal abstract class UfsIOHelper : IDisposable
