using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LootGenMaui;

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
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<string, Preset>>(json) ?? new();
    }

    public static void SavePresets(Dictionary<string, Preset> presets)
    {
        var path = DataPath("presets.json");
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
