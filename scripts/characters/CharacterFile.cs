using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using GodotUtils;
using SharpCompress;
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
    public GltfMesh? Model = null;

    public required string FilePath;

    const int Version = 0;

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
        await using var nameStream = new MemoryStream(Encoding.UTF8.GetBytes(DisplayName));
        await writer.WriteAsync("name.txt", nameStream);
        await using var authorStream = new MemoryStream(Encoding.UTF8.GetBytes(Authors.Join(",")));
        await writer.WriteAsync("authors.txt", authorStream);
        return null;
    }

    [Pure]
    public static async Task<Result<CharacterFile>> Read(string path, Action<ProgressReport>? progress = null) {
        var result = FileUtils.OpenReadStream(path);
        if (result.LetErr(out string err)) return Result<CharacterFile>.Err(err);
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

        string? version = await GetStringFast("version");
        if (int.TryParse(version, out int versionInt)) {
            if (versionInt != Version)
                throw new NotImplementedException($"Version conversion not implemented ({versionInt} to {Version})");
        }

        string? name = await GetStringFast("name.txt");
        if (name != null) character.DisplayName = name;

        string? authors = await GetStringFast("authors.txt");
        if (authors != null) character.Authors = authors.Split(',', StringSplitOptions.TrimEntries);

        return Result<CharacterFile>.Ok(character);

        async Task<BinaryReader?> GetFast(string key) {
            if (!entries.TryGetValue(key, out var entry)) return null;
            await using var file = await entry.OpenEntryStreamAsync();
            return new BinaryReader(file);
        }
        async Task<string?> GetStringFast(string key) {
            using var reader = await GetFast(key);
            return reader?.ReadString();
        }
    }
}