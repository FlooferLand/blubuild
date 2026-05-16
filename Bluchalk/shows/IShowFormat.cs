using System.IO.MemoryMappedFiles;
using GodotUtils;
using Project;
using FileAccess = Godot.FileAccess;

namespace Bluchalk.shows;

public abstract class ShowFormat<T> {
    public abstract string Extension { get; }

    protected abstract Result<T> Read(Stream stream, string path);
    protected abstract Result<Unit> Write(Stream stream, T data);

    /// File-specific way of reading shows. Faster than just reading off a stream
    public Result<T> ReadFile(string path) {
        try {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateViewStream(0, 0, MemoryMappedFileAccess.Read);
            return Read(accessor, path);
        } catch (Exception exception) {
            try {
                using var file = new FileAccessStream(path, FileAccess.ModeFlags.Read);
                return Read(file, path);
            } catch (Exception e) {
                return Result<T>.Err($"Error opening a file view on {typeof(T).Name}:\n{exception}\n+\n{e}");
            }
        }
    }

    public Result<Unit> WriteFile(string path) {
        throw new NotImplementedException("Writing shows is not yet implemented");
    }
}