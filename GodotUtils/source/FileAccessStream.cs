using System;
using System.IO;
using System.Threading.Tasks;
using Godot;
using FileAccess = Godot.FileAccess;

namespace Project;

/// Godot completely lacks a C# Stream implementation for their FileAccess API so this is needed to fill in the gaps
public class FileAccessStream(FileAccess file, FileAccess.ModeFlags flags) : Stream {
    public override bool CanRead { get; } = flags is FileAccess.ModeFlags.Read or FileAccess.ModeFlags.ReadWrite or FileAccess.ModeFlags.WriteRead;
    public override bool CanWrite { get; } = flags is FileAccess.ModeFlags.Write or FileAccess.ModeFlags.ReadWrite or FileAccess.ModeFlags.WriteRead;
    public override bool CanSeek => true;
    public override long Length => (long) file.GetLength();
    public override long Position {
        get => (long) file.GetPosition();
        set => file.Seek((ulong) value);
    }

    public FileAccessStream(string path, FileAccess.ModeFlags flags) : this(OpenFile(path, flags), flags) {}

    static FileAccess OpenFile(string path, FileAccess.ModeFlags flags) {
        var file = FileAccess.Open(path, flags);
        return FileAccess.GetOpenError() != Error.Ok
            ? throw new IOException(FileAccess.GetOpenError().ToString())
            : file;
    }

    public override void Flush() => file.Flush();
    public override int Read(byte[] buffer, int offset, int count) {
        byte[]? bytes = file.GetBuffer(count);
        Buffer.BlockCopy(bytes, 0, buffer, offset, bytes.Length);
        return bytes.Length;
    }
    public override void Write(byte[] buffer, int offset, int count) {
        if (offset == 0 && count == buffer.Length) {
            file.StoreBuffer(buffer);
        } else {
            byte[] slice = new byte[count];
            Buffer.BlockCopy(buffer, offset, slice, 0, count);
            file.StoreBuffer(slice);
        }
    }

    public override long Seek(long offset, SeekOrigin origin) {
        long pos = origin switch {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
        file.Seek((ulong)pos);
        return Position;
    }
    public override void SetLength(long length) {
        if (!CanWrite) return;
        var error = file.Resize(length);
        if (error != Error.Ok) throw new IOException(error.ToString());
    }

    protected override void Dispose(bool disposing) {
        if (disposing) file.Dispose();
        base.Dispose(disposing);
    }
}