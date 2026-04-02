using System.IO.MemoryMappedFiles;
using GodotUtils;

namespace Bluchalk.shows;

public abstract class ShowFormat<T> {
    protected abstract Result<T> Read(Stream stream);
    protected abstract Result<Unit> Write(Stream stream, T data);

    public Result<T> ReadStream(Stream stream) => Read(stream);
    public Result<Unit> WriteStream(Stream stream, T data) => Write(stream, data);

    /// File-specific way of reading shows. Faster than just reading off a stream
    public Result<T> ReadFile(string path) {
        try {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            return Read(accessor);
        } catch (Exception exception) {
            return Result<T>.Err($"Error opening a file view on {typeof(T).Name}: {exception}");
        }
    }

    public Result<Unit> WriteFile(string path) {
        throw new NotImplementedException("Writing shows is not yet implemented");
    }
}