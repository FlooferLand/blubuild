using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotUtils;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using SharpCompress.Writers.Zip;

namespace Project;

public class CharacterFile {
    public string Id = "";
    public string DisplayName = "";
    public string[] Authors = Array.Empty<string>();
    public readonly BitMappingData BitData = new();
    public Model? Model = null;

    public required string FilePath;

    const int CurrentVersion = 1;

    /// Returns an error if there is one
    [Pure]
    public async Task<string?> Write(ProgressHolder progress) =>
        await Write(FilePath, progress);

    // TODO: Write files to memory, and only then write it to a file. In case the file fails to write, it doesn't actually corrupt the physical file.
    /// Returns an error if there is one
    [Pure]
    public async Task<string?> Write(string path, ProgressHolder progress) {
        var result = FileUtils.OpenWriteStream(path);
        if (result.LetErr(out string e)) return e;
        await using var stream = result.Unwrap();

        // Creating the archive
        var options = new ZipWriterOptions(CompressionType.Deflate) {
            Progress = progress.CreateProgressReport()
        };
        await using var writer = await WriterFactory.OpenAsyncWriter(stream, ArchiveType.Zip, options);

        // Writing stuff
        await using var versionStream = new MemoryStream(Encoding.UTF8.GetBytes(CurrentVersion.ToString()));
        await writer.WriteAsync("version", versionStream);
        if (Model is {} model) {
            progress.Set("Saving the model");
            await using var modelStream = new MemoryStream();
            if (await model.Save(modelStream, progress) is { } error) return error;
            modelStream.Seek(0, SeekOrigin.Begin);

            progress.Set("Writing the model");
            await writer.WriteDirectoryAsync("models");
            await writer.WriteAsync($"models/main.{model.Extension}", modelStream);
        }
        if (BitData.BitsToData.Count > 0) {
            progress.Set("Writing bitmap.txt");
            (string? text, string? err) = BitData.ToText();
            if (err != null) Result.Err($"Failed to write bit data: {err}");
            await writer.WriteAsync($"bitmap.txt", new MemoryStream(Encoding.UTF8.GetBytes(text!)));
        }
        return null;
    }

    [Pure]
    public static async Task<Result<CharacterFile>> Read(string path, ProgressHolder progress) {
        var result = FileUtils.OpenReadStream(path);
        if (result.LetErr(out string err)) return Result.Err(err);
        await using var stream = result.Unwrap();

        // Reading the archive
        var options = new ReaderOptions {
            Progress = progress.CreateProgressReport()
        };

        Dictionary<string, IArchiveEntry> entries;
        try {
            await using var archive = await ZipArchive.OpenAsyncArchive(stream, options);
            entries = (await archive.EntriesAsync.ToListAsync())
                .Where(e => e is { IsDirectory: false, Key: not null })
                .ToDictionary(e => e.Key!);
        } catch (IncompleteArchiveException) {
            stream.Position = 0;
            return Result.Err($"Archive is incomplete ({stream.Length} bytes)");
        } catch (Exception e) {
            return Result.Err($"Failed to load the character: {e}");
        }
        if (entries.Count == 0)
            return Result.Err("Failed to load the character. Archive is empty.");

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
        progress.Set($"Running reader v{script.Version}");
        (var file, string? error) = await script.Run(character, progress);
        if (error != null) return Result.Err(error);
        return Result.Ok(file!);
    }
}