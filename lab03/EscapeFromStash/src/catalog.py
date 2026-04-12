"""Item catalog — all static item definitions.

Each tuple is: (name, size_x, size_y, base_price, ...rest)
Tweak prices, sizes, and stats here.
"""
import random
from src.models import Weapon, Consumable, Armor, Ammo, Loot

# === WEAPONS ===
# (name, sx, sy, base_price, caliber, magazine_size)
WEAPONS = [
    ("ak74m",   4, 2, 45000, "5.45x39",  30),
    ("m4a1",    4, 2, 60000, "5.56x45",  30),
    ("mp443",   2, 1, 15000, "9x19",     18),
]

# === CONSUMABLES ===
# (name, sx, sy, base_price, calories)
CONSUMABLES = [
    ("MRE",          1, 1, 8000,  50),
    ("chocolatepng", 1, 1, 4000,  30),
    ("tushonka",     1, 1, 12000, 80),
    ("water",        1, 1, 3000,  20),
]

# === ARMOR ===
# (name, sx, sy, base_price, cur_dur, max_dur, zones, mat_mod)
ARMORS = [
    ("6b13ke", 2, 3, 80000, 154, 154, ("thorax", "stomach"), 1.5),
    ("maska",  2, 2, 25000,  108,  108,  ("head",),             1.5),
    ("SSh-68",  2, 2, 20000,  54,  54,  ("head",),             1.0),
    ("PACA",   2, 2, 20000,  80,  80,  ("thorax",),           1.0),
]

# === AMMO ===
# (name, base_price, caliber, tier, stack_size)
AMMO = [
    ("5.56x45M856", 300,  "5.56x45", "ap",     10),
    ("5.45x39PS",   100,  "5.45x39", "normal", 15),
    ("9x19PBP",     200,  "9x19",    "ap",     20),
]

# === STARTING LOADOUT ===
# Items the player starts with: list of ("type_name", "item_name_from_catalog")
STARTING_LOADOUT = [
    ("weapon",       "ak74m"),
    ("consumable",   "MRE"),
    ("consumable",   "water"),
    ("armor",        "PACA"),
    ("armor",       "SSh-68"),
    ("ammo",         "5.45x39PS"),
]
