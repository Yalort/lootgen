# Loot Generator

A simple Windows Forms application for generating loot using a point based system. The program loads item and material definitions from JSON files and lets you filter by tags and rarity.

## Building

This project targets **.NET 8** and uses Windows Forms. You can open `LootGenWinForms.sln` in Visual Studio 2022 or build from the command line with the .NET SDK:

```bash
# restore and build
cd LootGenWinForms
dotnet build
```

## Running

After building, run the generated executable in the `bin/` directory or start the project from Visual Studio.

The main window allows you to enter the number of loot points, include or exclude tags and limit item rarity. Click **Generate** to produce a list of loot items.

All data files are located in the `data` directory and are copied next to the executable on build.
