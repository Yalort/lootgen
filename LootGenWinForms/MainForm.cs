using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LootGenWinForms
{
    public partial class MainForm : Form
    {
        private readonly List<LootItem> _items;
        private readonly List<Material> _materials;

        public MainForm()
        {
            InitializeComponent();
            _items = DataLoader.LoadLootItems();
            _materials = DataLoader.LoadMaterials();
        }

        private void OnGenerate(object? sender, EventArgs e)
        {
            int points = (int)numericPoints.Value;
            var include = ParseTags(textInclude.Text);
            var exclude = ParseTags(textExclude.Text);
            int? maxRarity = numericMax.Value > 0 ? (int)numericMax.Value : null;
            int? minRarity = numericMin.Value > 0 ? (int)numericMin.Value : null;

            var loot = LootLogic.GenerateLoot(_items, points, include, exclude, minRarity, maxRarity, _materials);
            var lines = loot.Select(i => $"{i.Name} (Rarity: {i.Rarity}) - {i.Description}");
            textOutput.Lines = lines.ToArray();
        }

        private static List<string>? ParseTags(string input) =>
            string.IsNullOrWhiteSpace(input)
                ? null
                : input.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();
    }

    public partial class MainForm
    {
        private NumericUpDown numericPoints = new NumericUpDown();
        private TextBox textInclude = new TextBox();
        private TextBox textExclude = new TextBox();
        private NumericUpDown numericMax = new NumericUpDown();
        private NumericUpDown numericMin = new NumericUpDown();
        private Button btnGenerate = new Button();
        private TextBox textOutput = new TextBox();

        private void InitializeComponent()
        {
            Text = "Loot Generator";
            Width = 600;
            Height = 400;

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            Controls.Add(table);

            table.Controls.Add(new Label { Text = "Loot Points" }, 0, 0);
            numericPoints.Maximum = 1000;
            table.Controls.Add(numericPoints, 1, 0);

            table.Controls.Add(new Label { Text = "Include Tags" }, 0, 1);
            table.Controls.Add(textInclude, 1, 1);

            table.Controls.Add(new Label { Text = "Exclude Tags" }, 0, 2);
            table.Controls.Add(textExclude, 1, 2);

            table.Controls.Add(new Label { Text = "Max Rarity" }, 0, 3);
            numericMax.Maximum = 100;
            table.Controls.Add(numericMax, 1, 3);

            table.Controls.Add(new Label { Text = "Min Rarity" }, 0, 4);
            numericMin.Maximum = 100;
            table.Controls.Add(numericMin, 1, 4);

            btnGenerate.Text = "Generate";
            btnGenerate.Click += OnGenerate;
            table.Controls.Add(btnGenerate, 0, 5);
            table.SetColumnSpan(btnGenerate, 2);

            textOutput.Multiline = true;
            textOutput.ScrollBars = ScrollBars.Vertical;
            textOutput.Dock = DockStyle.Fill;
            table.Controls.Add(textOutput, 0, 6);
            table.SetColumnSpan(textOutput, 2);
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        }
    }

    public record LootItem(string Name, int Rarity, string Description, int PointValue, List<string> Tags);

    public record Material(string Name, double Modifier, string Type);

    public static class DataLoader
    {
        private static string DataPath(string file) => Path.Combine(AppContext.BaseDirectory, "data", file);

        public static List<LootItem> LoadLootItems()
        {
            var path = DataPath("loot_items.json");
            if (!File.Exists(path)) return new();
            using var stream = File.OpenRead(path);
            var root = JsonSerializer.Deserialize<JsonElement>(stream);
            var items = root.TryGetProperty("items", out var arr) ? arr : root;
            var list = new List<LootItem>();
            foreach (var item in items.EnumerateArray())
            {
                var tags = item.GetProperty("tags").EnumerateArray().Select(t => t.GetString() ?? "").ToList();
                list.Add(new LootItem(
                    item.GetProperty("name").GetString() ?? "",
                    item.GetProperty("rarity").GetInt32(),
                    item.GetProperty("description").GetString() ?? "",
                    item.GetProperty("point_value").GetInt32(),
                    tags));
            }
            return list;
        }

        public static List<Material> LoadMaterials()
        {
            var path = DataPath("materials.json");
            if (!File.Exists(path)) return new();
            using var stream = File.OpenRead(path);
            var root = JsonSerializer.Deserialize<JsonElement>(stream);
            var mats = root.TryGetProperty("materials", out var arr) ? arr : root;
            var list = new List<Material>();
            foreach (var mat in mats.EnumerateArray())
            {
                list.Add(new Material(
                    mat.GetProperty("name").GetString() ?? "",
                    mat.GetProperty("modifier").GetDouble(),
                    mat.GetProperty("type").GetString() ?? ""));
            }
            return list;
        }
    }

    public static class LootLogic
    {
        private static readonly Regex Placeholder = new(@"\[([A-Za-z/]+?)(?:(/o))?\]");

        public static List<LootItem> GenerateLoot(List<LootItem> items, int points,
            List<string>? includeTags = null, List<string>? excludeTags = null,
            int? minRarity = null, int? maxRarity = null, List<Material>? materials = null)
        {
            if (points <= 0) throw new ArgumentException("Points must be positive", nameof(points));
            var filtered = items.Where(item =>
                (includeTags == null || item.Tags.Intersect(includeTags).Any()) &&
                (excludeTags == null || !item.Tags.Intersect(excludeTags).Any()) &&
                (minRarity == null || item.Rarity >= minRarity) &&
                (maxRarity == null || item.Rarity <= maxRarity) &&
                item.Rarity > 0 && item.PointValue > 0).ToList();

            var loot = new List<LootItem>();
            var rnd = new Random();
            int total = 0;
            while (total < points)
            {
                var remaining = points - total;
                var available = filtered.Where(i => i.PointValue <= remaining).ToList();
                if (!available.Any()) break;
                var weights = available.Select(i => 1.0 / i.Rarity).ToArray();
                var index = WeightedChoice(weights, rnd);
                var item = available[index];
                if (materials != null)
                    item = ResolveMaterials(item, materials, rnd);
                loot.Add(item);
                total += item.PointValue;
            }
            return loot;
        }

        private static int WeightedChoice(double[] weights, Random rnd)
        {
            var sum = weights.Sum();
            var r = rnd.NextDouble() * sum;
            for (int i = 0; i < weights.Length; i++)
            {
                r -= weights[i];
                if (r <= 0) return i;
            }
            return weights.Length - 1;
        }

        private static LootItem ResolveMaterials(LootItem item, List<Material> mats, Random rnd)
        {
            double modifier = 1.0;
            string name = Placeholder.Replace(item.Name, m =>
            {
                var types = m.Groups[1].Value.Split('/');
                bool optional = m.Groups[2].Success;
                if (optional && rnd.NextDouble() < 0.5) return string.Empty;
                var options = mats.Where(mat => types.Any(t => string.Equals(t, mat.Type, StringComparison.OrdinalIgnoreCase))).ToList();
                if (!options.Any()) return string.Empty;
                var choice = options[rnd.Next(options.Count)];
                modifier *= choice.Modifier;
                return choice.Name;
            });
            int value = (int)Math.Round(item.PointValue * modifier);
            return item with { Name = name.Trim(), PointValue = value };
        }
    }
}
