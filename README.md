# Loot Generator

A Windows Forms application for generating loot using a point based system. The program now mirrors all features of the original Python version. Items and materials can be edited through dedicated tabs and presets can be saved and loaded.

## Building

This project targets **.NET 8** and uses Windows Forms. You can open `LootGenWinForms.sln` in Visual Studio 2022 or build from the command line with the .NET SDK:

```bash
# restore and build
cd LootGenWinForms
dotnet build
```

## Running

After building, run the generated executable in the `bin/` directory or start the project from Visual Studio.

The main window contains three tabs:

* **Generate** – configure loot generation, manage presets and view all available tags.
* **Items** – view, add or remove loot items and bulk import using the `name|rarity|description|value|tags` format.
* **Materials** – edit material definitions with optional bulk import using the `name|modifier|type` format.

Click **Generate** to produce a list of loot items. Material placeholders in item names (e.g. `[Metal/o] Sword`) are resolved automatically.

All data files are located in the `data` directory and are copied next to the executable on build.
