using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LootGenWinForms
{
    public class AddItemForm : Form
    {
        private readonly TextBox txtName = new();
        private readonly NumericUpDown numRarity = new();
        private readonly TextBox txtDesc = new();
        private readonly NumericUpDown numValue = new();
        private readonly TextBox txtTags = new();
        private readonly Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK };
        private readonly Button btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

        public LootItem? Result { get; private set; }

        public AddItemForm()
        {
            Text = "Add Item";
            Width = 300;
            Height = 250;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 6, ColumnCount = 2 };
            Controls.Add(table);
            table.Controls.Add(new Label { Text = "Name" }, 0, 0);
            table.Controls.Add(txtName, 1, 0);
            table.Controls.Add(new Label { Text = "Rarity" }, 0, 1);
            numRarity.Minimum = 1; numRarity.Maximum = 100;
            table.Controls.Add(numRarity, 1, 1);
            table.Controls.Add(new Label { Text = "Description" }, 0, 2);
            table.Controls.Add(txtDesc, 1, 2);
            table.Controls.Add(new Label { Text = "Point Value" }, 0, 3);
            numValue.Minimum = 1; numValue.Maximum = 1000;
            table.Controls.Add(numValue, 1, 3);
            table.Controls.Add(new Label { Text = "Tags" }, 0, 4);
            table.Controls.Add(txtTags, 1, 4);
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            panel.Controls.Add(btnCancel);
            panel.Controls.Add(btnOk);
            table.Controls.Add(panel, 0, 5);
            table.SetColumnSpan(panel, 2);
            AcceptButton = btnOk;
            CancelButton = btnCancel;
            btnOk.Click += (_, _) =>
            {
                Result = new LootItem(
                    txtName.Text,
                    (int)numRarity.Value,
                    txtDesc.Text,
                    (int)numValue.Value,
                    txtTags.Text.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList());
            };
        }
    }
}
