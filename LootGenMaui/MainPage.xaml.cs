using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace LootGenMaui;

public partial class MainPage : ContentPage
{
    private readonly List<LootItem> _items;
    private readonly List<Material> _materials;

    public MainPage()
    {
        InitializeComponent();
        _items = DataLoader.LoadLootItems();
        _materials = DataLoader.LoadMaterials();
    }

    private void OnGenerate(object? sender, EventArgs e)
    {
        if (!int.TryParse(pointsEntry.Text, out int points))
        {
            DisplayAlert("Error", "Invalid points value", "OK");
            return;
        }
        var include = ParseTags(includeEntry.Text);
        var exclude = ParseTags(excludeEntry.Text);
        int? min = int.TryParse(minRarityEntry.Text, out int minVal) ? minVal : null;
        int? max = int.TryParse(maxRarityEntry.Text, out int maxVal) ? maxVal : null;
        var loot = LootLogic.GenerateLoot(_items, points, include, exclude, min, max, _materials);
        var lines = loot.Select(i => $"{i.Name} (Rarity: {i.Rarity}) - {i.Description}").ToList();
        resultsView.ItemsSource = lines;
    }

    private static List<string>? ParseTags(string? input) =>
        string.IsNullOrWhiteSpace(input) ? null : input.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToList();
}
