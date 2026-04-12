"""Game Manager - central game state and logic orchestrator."""
import random
import time
from typing import List, Optional, Callable
from src.models import Loot, Weapon, Consumable, Armor, Ammo
from src.managers.player import Player
from src.managers.stash import Stash
from src.managers.flea_market import FleaMarketManager
from src import catalog


class Config:
    """All game balance constants — tweak everything here."""

    # === TIMING (seconds) ===
    HUNGER_INTERVAL = 2.0           # Main tick (hunger decay)
    RENT_INTERVAL = 30.0          # Rent due
    LOOT_DROP_INTERVAL = 1.0      # Loot drop attempt
    RAID_INTERVAL = 5.0           # Raid/shot attempt

    # === ECONOMY ===
    RENT_COST = 100000.0
    STASH_UPGRADE_COST = 50000.0

    # === SURVIVAL ===
    HUNGER_DECAY = 10.0            # Per tick
    STARVATION_DAMAGE = 2         # Per tick when hunger <= 0

    # === COMBAT ===
    RAID_CHANCE = 0.5             # 50% chance per raid tick
    RAID_ZONES = ["head", "thorax", "stomach"]

    # Caliber damage dict: {caliber: raw_damage_to_player}
    RAID_CALIBERS = {
        "9x19":     44,    # pistol — weak
        "5.45x39":  56,    # rifle — standard
        "5.56x45":  64,    # rifle — stronger
    }

    # Caliber damage to ITEMS (per shot)
    ITEM_DAMAGE = {
        "9x19":     1,    # pistol — 1 dmg per shot
        "5.45x39":  2,    # rifle — 2 dmg per shot
        "5.56x45":  3,    # rifle — 3 dmg per shot
    }

    # === LOOT ===
    LOOT_DROP_CHANCE = 0.7        # 70% chance per loot tick

    # === STASH ===
    STASH_COLS = 10
    STASH_ROWS = 10
    STASH_UPGRADE_ROWS = 1


