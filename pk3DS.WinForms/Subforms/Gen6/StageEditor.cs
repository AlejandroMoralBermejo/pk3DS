using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace pk3DS.WinForms;

public class StageEditor : Form
{
    private CheckBox[] CHK_Enabled;
    private CheckBox[] CHK_Legendaries;
    private CheckBox[] CHK_Megas;
    private NumericUpDown[] NUD_BST;
    private ComboBox[] CB_MaxEvo;
    private CheckBox[] CHK_GymImportant;
    private Label[] L_Stage;

    private static readonly string[] StageNames = { "Early (Gyms 1-3)", "Mid (Gyms 4-6)", "Late (Gyms 7-8)", "Elite Four+" };
    private static readonly string[] EvoOptions = { "Any", "Max 1 Evo", "Max 2 Evo", "Max 3 Evo (Full)" };

    public static StageConfig[] Stages { get; private set; } = new StageConfig[4];

    public StageEditor()
    {
        InitializeControls();
        LoadConfig();
    }

    private void InitializeControls()
    {
        Text = "Progressive Difficulty Settings";
        Size = new Size(550, 380);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;

        int y = 20;
        var lbl = new Label { Location = new Point(12, y), Text = "Configure restrictions for each game stage. Trainers are categorized by their tag (GYM1-8, ELITE1-4, CHAMPION).", Width = 520 };
        Controls.Add(lbl);

        y += 35;

        // Header
        var lblStage = new Label { Location = new Point(12, y), Text = "Stage", Width = 120 };
        var lblEvo = new Label { Location = new Point(140, y), Text = "Max Evolution", Width = 90 };
        var lblBST = new Label { Location = new Point(235, y), Text = "BST Max", Width = 70 };
        var lblLegend = new Label { Location = new Point(310, y), Text = "Legends", Width = 70 };
        var lblMega = new Label { Location = new Point(385, y), Text = "Megas", Width = 60 };
        var lblGym = new Label { Location = new Point(450, y), Text = "Gym+1Lv", Width = 70 };
        Controls.AddRange(new Control[] { lblStage, lblEvo, lblBST, lblLegend, lblMega, lblGym });

        y += 25;

        CHK_Enabled = new CheckBox[4];
        CHK_Legendaries = new CheckBox[4];
        CHK_Megas = new CheckBox[4];
        NUD_BST = new NumericUpDown[4];
        CB_MaxEvo = new ComboBox[4];
        CHK_GymImportant = new CheckBox[4];
        L_Stage = new Label[4];

        for (int i = 0; i < 4; i++)
        {
            int stageY = y + (i * 45);

            CHK_Enabled[i] = new CheckBox { Location = new Point(12, stageY), Text = StageNames[i], Width = 120, Checked = true };
            CHK_Enabled[i].CheckedChanged += (s, e) => UpdateStageEnabled(i);

            CB_MaxEvo[i] = new ComboBox { Location = new Point(140, stageY - 2), DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
            CB_MaxEvo[i].Items.AddRange(EvoOptions);
            CB_MaxEvo[i].SelectedIndex = 0;

            NUD_BST[i] = new NumericUpDown { Location = new Point(235, stageY - 2), Minimum = 100, Maximum = 800, Value = 600, Width = 70 };

            CHK_Legendaries[i] = new CheckBox { Location = new Point(315, stageY), Width = 70 };
            CHK_Megas[i] = new CheckBox { Location = new Point(390, stageY), Width = 60 };

            CHK_GymImportant[i] = new CheckBox { Location = new Point(455, stageY), Width = 70 };
            CHK_GymImportant[i].Checked = i > 0; // Gym leaders get +1 stage by default after stage 0

            L_Stage[i] = new Label { Location = new Point(12, stageY + 20), Text = "", ForeColor = Color.Gray, Width = 520 };

            Controls.AddRange(new Control[] {
                CHK_Enabled[i], CB_MaxEvo[i], NUD_BST[i],
                CHK_Legendaries[i], CHK_Megas[i], CHK_GymImportant[i], L_Stage[i]
            });

            UpdateStageLabel(i);
        }

        y = y + (4 * 45) + 15;

        var btnOK = new Button { Location = new Point(370, y), Text = "OK", Width = 75 };
        btnOK.Click += BtnOK_Click;
        var btnCancel = new Button { Location = new Point(450, y), Text = "Cancel", Width = 75 };
        btnCancel.Click += (s, e) => Close();
        Controls.AddRange(new Control[] { btnOK, btnCancel });
    }

    private void UpdateStageEnabled(int i)
    {
        bool enabled = CHK_Enabled[i].Checked;
        CB_MaxEvo[i].Enabled = enabled;
        NUD_BST[i].Enabled = enabled;
        CHK_Legendaries[i].Enabled = enabled;
        CHK_Megas[i].Enabled = enabled;
        L_Stage[i].Visible = enabled;
        UpdateStageLabel(i);
    }

    private void UpdateStageLabel(int i)
    {
        if (!CHK_Enabled[i].Checked)
        {
            L_Stage[i].Text = "";
            return;
        }

        var restrictions = GetRestrictionSummary(i);
        L_Stage[i].Text = restrictions;
    }

    private string GetRestrictionSummary(int i)
    {
        var parts = new System.Collections.Generic.List<string>();
        parts.Add($"Evo: {EvoOptions[CB_MaxEvo[i].SelectedIndex]}");
        parts.Add($"BST ≤ {NUD_BST[i].Value}");
        if (CHK_Legendaries[i].Checked) parts.Add("Legends ✓");
        if (CHK_Megas[i].Checked) parts.Add("Megas ✓");
        if (CHK_GymImportant[i].Checked) parts.Add("Gym→Next");
        return string.Join(" | ", parts);
    }

    private void LoadConfig()
    {
        for (int i = 0; i < 4; i++)
        {
            if (Stages[i] == null)
            {
                Stages[i] = new StageConfig();
                Stages[i].MaxEvolution = i switch { 0 => 1, 1 => 2, 2 => 3, _ => 3 };
                Stages[i].MaxBST = (ushort)(600 - (i * 50));
                Stages[i].AllowLegendaries = i >= 2;
                Stages[i].AllowMegas = i >= 1;
                Stages[i].GymBoostStage = i > 0;
            }

            CHK_Enabled[i].Checked = Stages[i].Enabled;
            CB_MaxEvo[i].SelectedIndex = Stages[i].MaxEvolution;
            NUD_BST[i].Value = Stages[i].MaxBST;
            CHK_Legendaries[i].Checked = Stages[i].AllowLegendaries;
            CHK_Megas[i].Checked = Stages[i].AllowMegas;
            CHK_GymImportant[i].Checked = Stages[i].GymBoostStage;
        }
    }

    private void BtnOK_Click(object sender, EventArgs e)
    {
        for (int i = 0; i < 4; i++)
        {
            Stages[i] = new StageConfig
            {
                Enabled = CHK_Enabled[i].Checked,
                MaxEvolution = CB_MaxEvo[i].SelectedIndex,
                MaxBST = (ushort)NUD_BST[i].Value,
                AllowLegendaries = CHK_Legendaries[i].Checked,
                AllowMegas = CHK_Megas[i].Checked,
                GymBoostStage = CHK_GymImportant[i].Checked,
            };
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    public static StageConfig GetStageForTrainer(string tag, int avgLevel, bool isImportant)
    {
        int stageIndex = GetStageIndex(tag, avgLevel, isImportant);
        if (stageIndex < 0 || stageIndex >= Stages.Length || !Stages[stageIndex].Enabled)
            return new StageConfig { Enabled = false };

        var stage = Stages[stageIndex];
        if (stage.GymBoostStage && isImportant && stageIndex < Stages.Length - 1)
            return Stages[stageIndex + 1];

        return stage;
    }

    private static int GetStageIndex(string tag, int avgLevel, bool isImportant)
    {
        if (tag.Contains("ELITE") || tag.Contains("CHAMPION"))
            return 3;
        if (tag.Contains("GYM7") || tag.Contains("GYM8"))
            return 2;
        if (tag.Contains("GYM4") || tag.Contains("GYM5") || tag.Contains("GYM6"))
            return 1;
        return 0;
    }
}

public class StageConfig
{
    public bool Enabled { get; set; } = true;
    public int MaxEvolution { get; set; }
    public ushort MaxBST { get; set; } = 600;
    public bool AllowLegendaries { get; set; }
    public bool AllowMegas { get; set; }
    public bool GymBoostStage { get; set; }
}