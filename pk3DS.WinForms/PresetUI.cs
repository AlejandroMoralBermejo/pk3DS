using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public class PresetUI : Form
{
    private readonly ListBox listBox = new();
    private readonly TextBox txtName = new();
    private readonly Button btnSave = new();
    private readonly Button btnLoad = new();
    private readonly Button btnDelete = new();
    private readonly Button btnExport = new();
    private readonly Button btnImport = new();
    private readonly Button btnClose = new();

    public PresetUI()
    {
        InitializeComponent();
        RefreshList();
    }

    private void InitializeComponent()
    {
        Text = "Randomizer Presets";
        Size = new Size(420, 340);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        listBox.Location = new Point(12, 12);
        listBox.Size = new Size(380, 180);
        listBox.SelectedIndexChanged += (s, e) =>
        {
            if (listBox.SelectedItem != null)
                txtName.Text = listBox.SelectedItem.ToString() ?? "";
        };

        var lblName = new Label { Location = new Point(12, 198), Text = "Preset Name:" };
        txtName.Location = new Point(12, 218);
        txtName.Size = new Size(380, 20);

        btnSave.Text = "Save Current"; btnSave.Size = new Size(90, 23);
        btnSave.Location = new Point(12, 245);
        btnSave.Click += BtnSave_Click;

        btnLoad.Text = "Load"; btnLoad.Size = new Size(90, 23);
        btnLoad.Location = new Point(108, 245);
        btnLoad.Click += BtnLoad_Click;

        btnDelete.Text = "Delete"; btnDelete.Size = new Size(90, 23);
        btnDelete.Location = new Point(204, 245);
        btnDelete.Click += BtnDelete_Click;

        btnExport.Text = "Export..."; btnExport.Size = new Size(90, 23);
        btnExport.Location = new Point(12, 274);
        btnExport.Click += BtnExport_Click;

        btnImport.Text = "Import..."; btnImport.Size = new Size(90, 23);
        btnImport.Location = new Point(108, 274);
        btnImport.Click += BtnImport_Click;

        btnClose.Text = "Close"; btnClose.Size = new Size(90, 23);
        btnClose.Location = new Point(302, 274);
        btnClose.Click += (s, e) => Close();

        Controls.AddRange(new Control[] { listBox, lblName, txtName, btnSave, btnLoad, btnDelete, btnExport, btnImport, btnClose });
    }

    private void RefreshList()
    {
        listBox.Items.Clear();
        listBox.Items.AddRange(PresetManager.GetPresetNames());
    }

    private void BtnSave_Click(object sender, EventArgs e)
    {
        string name = txtName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        { WinFormsUtil.Alert("Enter a preset name."); return; }

        var preset = new RandomizerPreset { Name = name };
        CaptureSettings(preset);
        if (PresetManager.SavePreset(preset))
        { RefreshList(); WinFormsUtil.Alert($"Preset '{name}' saved."); }
        else
        { WinFormsUtil.Alert("Failed to save preset."); }
    }

    private void BtnLoad_Click(object sender, EventArgs e)
    {
        string? name = listBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(name))
        { WinFormsUtil.Alert("Select a preset."); return; }

        var preset = PresetManager.LoadPreset(name);
        if (preset == null)
        { WinFormsUtil.Alert("Failed to load preset."); return; }

        ApplySettings(preset);
        WinFormsUtil.Alert($"Preset '{name}' loaded!");
    }

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        string? name = listBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(name))
        { WinFormsUtil.Alert("Select a preset."); return; }

        if (DialogResult.Yes != WinFormsUtil.Prompt(MessageBoxButtons.YesNo, $"Delete '{name}'?"))
            return;

        if (PresetManager.DeletePreset(name))
        { RefreshList(); txtName.Text = ""; WinFormsUtil.Alert("Deleted."); }
        else
        { WinFormsUtil.Alert("Failed to delete."); }
    }

    private void BtnExport_Click(object sender, EventArgs e)
    {
        string? name = listBox.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(name))
        { WinFormsUtil.Alert("Select a preset."); return; }

        using var dialog = new SaveFileDialog { Filter = "JSON (*.json)|*.json", FileName = name };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        PresetManager.ExportPreset(name, dialog.FileName);
        WinFormsUtil.Alert($"Exported to {dialog.FileName}");
    }

    private void BtnImport_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Filter = "JSON (*.json)|*.json" };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        var preset = PresetManager.ImportPreset(dialog.FileName);
        if (preset == null)
        { WinFormsUtil.Alert("Invalid preset file."); return; }

        string name = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
        preset.Name = name;
        PresetManager.SavePreset(preset);
        RefreshList();
        WinFormsUtil.Alert($"Imported as '{name}'!");
    }

    private void CaptureSettings(RandomizerPreset preset)
    {
        foreach (Form form in Application.OpenForms)
        {
            if (form is SMWE smwe)
            {
                var settings = smwe.GetSettings();
                foreach (var kvp in settings)
                    preset.Settings.Add(new PresetEntry { Form = "SMWE", Name = kvp.Key, Value = kvp.Value?.ToString() ?? "" });
            }
        }
    }

    private void ApplySettings(RandomizerPreset preset)
    {
        foreach (Form form in Application.OpenForms)
        {
            if (form is SMWE smwe)
            {
                var dict = preset.Settings
                    .Where(s => s.Form == "SMWE")
                    .ToDictionary(s => s.Name, s => ParseValue(s.Value, s.Name, smwe));
                smwe.SetSettings(dict);
            }
        }
    }

    private object? ParseValue(string value, string name, SMWE form)
    {
        if (form.Controls.Find(name, true).FirstOrDefault() is CheckBox)
            return bool.TryParse(value, out var b) ? b : false;
        if (form.Controls.Find(name, true).FirstOrDefault() is NumericUpDown)
            return decimal.TryParse(value, out var d) ? d : 0m;
        if (form.Controls.Find(name, true).FirstOrDefault() is ComboBox)
            return int.TryParse(value, out var i) ? i : 0;
        return null;
    }
}