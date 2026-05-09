using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotUtils;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.SevenZip;

namespace Project;

public class CharacterFile {
    public string Id = "";
    public string DisplayName = "";
    public string[] Authors = Array.Empty<string>();
    public Model? Model = null;

    public required string FilePath;

    const int CurrentVersion = 1;

    /// Returns an error if there is one
    [Pure]
    public async Task<string?> Write(Action<ProgressReport>? progress = null) =>
        await Write(FilePath, progress);

    /// Returns an error if there is one
    [Pure]
    public async Task<string?> Write(string path, Action<ProgressReport>? progress = null) {
        var result = FileUtils.OpenWriteStream(path);
        if (result.LetErr(out string err)) return err;
        await using var stream = result.Unwrap();

        // Creating the archive
        var options = new SevenZipWriterOptions {
            Progress = new Progress<ProgressReport>(p => progress?.Invoke(p))
        };
        await using var writer = await WriterFactory.OpenAsyncWriter(stream, ArchiveType.SevenZip, options);

        // Writing stuff
        await using var versionStream = new MemoryStream(Encoding.UTF8.GetBytes(CurrentVersion.ToString()));
        await writer.WriteAsync("version", versionStream);
        if (Model is {} model) {
            await using var modelStream = new MemoryStream();
            if (await model.Save(modelStream) is { } error) return error;
            modelStream.Seek(0, SeekOrigin.Begin);
            await writer.WriteDirectoryAsync("models");
            await writer.WriteAsync($"models/main.{model.Extension}", modelStream);
        }
        return null;
    }

    [Pure]
    public static async Task<Result<CharacterFile>> Read(string path, Action<ProgressReport>? progress = null) {
        var result = FileUtils.OpenReadStream(path);
        if (result.LetErr(out string err)) return Result.Err(err);
        await using var stream = result.Unwrap();

        // Reading the archive
        var options = new ReaderOptions {
            Progress = new Progress<ProgressReport>(p => progress?.Invoke(p))
        };
        await using var archive = await SevenZipArchive.OpenAsyncArchive(stream, options);

        var entries = (await archive.EntriesAsync.ToListAsync())
            .Where(e => !e.IsDirectory && e.Key != null)
            .ToDictionary(e => e.Key!);

        // Reading stuff
        var character = new CharacterFile { FilePath = path };
        string? version = await CharacterFileVersion.GetFileAsUtf(entries, "version");
        if (!int.TryParse(version, out int versionInt)) return Result.Err("Failed to get the format version");
        CharacterFileVersion? script = versionInt switch {
            1 => new CharacterFileV1(),
            _ => null
        };
        if (script == null) return Result.Err($"Version conversion not implemented ({versionInt} to {CurrentVersion})");
        if (script.Version != versionInt) return Result.Err($"Mismatched script version ({script.Version}, {versionInt})");
        script.Entries = entries;
        (var file, string? error) = await script.Run(character);
        if (error != null) return Result.Err(error);
        return Result.Ok(file!);
    }
}