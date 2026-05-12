using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GodotUtils;
using SharpCompress.Archives;

namespace Project;

/// Overriden by other classes for reading older files
public abstract record CharacterFileVersion {
    public Dictionary<string, IArchiveEntry> Entries = null!;

    public abstract int Version { get; }

    /** Returns an error, if any */
    public abstract Task<Result<CharacterFile>> Run(CharacterFile file, ProgressHolder progress);

    public bool HasFile(string key) => Entries.ContainsKey(key);
    public async Task<MemoryStream?> OpenFile(string key) => await OpenFile(Entries, key);
    public async Task<string?> GetFileAsUtf(string key) => await GetFileAsUtf(Entries, key);
    public IEnumerable<string> ListFolder(string key) {
        string folder = key.EndsWith('/') ? key : key + '/';
        return Entries
            .AsParallel()
            .Where(e => e.Key.StartsWith(folder, StringComparison.Ordinal))
            .Select(e => e.Key);
    }

    // NOTE: Having to copy mem here because SharpCompress hauls ass and doesn't implement `Length`
    public static async Task<MemoryStream?> OpenFile(Dictionary<string, IArchiveEntry> entries, string key) {
        if (!entries.TryGetValue(key, out var entry)) return null;
        await using var entryStream = await entry.OpenEntryStreamAsync();
        var stream = new MemoryStream((int) entry.Size);
        await entryStream.CopyToAsync(stream);
        stream.Position = 0;
        return stream;
    }
    public static async Task<string?> GetFileAsUtf(Dictionary<string, IArchiveEntry> entries, string key) {
        if (!entries.TryGetValue(key, out var entry)) return null;
        await using var stream = await entry.OpenEntryStreamAsync();
        byte[] bytes = new byte[entry.Size];
        await stream.ReadExactlyAsync(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
}