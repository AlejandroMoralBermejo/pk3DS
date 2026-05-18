using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace pk3DS.WinForms;

public static class PresetManager
{
    private static readonly string PresetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    static PresetManager()
    {
        if (!Directory.Exists(PresetDirectory))
            Directory.CreateDirectory(PresetDirectory);
    }

    public static string[] GetPresetNames() =>
        Directory.Exists(PresetDirectory)
            ? Directory.GetFiles(PresetDirectory, "*.json").Select(Path.GetFileNameWithoutExtension).Where(n => n != null).Cast<string>().ToArray()
            : [];

    public static RandomizerPreset? LoadPreset(string name)
    {
        string path = GetPresetPath(name);
        return File.Exists(path) ? JsonSerializer.Deserialize<RandomizerPreset>(File.ReadAllText(path), JsonOptions) : null;
    }

    public static bool SavePreset(RandomizerPreset preset)
    {
        if (string.IsNullOrWhiteSpace(preset.Name))
            return false;
        try { File.WriteAllText(GetPresetPath(preset.Name), JsonSerializer.Serialize(preset, JsonOptions)); return true; }
        catch { return false; }
    }

public static bool DeletePreset(string name)
    {
        string path = GetPresetPath(name);
        if (!File.Exists(path))
            return false;
        try { File.Delete(path); return true; }
        catch { return false; }
    }

    public static void ExportPreset(string name, string path)
    {
        var preset = LoadPreset(name);
        if (preset != null)
            File.WriteAllText(path, JsonSerializer.Serialize(preset, JsonOptions));
    }

    public static RandomizerPreset? ImportPreset(string path) =>
        File.Exists(path) ? JsonSerializer.Deserialize<RandomizerPreset>(File.ReadAllText(path), JsonOptions) : null;

    private static string GetPresetPath(string name) => Path.Combine(PresetDirectory, $"{name}.json");
}