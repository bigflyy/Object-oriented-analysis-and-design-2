# Escape From Stash

A Tarkov-inspired inventory survival game built with Python + Pygame. Demonstrates the **Visitor pattern** for separating loot data from game operations (Sell / Use).

## How to Run

```bash
cd lab03/EscapeFromStash
pip install -r requirements.txt
python main.py
```

## Controls

| Input | Action |
|-------|--------|
| **Right-click** item | Use item |
| **Shift + Right-click** item | Sell item on flea market |
| **Middle-click** item | Rotate item |
| **Left-click** (targeting mode) | Fire weapon at selected item |
| **Left-click** (ammo select) | Load ammo into selected weapon |
| **ESC** | Cancel current action |
| **U** | Upgrade stash (costs 50,000₽) |
| **R** (game over screen) | Restart game |

## Gameplay

You manage a stash (inventory grid) that periodically fills with random loot. Your character's hunger decays over time. Three lose conditions:

1. **HP reaches 0** — from starvation or raid shots to critical zones (head, thorax)
2. **Can't afford stash rent** — rent costs 5,000₽ every 30 seconds
3. **Stash overflows** — new loot drops with no space

### Core Mechanics

- **Sell** items on the flea market (3 slots max, each type has unique sell time)
- **Use** items:
  - **Weapon** — enter targeting mode, click any item to destroy (costs 1 round from magazine, −5 durability)
  - **Consumable** — eat to restore hunger
  - **Armor** — equip on its body zone (replaces existing armor)
  - **Ammo** — highlights compatible weapons, click one to load
- **Rotate** items to fit them better in the grid

## Architecture (Visitor Pattern)

```
Loot (abstract)
├── Weapon
├── Consumable
├── Armor
└── Ammo

Visitor (abstract)
├── SellVisitor  → FleaMarketManager (price calculation, timed sales)
└── UseVisitor   → GameManager (use logic, state mutation)
```

Loot classes are pure data containers. All operations are delegated through visitors via double dispatch:

```python
item.accept(visitor)  # Routes to the correct VisitXxx method automatically
```

### Class Diagram

See `EscapeFromStash.drawio` for the full UML class diagram.

## Image Assets

The game works without images (renders colored boxes with text). To add item images, place `.png` files named exactly as the item names in `assets/images/`:

### Required images (all `.png`, any size, will be auto-scaled to grid cells):

**Weapons:**
- `AK-74M.png`
- `M4A1.png`
- `MP-443.png`
- `SVD.png`

**Consumables:**
- `MRE.png`
- `Water.png`
- `Chocolate.png`
- `Tushkan.png`

**Armor:**
- `PACA.png`
- `6B13.png`
- `SH-20.png`
- `Leg Armor.png`

**Ammo:**
- `5.45x39 PS.png`
- `5.56x45 M855.png`
- `9x19 PBP.png`

> Minimal Tarkov-style icons work great — just small transparent PNGs of weapon/item silhouettes.
