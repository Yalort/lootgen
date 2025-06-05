import json
import random
from dataclasses import dataclass
from typing import List, Optional

@dataclass
class LootItem:
    name: str
    rarity: int
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

def generate_loot(
    items: List[LootItem],
    points: int,
    include_tags: Optional[List[str]] = None,
    exclude_tags: Optional[List[str]] = None,
    min_rarity: Optional[int] = None,
    max_rarity: Optional[int] = None,
):
    filtered_items = [
        item
        for item in items
        if (not include_tags or set(include_tags).intersection(item.tags))
        and (not exclude_tags or not set(exclude_tags).intersection(item.tags))
        and (min_rarity is None or item.rarity >= min_rarity)
        and (max_rarity is None or item.rarity <= max_rarity)
    ]

    loot = []
    total_points = 0

    if not filtered_items:
        return loot

    weights = [1/item.rarity for item in filtered_items]

    while total_points < points:
        item = random.choices(filtered_items, weights=weights, k=1)[0]
        if total_points + item.point_value <= points:
            loot.append(item)
            total_points += item.point_value
        else:
            break

    return loot