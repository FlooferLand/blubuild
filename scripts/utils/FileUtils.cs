using System;
using System.IO;
using GodotUtils;
using FileAccess = Godot.FileAccess;

namespace Project;

public static class FileUtils {
    public static bool IsSandboxed => SystemAutoload.Instance.IsFileSystemSandboxed;

    /// Opens a file as a C# `Stream`, using native file APIs whenever possible
    public static Result<Stream> OpenReadStream(string path) {
        if (!IsSandboxed) {
            try {
                var stream = new FileStream(path, FileMode.Open);
                return Result<Stream>.Ok(stream);
            } catch (Exception e) {
                Log.Error($"Exception thrown while reading file '{path}'", e);
            }
        }

        try {
            var stream = new FileAccessStream(path, FileAccess.ModeFlags.Read);
            return Result<Stream>.Ok(stream);
        } catch (Exception e) {
            return Result<Stream>.Err(e.ToString());
        }
    }

    /// Opens a file as a C# `Stream`, using native file APIs whenever possible
    public static Result<Stream> OpenWriteStream(string path) {
        if (!IsSandboxed) {
            try {
                var stream = new FileStream(path, FileMode.Create);
                return Result<Stream>.Ok(stream);
            } catch (Exception e) {
                Log.Error($"Exception thrown while writing file '{path}'", e);
            }
        }

        try {
            var stream = new FileAccessStream(path, FileAccess.ModeFlags.Write);
            return Result<Stream>.Ok(stream);
        } catch (Exception e) {
            return Result<Stream>.Err(e.ToString());
        }
    }
}