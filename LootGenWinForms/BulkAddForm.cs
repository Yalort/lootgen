using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace LootGenWinForms
{
    public class BulkAddForm<T> : Form
    {
        private readonly TextBox textArea = new() { Multiline = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill };
        private readonly Button btnOk = new() { Text = "Add", DialogResult = DialogResult.OK };
        private readonly Button btnCancel = new() { Text = "Cancel", DialogResult = DialogResult.Cancel };
        private readonly Func<string, List<T>> _parser;
        public List<T> Results { get; } = new();

        public BulkAddForm(string title, string instructions, Func<string, List<T>> parser)
        {
            _parser = parser;
            Text = title;
            Width = 500;
            Height = 300;
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            Controls.Add(table);
            table.Controls.Add(new Label { Text = instructions }, 0, 0);
            table.Controls.Add(textArea, 0, 1);
            var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            panel.Controls.Add(btnCancel); panel.Controls.Add(btnOk);
            table.Controls.Add(panel, 0, 2);
            AcceptButton = btnOk; CancelButton = btnCancel;
            btnOk.Click += (_, _) =>
            {
                Results.AddRange(_parser(textArea.Text));
            };
        }
    }
}
