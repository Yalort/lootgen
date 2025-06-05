import json
import os
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

BASE_DIR = os.path.dirname(__file__)


def _resolve(path: str) -> str:
    """Return absolute path relative to this module."""
    return os.path.join(BASE_DIR, path)


def load_loot_items(filepath=_resolve('data/loot_items.json')):
    """Load loot items from json file.

    The file may contain either a list of items or an object with
    ``items`` and ``tags`` keys. Only the item data is returned here.
    """
    with open(_resolve(filepath) if isinstance(filepath, str) and not os.path.isabs(filepath) else filepath, 'r') as file:
        data = json.load(file)

    if isinstance(data, dict):
        items_data = data.get("items", [])
    else:
        items_data = data

    return [LootItem(**item) for item in items_data]

def load_all_tags(filepath=_resolve('data/loot_items.json')):
    """Return the list of all tags stored in ``loot_items.json``.

    For backward compatibility, if the file does not contain a ``tags``
    key the tags are derived from the items.
    """
    with open(_resolve(filepath) if isinstance(filepath, str) and not os.path.isabs(filepath) else filepath, 'r') as file:
        data = json.load(file)

    if isinstance(data, dict) and "tags" in data:
        return data.get("tags", [])

    # Older format: derive tags from items list
    items = data if not isinstance(data, dict) else data.get("items", [])
    tags = sorted({tag for item in items for tag in item.get("tags", [])})
    return tags

def load_presets(filepath=_resolve('data/presets.json')):
    with open(_resolve(filepath) if isinstance(filepath, str) and not os.path.isabs(filepath) else filepath, 'r') as file:
        return json.load(file)

def save_presets(presets, filepath=_resolve('data/presets.json')):
    with open(_resolve(filepath) if isinstance(filepath, str) and not os.path.isabs(filepath) else filepath, 'w') as file:
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

    # Validate rarities and skip items with non-positive values
    invalid_items = [item for item in filtered_items if item.rarity <= 0]
    if invalid_items:
        filtered_items = [item for item in filtered_items if item.rarity > 0]
        if not filtered_items:
            invalid_names = ", ".join(item.name for item in invalid_items)
            raise ValueError(
                f"All filtered items have non-positive rarity: {invalid_names}"
            )

    # Filter out items with invalid or zero point values
    invalid_value_items = [item for item in filtered_items if item.point_value <= 0]
    if invalid_value_items:
        filtered_items = [item for item in filtered_items if item.point_value > 0]
        if not filtered_items:
            invalid_names = ", ".join(item.name for item in invalid_value_items)
            raise ValueError(
                f"All filtered items have non-positive point value: {invalid_names}"
            )

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
