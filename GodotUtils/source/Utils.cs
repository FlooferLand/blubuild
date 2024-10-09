using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GodotUtils;

public static class Utils {
    public static readonly RandomNumberGenerator Random = new();

    #region Math (vectors, random, etc)
    public static T ListRandom<T>(List<T> list) =>
        list[Random.RandiRange(0, list.Count - 1)];

    public static Vector2 CalculateRatio(Vector2 res, float factor = 0.5f) {
        res.X = (res.X / res.Y) * factor;
        res.Y = factor;
        return res;
    }
    #endregion

    #region Visual (console)
    public static void ClearConsole() {
        GD.Print(new string('\n', 16));
    }

    public static void Print(object var, string name = "Var") {
        GD.Print($"{name}: {var}");
    }
    public static void PrintVar(object var, string name = "Var") {
        ClearConsole();
        Print(var, name);
    }
    #endregion

    public static List<AudioStream> LoadAudioFromDir(string path) {
        var streams = new List<AudioStream>();
        foreach (string filename in DirAccess.Open(path).GetFiles()) {
            if (filename.EndsWith(".import")) continue;
            var resource = GD.Load($"{path}/{filename}");
            if (resource is AudioStream stream)
                streams.Add(stream);
        }
		
        // Safety check
        if (streams.Count == 0)
            GD.PrintErr($"No audio found at \"{path}\"");
		
        // Returning
        return streams;
    }

    public static IEnumerable<T> EnumValues<T>() {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Snaps to the closest (snap) increment
    /// </summary>
    public static float SnappyBoi(float value, float snap = 16) {
        return snap * Mathf.Round(value / snap);
    }
    
    // https://stackoverflow.com/a/30728269
    public static T? ForceNew<T>(params object[] args) {
        var type = typeof (T);
        object? instance = type.Assembly.CreateInstance(
            type.FullName!, false,
            BindingFlags.Instance | BindingFlags.NonPublic,
            null, args, null, null
        );
        return (T?) instance;
    }

    #region Old consoles and mobile
    public static bool IsWiiU() {
        return OS.GetName() == "wiiu";
    }
    public static bool Is3ds() {
        return OS.GetName() == "3ds";
    }
    
    /// Whenever we're running on a very old console
    public static bool IsHomebrew() {
        return IsWiiU() || Is3ds();
    }
    /// Whenever we're running on a very old console that has touch/stylus support
    public static bool IsTouchCapableHomebrew() {
        return IsWiiU() || Is3ds();
    }

    public static bool HasTouchscreen() {
        return IsTouchCapableHomebrew() || DisplayServer.IsTouchscreenAvailable();
    }
    #endregion

    public static SceneTree GetMainSceneTree() {
        return (SceneTree)Engine.GetMainLoop();
    }
}