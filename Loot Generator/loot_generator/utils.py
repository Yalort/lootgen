import json
import random
from dataclasses import dataclass
from typing import List

@dataclass
class LootItem:
    name: str
    rarity: str
    description: str
    point_value: int
    tags: List[str]

def load_loot_items(filepath='data/loot_items.json'):
    with open(filepath, 'r') as file:
        items = json.load(file)
    return [LootItem(**item) for item in items]

def load_presets(filepath='data/presets.json'):
    with open(filepath, 'r') as file:
        return json.load(file)

def save_presets(presets, filepath='data/presets.json'):
    with open(filepath, 'w') as file:
        json.dump(presets, file, indent=4)

def generate_loot(items, points, tags=None):
    filtered_items = [item for item in items if not tags or set(tags).intersection(item.tags)]
    random.shuffle(filtered_items)

    loot = []
    total_points = 0

    for item in filtered_items:
        if total_points + item.point_value <= points:
            loot.append(item)
            total_points += item.point_value

        if total_points == points:
            break

    return loot
