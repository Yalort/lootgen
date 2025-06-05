import os
import json
import random
import tempfile
import pytest
from loot_generator import utils


OLD_FORMAT_ITEMS = [
    {
        "name": "Sword",
        "rarity": 1,
        "description": "A sword",
        "point_value": 10,
        "tags": ["weapon", "melee"]
    },
    {
        "name": "Potion",
        "rarity": 2,
        "description": "Heals",
        "point_value": 5,
        "tags": ["consumable"]
    }
]


def test_resolve_returns_module_relative_path():
    path = utils._resolve('data/loot_items.json')
    expected = os.path.join(os.path.dirname(utils.__file__), 'data/loot_items.json')
    assert path == expected


def test_load_loot_items_returns_objects():
    items = utils.load_loot_items()
    assert items
    assert all(isinstance(i, utils.LootItem) for i in items)


def test_load_all_tags_from_tags_key():
    tags = utils.load_all_tags()
    assert 'weapon' in tags
    assert isinstance(tags, list)


def test_load_all_tags_fallback(tmp_path):
    tmp_file = tmp_path / 'items.json'
    with open(tmp_file, 'w') as fh:
        json.dump(OLD_FORMAT_ITEMS, fh)
    tags = utils.load_all_tags(str(tmp_file))
    assert set(tags) == {'weapon', 'melee', 'consumable'}


def test_load_and_save_presets_roundtrip(tmp_path):
    presets = {'Test': {'loot_points': 5, 'include_tags': ['weapon'], 'exclude_tags': []}}
    tmp_file = tmp_path / 'presets.json'
    utils.save_presets(presets, str(tmp_file))
    loaded = utils.load_presets(str(tmp_file))
    assert presets == loaded


def test_generate_loot_include_tags():
    items = utils.load_loot_items()
    random.seed(1)
    loot = utils.generate_loot(items, points=20, include_tags=['weapon'])
    assert loot
    assert all('weapon' in item.tags for item in loot)


def test_generate_loot_exclude_tags():
    items = utils.load_loot_items()
    random.seed(0)
    loot = utils.generate_loot(items, points=30, exclude_tags=['magic'])
    assert loot
    assert all('magic' not in item.tags for item in loot)


def test_generate_loot_rarity_filters():
    items = utils.load_loot_items()
    random.seed(0)
    loot = utils.generate_loot(items, points=30, min_rarity=2, max_rarity=3)
    assert loot
    assert all(2 <= item.rarity <= 3 for item in loot)


def test_generate_loot_no_items_when_filtered_out():
    items = utils.load_loot_items()
    random.seed(0)
    loot = utils.generate_loot(items, points=10, include_tags=['nonexistent'])
    assert loot == []


def test_generate_loot_invalid_point_values_raise():
    items = [
        utils.LootItem('Bad1', 1, '', 0, []),
        utils.LootItem('Bad2', 1, '', -5, []),
    ]
    with pytest.raises(ValueError):
        utils.generate_loot(items, points=5)


def test_parse_items_text_valid():
    text = "Sword|1|Sharp blade|10|weapon,melee"
    items = utils.parse_items_text(text)
    assert len(items) == 1
    item = items[0]
    assert item.name == "Sword"
    assert item.rarity == 1
    assert item.description == "Sharp blade"
    assert item.point_value == 10
    assert item.tags == ["weapon", "melee"]


def test_parse_items_text_invalid():
    text = "Bad|data"
    with pytest.raises(ValueError):
        utils.parse_items_text(text)
