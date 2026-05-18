using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public static class PresetManager
{
    private static readonly string PresetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Presets");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static RandomizerPreset? ActivePreset { get; private set; }

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
        if (!File.Exists(path))
            return null;
        try
        {
            ActivePreset = JsonSerializer.Deserialize<RandomizerPreset>(File.ReadAllText(path), JsonOptions);
        }
        catch
        {
            ActivePreset = null;
            return null;
        }
        return ActivePreset;
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

    public static void ApplyPresetToForm(Form form)
    {
        if (ActivePreset == null) return;

        switch (form)
        {
            case SMWE smwe:
                ApplySettings(smwe, "SMWE", ActivePreset);
                break;
            case SMTE smte:
                ApplySettings(smte, "SMTE", ActivePreset);
                break;
            case TrainerRand tr6:
                ApplySettings(tr6, "TR6", ActivePreset);
                break;
            case StaticEncounterEditor7 see7:
                ApplySettings(see7, "SEE7", ActivePreset);
                break;
            case MartEditor7 me7:
                ApplySettings(me7, "ME7", ActivePreset);
                break;
            case MartEditor6 me6:
                ApplySettings(me6, "ME6", ActivePreset);
                break;
            case MoveEditor7 mve7:
                ApplySettings(mve7, "MVE7", ActivePreset);
                break;
            case EvolutionEditor7 evo7:
                ApplySettings(evo7, "EVO7", ActivePreset);
                break;
        }
    }

    private static void ApplySettings(Form form, string formKey, RandomizerPreset preset)
    {
        var settings = preset.Settings.Where(s => s.Form == formKey).ToDictionary(s => s.Name, s => ParseValue(s.Value));
        foreach (var kvp in settings)
        {
            var ctrl = form.Controls.Find(kvp.Key, true).FirstOrDefault();
            if (ctrl == null) continue;
            if (kvp.Value is bool b && ctrl is CheckBox cb)
                cb.Checked = b;
            else if (kvp.Value is int i && ctrl is ComboBox combo)
                combo.SelectedIndex = i;
            else if (kvp.Value is decimal d && ctrl is NumericUpDown nud)
                nud.Value = d;
        }
    }

    public static void CaptureFormSettings(Form form)
    {
        if (ActivePreset == null) return;

        switch (form)
        {
            case SMWE smwe:
                CaptureSettings(smwe, "SMWE", ActivePreset);
                break;
            case SMTE smte:
                CaptureSettings(smte, "SMTE", ActivePreset);
                break;
            case TrainerRand tr6:
                CaptureSettings(tr6, "TR6", ActivePreset);
                break;
            case StaticEncounterEditor7 see7:
                CaptureSettings(see7, "SEE7", ActivePreset);
                break;
            case MartEditor7 me7:
                CaptureSettings(me7, "ME7", ActivePreset);
                break;
            case MartEditor6 me6:
                CaptureSettings(me6, "ME6", ActivePreset);
                break;
            case MoveEditor7 mve7:
                CaptureSettings(mve7, "MVE7", ActivePreset);
                break;
            case EvolutionEditor7 evo7:
                CaptureSettings(evo7, "EVO7", ActivePreset);
                break;
        }

        ActivePreset.CreatedAt = DateTime.Now;
        SavePreset(ActivePreset);
    }

    private static void CaptureSettings(Form form, string formKey, RandomizerPreset preset)
    {
        var existing = preset.Settings.Where(s => s.Form != formKey).ToList();

        switch (form)
        {
            case SMWE smwe:
                existing.AddRange(smwe.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case SMTE smte:
                existing.AddRange(smte.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case TrainerRand tr6:
                existing.AddRange(tr6.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case StaticEncounterEditor7 see7:
                existing.AddRange(see7.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case MartEditor7 me7:
                existing.AddRange(me7.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case MartEditor6 me6:
                existing.AddRange(me6.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case MoveEditor7 mve7:
                existing.AddRange(mve7.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
            case EvolutionEditor7 evo7:
                existing.AddRange(evo7.GetSettings().Select(kvp => new PresetEntry { Form = formKey, Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" }));
                break;
        }

        preset.Settings = existing;
    }

    private static object? ParseValue(string value)
    {
        if (bool.TryParse(value, out var b)) return b;
        if (int.TryParse(value, out var i)) return i;
        if (decimal.TryParse(value, out var d)) return d;
        return value;
    }
}