using System;
using System.Collections.Generic;
using System.Linq;

namespace pk3DS.WinForms;

public class RandomizerPreset
{
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int? Seed { get; set; }
    public List<PresetEntry> Settings { get; set; } = [];
}

public class PresetEntry
{
    public string Form { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}