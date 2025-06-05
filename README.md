# Loot Generator

An application designed for generating loot via a point-buy tag based system.

## Prerequisites

- Python 3.8 or newer (the GUI uses Tkinter which is included with the
  standard Python distribution)

## Installation

Clone the repository and install any dependencies (none are required beyond
Python itself):

```bash
git clone <repository-url>
cd lootgen
```

## Running the GUI

Launch the application by executing `loot_app.pyw` with Python:

```bash
python loot_generator/loot_app.pyw
```

This opens a window where you can generate loot, manage presets and maintain
the list of available items. Generated loot now includes each item's tags in
the output so you can easily see why an item was selected.

## Using Presets

Presets store common tag and point configurations. Select a preset from the
"Preset" drop-down in the GUI and click **Load Preset** to populate the input
fields. After adjusting the values, click **Save Preset** to create or update
a preset, or **Delete Preset** to remove it.

## Adding Items

Click **Add Item** to open a dialog where you can enter the item's name,
rarity, description, point value and tags. Items you add are saved to
`loot_items.json` and become available for future loot generation.