class GameManager:
    """Central game orchestrator.

    Manages the game loop, player, stash, flea market,
    and connects visitors to game state.
    """

    def __init__(self):
        stash = Stash(cols=Config.STASH_COLS, rows=Config.STASH_ROWS)
        self.player = Player(stash=stash)
        self.market = FleaMarketManager()

        self._log_messages: List[str] = []
        self._game_over = False
        self._game_over_reason = ""
        self._last_time = time.time()
        self._hunger_timer = 0.0
        self._rent_timer = 0.0
        self._loot_timer = 0.0
        self._raid_timer = 0.0

        # Interaction state
        self._targeting_weapon: Optional[Weapon] = None
        self._ammo_selection: Optional[Ammo] = None
        self._compatible_weapons: List[Weapon] = []
        self._pending_removal: Optional[Loot] = None

        # Feedback flash (on-screen messages)
        self._feedback: List[tuple] = []  # (text, color, timer)

        # Drag state
        self._dragging_item: Optional[Loot] = None
        self._drag_origin_col: int = -1
        self._drag_origin_row: int = -1
        
        # Place some starting items
        self._spawn_starting_items()
    
    def _spawn_starting_items(self):
        """Add starting items from catalog."""
        for item_type, name in catalog.STARTING_LOADOUT:
            item = self._make_item(item_type, name)
            if item is not None:
                self.player.stash.add_item(item)

    def _make_item(self, item_type: str, name: str):
        """Create an item from catalog data."""
        if item_type == "weapon":
            for data in catalog.WEAPONS:
                if data[0] == name:
                    n, sx, sy, price, cal, mag = data
                    return Weapon(n, sx, sy, price, random.randint(50, 100), cal, mag, 0)
        elif item_type == "consumable":
            for data in catalog.CONSUMABLES:
                if data[0] == name:
                    n, sx, sy, price, cal = data
                    return Consumable(n, sx, sy, price, cal)
        elif item_type == "armor":
            for data in catalog.ARMORS:
                if data[0] == name:
                    n, sx, sy, price, cur, max_d, zones, mat = data
                    return Armor(n, sx, sy, price, cur, max_d, zones, mat)
        elif item_type == "ammo":
            for data in catalog.AMMO:
                if data[0] == name:
                    n, price, cal, tier, stack = data
                    return Ammo(n, 1, 1, price, cal, tier, stack)
        return None
    
    def update(self, dt: float):
        """Main update called every frame."""
        if self._game_over:
            return

        self._hunger_timer += dt
        self._rent_timer += dt
        self._loot_timer += dt
        self._raid_timer += dt

        # Update feedback flashes
        self._feedback = [
            (text, color, t - dt) for text, color, t in self._feedback if t > 0
        ]
        
        # Update flea market
        earnings = self.market.update(dt)
        for item, amount in earnings:
            self.player.money += amount
            self.log(f"Sold {item.name} for {amount:.0f}₽")
            self.player.stash.remove_item(item)
        
        # Rent check
        if self._rent_timer >= Config.RENT_INTERVAL:
            self._rent_timer = 0.0
            self.player.money -= Config.RENT_COST
            self.player.rents_paid += 1
            self.log(f"Stash rent: -{Config.RENT_COST:.0f}₽ (#{self.player.rents_paid})")
            self.flash_feedback(f"RENT: -{Config.RENT_COST:.0f}₽", "yellow")
            if self.player.money < 0:
                self._end_game("Cannot afford stash rent!")
        
        # Main tick (hunger, starvation)
        if self._hunger_timer >= Config.HUNGER_INTERVAL:
            self._hunger_timer = 0.0
            self._game_tick()

        # Loot drop
        if self._loot_timer >= Config.LOOT_DROP_INTERVAL:
            self._loot_timer = 0.0
            self._try_loot_drop()

        # Raid/shot
        if self._raid_timer >= Config.RAID_INTERVAL:
            self._raid_timer = 0.0
            self._try_raid()
    
    def _game_tick(self):
        """Process a game tick."""
        # Hunger decay
        self.player.hunger -= Config.HUNGER_DECAY

        if self.player.hunger <= 0:
            self.player.apply_starvation(Config.STARVATION_DAMAGE)
            self.log("STARVING! Taking damage!")
            self.flash_feedback("★ STARVING ★", "red")
            if not self.player.is_alive():
                self._end_game("Died from starvation!")
    
    def _try_loot_drop(self):
        """Attempt to drop a new loot item."""
        if random.random() > Config.LOOT_DROP_CHANCE:
            return
        
        new_item = self._generate_random_item()
        if not self.player.stash.can_fit(new_item):
            self._end_game("Stash is full! No space for new loot!")
            return
        
        self.player.stash.add_item(new_item)
        self.log(f"New loot appeared: {new_item.name}")
    
    def _try_raid(self):
        """Random raid/shot event with caliber-based damage."""
        if random.random() > Config.RAID_CHANCE:
            return

        # Pick a random caliber for this raid
        caliber = random.choice(list(Config.RAID_CALIBERS.keys()))
        damage = Config.RAID_CALIBERS[caliber]
        
        target_zone = random.choice(Config.RAID_ZONES)
        self.log(f"INCOMING! {caliber} round to {target_zone}!")
        self.flash_feedback(f"HIT: {caliber} → {target_zone}", "red")

        died = self.player.damage_zone(target_zone, damage)
        if died:
            self._end_game(f"Killed by {caliber} shot to {target_zone}!")
    
    def _generate_random_item(self) -> Loot:
        """Generate a random loot item from catalog."""
        item_type = random.choice(["weapon", "consumable", "armor", "ammo"])
        name = None

        if item_type == "weapon":
            name = random.choice(catalog.WEAPONS)[0]
        elif item_type == "consumable":
            name = random.choice(catalog.CONSUMABLES)[0]
        elif item_type == "armor":
            name = random.choice(catalog.ARMORS)[0]
        elif item_type == "ammo":
            name = random.choice(catalog.AMMO)[0]

        item = self._make_item(item_type, name)
        if item is None:
            return Consumable("MRE", 1, 1, 8000, 50)
        return item
    
    def sell_item(self, item: Loot):
        """Продать предмет (вызов item.sell() напрямую, без Visitor)."""
        item.sell(self.market)

    def use_item(self, item: Loot):
        """Использовать предмет (вызов item.use(self) напрямую, без Visitor)."""
        item.use(self)
    
    def mark_for_removal(self, item: Loot):
        """Mark an item for removal after use."""
        self._pending_removal = item
    
    def process_removals(self):
        """Process pending item removals (called after visitor completes)."""
        if self._pending_removal is not None:
            self.player.stash.remove_item(self._pending_removal)
            self._pending_removal = None
    
    def enter_targeting_mode(self, weapon: Weapon):
        """Enter weapon targeting mode."""
        self._targeting_weapon = weapon
        self.log(f"Select target for {weapon.name} ({weapon.loaded_ammo} rounds)")
    
    def enter_ammo_selection_mode(self, ammo: Ammo, weapons: List[Weapon]):
        """Enter ammo selection mode."""
        self._ammo_selection = ammo
        self._compatible_weapons = list(weapons)
        self.log(f"Select a weapon for {ammo.name}")
    
    def fire_at_stash_item(self, target_item: Loot):
        """Fire weapon at an item in the stash."""
        if self._targeting_weapon is None:
            return

        if self.market.is_listed(target_item):
            self.flash_feedback("Item is on market!", "warn")
            return

        weapon = self._targeting_weapon
        weapon.loaded_ammo -= 1
        weapon.durability -= 5

        # Caliber-based damage to item
        dmg = Config.ITEM_DAMAGE.get(weapon.caliber, 1)
        destroyed = target_item.take_damage(dmg)

        self.log(f"Shot {target_item.name} ({target_item.health}/{target_item.max_health} HP)")

        if destroyed:
            self.player.stash.remove_item(target_item)
            self.log(f"{target_item.name} destroyed!")

        # Check if weapon broke
        if weapon.durability <= 0:
            self.log(f"{weapon.name} is now broken!")
            self.flash_feedback(f"{weapon.name} BROKEN!", "red")
        
        self._targeting_weapon = None
    
    def load_ammo_into(self, weapon: Weapon):
        """Load selected ammo into selected weapon."""
        if self._ammo_selection is None:
            return
        self._ammo_selection._load_ammo(weapon, self)
        self._ammo_selection = None
        self._compatible_weapons = []
    
    def rotate_item(self, item: Loot):
        """Rotate an item in the stash."""
        pos = self.player.stash.find_item_position(item)
        if pos is None:
            self.log(f"Cannot find {item.name} in stash!")
            return

        col, row = pos
        old_sx, old_sy = item.size_x, item.size_y
        old_rotation = item._rotation

        # Temporarily change dimensions to check fit
        item.size_x, item.size_y = old_sy, old_sx
        item._rotation = (old_rotation + 90) % 360

        fits = self.player.stash._can_place_at(item, col, row)

        if fits:
            # Actually re-place with new dimensions
            # Clear old cells manually (since item now has new dimensions)
            for dy in range(old_sy):
                for dx in range(old_sx):
                    r, c = row + dy, col + dx
                    if 0 <= r < self.player.stash._rows and 0 <= c < self.player.stash._cols:
                        if self.player.stash._grid[r][c] is item:
                            self.player.stash._grid[r][c] = None
            self.player.stash._items = [(inv, ic, ir) for inv, ic, ir in self.player.stash._items if inv is not item]
            self.player.stash._place_item(item, col, row)
            self.log(f"Rotated {item.name}")
        else:
            # Revert
            item.size_x, item.size_y = old_sx, old_sy
            item._rotation = old_rotation
            self.log("Cannot rotate - no space!")
    
    def upgrade_stash(self):
        """Upgrade stash size."""
        if self.player.money >= Config.STASH_UPGRADE_COST:
            self.player.money -= Config.STASH_UPGRADE_COST
            self.player.stash.upgrade(rows=Config.STASH_UPGRADE_ROWS)
            self.log(f"Stash upgraded! +{Config.STASH_UPGRADE_ROWS} rows")
        else:
            self.log("Not enough money for stash upgrade!")
    
    def flash_feedback(self, text: str, color: str = "warn", duration: float = 1.5):
        """Show a brief on-screen feedback message."""
        self._feedback.append((text, color, duration))

    def drag_start(self, item: Loot, col: int, row: int):
        """Pick up item from stash for dragging."""
        if self.market.is_listed(item):
            self.flash_feedback("Item is on market!", "warn")
            return
        pos = self.player.stash.find_item_position(item)
        if pos is not None:
            self._dragging_item = item
            self._drag_origin_col = col
            self._drag_origin_row = row

    def drag_place(self, col: int, row: int) -> bool:
        """Try to place dragged item. Returns True if placed."""
        if self._dragging_item is None:
            return False
        success = self.player.stash.remove_and_place(self._dragging_item, col, row)
        self._dragging_item = None
        return success

    def drag_cancel(self):
        """Cancel drag - item stays where it was."""
        self._dragging_item = None

    @property
    def is_dragging(self) -> bool:
        return self._dragging_item is not None

    @property
    def rents_due_in(self) -> float:
        """Seconds until next rent payment."""
        return max(0, Config.RENT_INTERVAL - self._rent_timer)

    @property
    def dragging_item(self) -> Optional[Loot]:
        return self._dragging_item

    def cancel_interaction(self):
        """Cancel current interaction mode."""
        self._targeting_weapon = None
        self._ammo_selection = None
        self._compatible_weapons = []
    
    @property
    def is_targeting(self) -> bool:
        return self._targeting_weapon is not None
    
    @property
    def is_selecting_ammo(self) -> bool:
        return self._ammo_selection is not None
    
    @property
    def targeting_weapon(self) -> Optional[Weapon]:
        return self._targeting_weapon
    
    @property
    def ammo_selection(self) -> Optional[Ammo]:
        return self._ammo_selection
    
    @property
    def compatible_weapons(self) -> List[Weapon]:
        return list(self._compatible_weapons)
    
    def _end_game(self, reason: str):
        """End the game."""
        self._game_over = True
        self._game_over_reason = reason
        self.log(f"GAME OVER: {reason}")
    
    @property
    def is_game_over(self) -> bool:
        return self._game_over
    
    @property
    def game_over_reason(self) -> str:
        return self._game_over_reason
    
    def log(self, message: str):
        """Add a log message."""
        timestamp = time.strftime("%H:%M:%S")
        self._log_messages.append(f"[{timestamp}] {message}")
        # Keep only last 50 messages
        if len(self._log_messages) > 50:
            self._log_messages = self._log_messages[-50:]
    
    @property
    def log_messages(self) -> List[str]:
        return list(self._log_messages)
