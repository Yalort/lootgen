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
        private readonly Dictionary<string, Preset> _presets;

        public MainForm()
        {
            // Load data before initializing UI components so that bindings
            // and preset lists have valid sources during initialization.
            _items = DataLoader.LoadLootItems();
            _materials = DataLoader.LoadMaterials();
            _presets = DataLoader.LoadPresets();

            InitializeComponent();
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
        private TabControl tabs = new TabControl();

        // generate tab controls
        private NumericUpDown numericPoints = new NumericUpDown();
        private TextBox textInclude = new TextBox();
        private TextBox textExclude = new TextBox();
        private NumericUpDown numericMax = new NumericUpDown();
        private NumericUpDown numericMin = new NumericUpDown();
        private TextBox textPresetSearch = new TextBox();
        private ListBox listPresets = new ListBox();
        private Button btnLoadPreset = new Button();
        private Button btnSavePreset = new Button();
        private Button btnDeletePreset = new Button();
        private Button btnGenerate = new Button();
        private Button btnShowTags = new Button();
        private TextBox textOutput = new TextBox();

        // item tab controls
        private DataGridView gridItems = new DataGridView();
        private Button btnAddItem = new Button();
        private Button btnDelItem = new Button();
        private Button btnBulkItems = new Button();
        private Button btnSaveItems = new Button();

        // material tab controls
        private DataGridView gridMaterials = new DataGridView();
        private Button btnAddMat = new Button();
        private Button btnDelMat = new Button();
        private Button btnBulkMat = new Button();
        private Button btnSaveMat = new Button();

        private void InitializeComponent()
        {
            Text = "Loot Generator";
            Width = 800;
            Height = 600;

            tabs.Dock = DockStyle.Fill;
            Controls.Add(tabs);

            var tabGenerate = new TabPage("Generate");
            var tabItems = new TabPage("Items");
            var tabMats = new TabPage("Materials");
            tabs.TabPages.AddRange(new[] { tabGenerate, tabItems, tabMats });

            // --- generate tab ---
            var gTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tabGenerate.Controls.Add(gTable);
            gTable.Controls.Add(new Label { Text = "Loot Points" }, 0, 0);
            numericPoints.Maximum = 1000;
            gTable.Controls.Add(numericPoints, 1, 0);

            gTable.Controls.Add(new Label { Text = "Include Tags" }, 0, 1);
            gTable.Controls.Add(textInclude, 1, 1);

            gTable.Controls.Add(new Label { Text = "Exclude Tags" }, 0, 2);
            gTable.Controls.Add(textExclude, 1, 2);

            gTable.Controls.Add(new Label { Text = "Max Rarity" }, 0, 3);
            numericMax.Maximum = 100;
            gTable.Controls.Add(numericMax, 1, 3);

            gTable.Controls.Add(new Label { Text = "Min Rarity" }, 0, 4);
            numericMin.Maximum = 100;
            gTable.Controls.Add(numericMin, 1, 4);

            gTable.Controls.Add(new Label { Text = "Search Presets" }, 0, 5);
            textPresetSearch.TextChanged += (_, _) => UpdatePresetList();
            gTable.Controls.Add(textPresetSearch, 1, 5);

            gTable.Controls.Add(new Label { Text = "Presets" }, 0, 6);
            listPresets.Height = 80;
            gTable.Controls.Add(listPresets, 1, 6);

            var presetBtnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
            btnLoadPreset.Text = "Load"; btnLoadPreset.Click += (_, _) => LoadSelectedPreset();
            btnSavePreset.Text = "Save"; btnSavePreset.Click += (_, _) => SaveCurrentPreset();
            btnDeletePreset.Text = "Delete"; btnDeletePreset.Click += (_, _) => DeleteSelectedPreset();
            presetBtnPanel.Controls.AddRange(new Control[] { btnLoadPreset, btnSavePreset, btnDeletePreset });
            gTable.Controls.Add(presetBtnPanel, 0, 7);
            gTable.SetColumnSpan(presetBtnPanel, 2);

            btnGenerate.Text = "Generate";
            btnGenerate.Click += OnGenerate;
            btnShowTags.Text = "Show Tags"; btnShowTags.Click += (_, _) => ShowAllTags();
            var genButtons = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
            genButtons.Controls.AddRange(new Control[] { btnGenerate, btnShowTags });
            gTable.Controls.Add(genButtons, 0, 8);
            gTable.SetColumnSpan(genButtons, 2);

            textOutput.Multiline = true;
            textOutput.ScrollBars = ScrollBars.Vertical;
            textOutput.Dock = DockStyle.Fill;
            gTable.Controls.Add(textOutput, 0, 9);
            gTable.SetColumnSpan(textOutput, 2);
            for (int i = 0; i < 10; i++) gTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            gTable.RowStyles[9] = new RowStyle(SizeType.Percent, 100);

            // --- items tab ---
            var iTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tabItems.Controls.Add(iTable);
            gridItems.Dock = DockStyle.Fill;
            gridItems.AutoGenerateColumns = true;
            iTable.Controls.Add(gridItems, 0, 0);
            iTable.SetColumnSpan(gridItems, 2);
            var itemBtnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
            btnAddItem.Text = "Add"; btnAddItem.Click += (_, _) => AddItem();
            btnDelItem.Text = "Delete"; btnDelItem.Click += (_, _) => DeleteSelectedItem();
            btnBulkItems.Text = "Bulk Add"; btnBulkItems.Click += (_, _) => BulkAddItems();
            btnSaveItems.Text = "Save"; btnSaveItems.Click += (_, _) => SaveItems();
            itemBtnPanel.Controls.AddRange(new Control[] { btnAddItem, btnDelItem, btnBulkItems, btnSaveItems });
            iTable.Controls.Add(itemBtnPanel, 0, 1);
            iTable.SetColumnSpan(itemBtnPanel, 2);
            iTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            iTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // --- materials tab ---
            var mTable = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            tabMats.Controls.Add(mTable);
            gridMaterials.Dock = DockStyle.Fill;
            gridMaterials.AutoGenerateColumns = true;
            mTable.Controls.Add(gridMaterials, 0, 0);
            mTable.SetColumnSpan(gridMaterials, 2);
            var matBtnPanel = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, Dock = DockStyle.Fill };
            btnAddMat.Text = "Add"; btnAddMat.Click += (_, _) => AddMaterial();
            btnDelMat.Text = "Delete"; btnDelMat.Click += (_, _) => DeleteSelectedMaterial();
            btnBulkMat.Text = "Bulk Add"; btnBulkMat.Click += (_, _) => BulkAddMaterials();
            btnSaveMat.Text = "Save"; btnSaveMat.Click += (_, _) => SaveMaterials();
            matBtnPanel.Controls.AddRange(new Control[] { btnAddMat, btnDelMat, btnBulkMat, btnSaveMat });
            mTable.Controls.Add(matBtnPanel, 0, 1);
            mTable.SetColumnSpan(matBtnPanel, 2);
            mTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // bind grids
            gridItems.DataSource = new BindingSource { DataSource = _items };
            gridMaterials.DataSource = new BindingSource { DataSource = _materials };
            UpdatePresetList();
        }

        private void AddItem()
        {
            using var f = new AddItemForm();
            if (f.ShowDialog() == DialogResult.OK && f.Result != null)
            {
                _items.Add(f.Result);
                gridItems.DataSource = new BindingSource { DataSource = _items };
            }
        }

        private void DeleteSelectedItem()
        {
            if (gridItems.CurrentRow?.DataBoundItem is LootItem item)
            {
                _items.Remove(item);
                gridItems.DataSource = new BindingSource { DataSource = _items };
            }
        }

        private void BulkAddItems()
        {
            using var f = new BulkAddForm<LootItem>(
                "Bulk Add Items",
                "Enter items as name|rarity|description|value|tag1,tag2",
                DataLoader.ParseItemsText);
            if (f.ShowDialog() == DialogResult.OK)
            {
                _items.AddRange(f.Results);
                gridItems.DataSource = new BindingSource { DataSource = _items };
            }
        }

        private void SaveItems()
        {
            DataLoader.SaveLootItems(_items);
        }

        private void AddMaterial()
        {
            using var f = new AddMaterialForm();
            if (f.ShowDialog() == DialogResult.OK && f.Result != null)
            {
                _materials.Add(f.Result);
                gridMaterials.DataSource = new BindingSource { DataSource = _materials };
            }
        }

        private void DeleteSelectedMaterial()
        {
            if (gridMaterials.CurrentRow?.DataBoundItem is Material mat)
            {
                _materials.Remove(mat);
                gridMaterials.DataSource = new BindingSource { DataSource = _materials };
            }
        }

        private void BulkAddMaterials()
        {
            using var f = new BulkAddForm<Material>(
                "Bulk Add Materials",
                "Enter materials as name|modifier|type",
                DataLoader.ParseMaterialsText);
            if (f.ShowDialog() == DialogResult.OK)
            {
                _materials.AddRange(f.Results);
                gridMaterials.DataSource = new BindingSource { DataSource = _materials };
            }
        }

        private void SaveMaterials()
        {
            DataLoader.SaveMaterials(_materials);
        }

        private void ShowAllTags()
        {
            var tags = _items.SelectMany(i => i.Tags).Distinct().OrderBy(t => t);
            MessageBox.Show(string.Join(", ", tags), "Tags");
        }

        private void UpdatePresetList()
        {
            var term = textPresetSearch.Text?.Trim().ToLowerInvariant() ?? string.Empty;
            listPresets.Items.Clear();
            foreach (var name in _presets.Keys.Where(n => n.ToLowerInvariant().Contains(term)))
                listPresets.Items.Add(name);
        }

        private void LoadSelectedPreset()
        {
            if (listPresets.SelectedItem is string name && _presets.TryGetValue(name, out var p))
            {
                numericPoints.Value = p.LootPoints;
                textInclude.Text = string.Join(", ", p.IncludeTags);
                textExclude.Text = string.Join(", ", p.ExcludeTags);
            }
        }

        private void SaveCurrentPreset()
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox("Preset name?", "Save Preset", "Preset");
            if (string.IsNullOrWhiteSpace(name)) return;
            var preset = new Preset(
                (int)numericPoints.Value,
                ParseTags(textInclude.Text) ?? new List<string>(),
                ParseTags(textExclude.Text) ?? new List<string>());
            _presets[name] = preset;
            DataLoader.SavePresets(_presets);
            UpdatePresetList();
        }

        private void DeleteSelectedPreset()
        {
            if (listPresets.SelectedItem is string name && _presets.Remove(name))
            {
                DataLoader.SavePresets(_presets);
                UpdatePresetList();
            }
        }
    }

    public record LootItem(string Name, int Rarity, string Description, int PointValue, List<string> Tags);

    public record Material(string Name, double Modifier, string Type);

    public record Preset(int LootPoints, List<string> IncludeTags, List<string> ExcludeTags);

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

        public static Dictionary<string, Preset> LoadPresets()
        {
            var path = DataPath("presets.json");
            if (!File.Exists(path)) return new();
            using var stream = File.OpenRead(path);
            var presets = JsonSerializer.Deserialize<Dictionary<string, Preset>>(stream);
            return presets ?? new();
        }

        public static void SavePresets(Dictionary<string, Preset> presets)
        {
            var path = DataPath("presets.json");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var opts = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(path, JsonSerializer.Serialize(presets, opts));
        }

        public static void SaveLootItems(List<LootItem> items)
        {
            var path = DataPath("loot_items.json");
            var tags = items.SelectMany(i => i.Tags).Distinct().OrderBy(t => t).ToList();
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var obj = new { items, tags };
            File.WriteAllText(path, JsonSerializer.Serialize(obj, opts));
        }

        public static void SaveMaterials(List<Material> materials)
        {
            var path = DataPath("materials.json");
            var opts = new JsonSerializerOptions { WriteIndented = true };
            var obj = new { materials };
            File.WriteAllText(path, JsonSerializer.Serialize(obj, opts));
        }

        public static List<LootItem> ParseItemsText(string text)
        {
            var list = new List<LootItem>();
            foreach (var line in text.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length != 5) throw new FormatException("Invalid item line");
                var tags = parts[4].Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();
                list.Add(new LootItem(parts[0].Trim(), int.Parse(parts[1]), parts[2].Trim(), int.Parse(parts[3]), tags));
            }
            return list;
        }

        public static List<Material> ParseMaterialsText(string text)
        {
            var list = new List<Material>();
            foreach (var line in text.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split('|');
                if (parts.Length != 3) throw new FormatException("Invalid material line");
                list.Add(new Material(parts[0].Trim(), double.Parse(parts[1]), parts[2].Trim()));
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
