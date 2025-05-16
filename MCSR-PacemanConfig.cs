using Newtonsoft.Json;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System;

// TODO: Set paceman_muted to False everytime this is run. (so initialisation works)
// TODO: add instructions on filepaths
public class CPHInline
{
    public PacemanForm form;
    public bool RunConfig()
    {
        form = new PacemanForm(CPH);
        form.Show();
        return true;
    }
}

public partial class PacemanForm : Form
{
    IInlineInvokeProxy cph;
    public PacemanForm(IInlineInvokeProxy cphButInFormClasses)
    {
        cph = cphButInFormClasses;
        InitializeComponent();
    }
}

partial class PacemanForm
{
    private System.ComponentModel.IContainer components = null;
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private int AddFieldGroup(Dictionary<string, string> fieldLabels, int startY, int labelX, int inputX, int spacing, Dictionary<string, Control> fieldMap)
    {
        foreach (var pair in fieldLabels)
        {
            var label = new Label
            {
                Text = pair.Value,
                Location = new Point(labelX, startY),
                Size = new Size(200, 20)
            };
            this.Controls.Add(label);
            var tb = new TextBox
            {
                Location = new Point(inputX, startY),
                Size = new Size(300, 20)
            };
            tb.Text = cph.GetGlobalVar<string>(pair.Key) ?? "";
            fieldMap[pair.Key] = tb;
            this.Controls.Add(tb);
            startY += spacing;
        }

        return startY;
    }

    private int AddIntFieldGroup(Dictionary<string, string> fieldLabels, int startY, int labelX, int inputX, int spacing, Dictionary<string, Control> fieldMap)
    {
        foreach (var pair in fieldLabels)
        {
            var label = new Label
            {
                Text = pair.Value,
                Location = new Point(labelX, startY),
                Size = new Size(200, 20)
            };
            this.Controls.Add(label);
            var nud = new NumericUpDown
            {
                Location = new Point(inputX, startY),
                Size = new Size(100, 20),
                Minimum = 0,
                Maximum = int.MaxValue,
                Value = cph.GetGlobalVar<int?>(pair.Key) ?? 0
            };
            fieldMap[pair.Key] = nud;
            this.Controls.Add(nud);
            startY += spacing;
        }

        return startY;
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.ClientSize = new System.Drawing.Size(600, 600);
        this.Name = "PacemanForm";
        this.Text = "Configure Paceman Settings";
        int labelX = 20;
        int inputX = 250;
        int spacing = 30;
        int currentY = 20;
        Dictionary<string, Control> fieldMap = new();
        // Splits Section
        var splitHelper = new Label
        {
            Text = "Configure your splits (ms). Any pace at or below these times will trigger mute events. Leave at 0 to disable.",
            Location = new Point(labelX, currentY),
            Size = new Size(550, 40),
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };
        this.Controls.Add(splitHelper);
        currentY += 50;
        var splitFields = new Dictionary<string, string>
        {
            ["paceman_enter_nether"] = "Enter Nether",
            ["paceman_first_structure"] = "First Structure",
            ["paceman_second_structure"] = "Second Structure",
            ["paceman_first_portal"] = "First Portal",
            ["paceman_enter_stronghold"] = "Stronghold",
            ["paceman_enter_end"] = "End Enter"
        };
        currentY = AddIntFieldGroup(splitFields, currentY, labelX, inputX, spacing, fieldMap);
        // WPState
        currentY += 20;
        var wpstateoutPathText1 = new Label
        {
            Text = "Path to wpstateout.txt in your MC instance",
            Location = new Point(labelX, currentY),
            Size = new Size(550, 20),
            Font = new Font("Segoe UI", 9, FontStyle.Regular)
        };
        this.Controls.Add(wpstateoutPathText1);
        currentY += 20;
        var wpstateoutPathText2 = new Label
        {
            Text = "e.g. C:/Users/<YOURNAME>/path/to/instance/.minecraft/wpstateout.txt",
            Location = new Point(labelX, currentY),
            Size = new Size(550, 20),
            Font = new Font("Segoe UI", 9, FontStyle.Italic)
        };
        this.Controls.Add(wpstateoutPathText2);
        currentY += 30;
        var wpstatoutField = new Dictionary<string, string>
        {
            ["paceman_wpstateoutPath"] = "WPstateout Path"
        };
        currentY = AddFieldGroup(wpstatoutField, currentY, labelX, inputX, spacing, fieldMap);
        // SpeedrunIGT
        currentY += 20;
        var speedrunIGTPathText1 = new Label
        {
            Text = "Path to speedrunigt folder",
            Location = new Point(labelX, currentY),
            Size = new Size(550, 20),
            Font = new Font("Segoe UI", 9, FontStyle.Regular),
            AutoSize = false
        };
        this.Controls.Add(speedrunIGTPathText1);
        currentY += 20;
        var speedrunIGTPathText2 = new Label
        {
            Text = "e.g. C:/Users/<YOURNAME>/speedrunigt",
            Location = new Point(labelX, currentY),
            Size = new Size(550, 20),
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            AutoSize = false
        };
        this.Controls.Add(speedrunIGTPathText2);
        currentY += 30;
        var igtPathField = new Dictionary<string, string>
        {
            ["paceman_igtPath"] = "IGT Path"
        };
        currentY = AddFieldGroup(igtPathField, currentY, labelX, inputX, spacing, fieldMap);
        // Save Button
        var saveButton = new Button
        {
            Text = "Save",
            Location = new Point(250, currentY + 20),
            Size = new Size(100, 30)
        };
        saveButton.Click += (s, e) =>
        {
            foreach (var pair in fieldMap)
            {
                object value = pair.Value switch
                {
                    TextBox tb => tb.Text,
                    NumericUpDown nud => (int)nud.Value,
                    _ => null
                };
                if (value != null)
                    cph.SetGlobalVar(pair.Key, value, true);
            }

            cph.SetGlobalVar("paceman_muted", false, true);
            MessageBox.Show("Settings saved!");
        };
        this.ClientSize = new Size(600, currentY + 100);
        this.Controls.Add(saveButton);
        this.ResumeLayout(false);
    }
}