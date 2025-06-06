using System;
using System.Linq;
using System.Windows.Forms;

namespace LootGenWinForms
{
    public class AddMaterialForm : Form
    {
        private readonly TextBox txtName = new();
        private readonly NumericUpDown numModifier = new();
        private readonly TextBox txtType = new();
        private readonly Button btnOk = new() { Text = "OK", DialogResult = DialogResult.OK };
        private readonly Button btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };

        public Material? Result { get; private set; }

        public AddMaterialForm()
        {
            Text = "Add Material";
            Width = 300;
            Height = 180;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 2 };
            Controls.Add(table);
            table.Controls.Add(new Label { Text = "Name" }, 0, 0);
            table.Controls.Add(txtName, 1, 0);
            table.Controls.Add(new Label { Text = "Modifier" }, 0, 1);
            numModifier.DecimalPlaces = 2; numModifier.Minimum = 0; numModifier.Maximum = 100;
            numModifier.Increment = 0.1M; table.Controls.Add(numModifier, 1, 1);
            table.Controls.Add(new Label { Text = "Type" }, 0, 2);
            table.Controls.Add(txtType, 1, 2);
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            panel.Controls.Add(btnCancel); panel.Controls.Add(btnOk);
            table.Controls.Add(panel, 0, 3); table.SetColumnSpan(panel, 2);
            AcceptButton = btnOk; CancelButton = btnCancel;
            btnOk.Click += (_, _) =>
            {
                Result = new Material(txtName.Text, (double)numModifier.Value, txtType.Text);
            };
        }
    }
}
