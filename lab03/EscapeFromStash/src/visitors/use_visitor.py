"""UseVisitor - handles use logic for all loot types."""
from typing import Callable, List, Any
from src.visitors import Visitor
from src.models import Weapon, Consumable, Armor, Ammo


class UseVisitor(Visitor):
    """Visitor that handles item usage logic.
    
    Takes specific dependencies instead of the whole GameManager
    to avoid circular dependencies.
    """
    
    def __init__(self, player: Any, market: Any, 
                 log_fn: Callable[[str], None], flash_fn: Callable[[str, str], None],
                 targeting_fn: Callable[[Any], None], ammo_select_fn: Callable[[Any, List[Any]], None],
                 mark_removal_fn: Callable[[Any], None]):
        self._player = player
        self._market = market
        self._log = log_fn
        self._flash = flash_fn
        self._start_targeting = targeting_fn
        self._start_ammo_selection = ammo_select_fn
        self._mark_removal = mark_removal_fn

    def visit_weapon(self, weapon: 'Weapon') -> None:
        """Fire weapon: requires loaded ammo, destroys target item in line of fire."""
        if self._market.is_listed(weapon):
            self._flash("Item is on market!", "warn")
            return

        if weapon.loaded_ammo <= 0:
            self._log("Weapon has no ammo!")
            self._flash("No ammo!", "warn")
            return

        if weapon.durability <= 0:
            self._log("Weapon is broken!")
            self._flash("Weapon broken!", "warn")
            return

        # Enter targeting mode - player selects target
        self._start_targeting(weapon)

    def visit_consumable(self, consumable: 'Consumable') -> None:
        """Eat consumable: restores hunger."""
        if self._market.is_listed(consumable):
            self._flash("Item is on market!", "warn")
            return

        self._player.hunger += consumable.calories
        self._log(f"Ate {consumable.name}, +{consumable.calories} hunger")
        self._mark_removal(consumable)

    def visit_armor(self, armor: 'Armor') -> None:
        """Equip armor on its designated body zones."""
        if self._market.is_listed(armor):
            self._flash("Item is on market!", "warn")
            return

        player = self._player
        stash = player.stash
        
        for zone in armor.zones:
            old_armor = player.get_equipped_armor(zone)
            if old_armor is not None and old_armor is not armor:
                if stash.can_fit(old_armor):
                    stash.add_item(old_armor)
                    self._log(f"Unequipped {old_armor.name}")
                else:
                    self._log(f"No space for {old_armor.name}, it is destroyed!")
                player.equip_armor(None, zone)

        player.equip_armor(armor, armor.zones[0])
        zone_text = " + ".join(armor.zones)
        self._log(f"Equipped {armor.name} on {zone_text}")
        self._mark_removal(armor)

    def visit_ammo(self, ammo: 'Ammo') -> None:
        """Load ammo into a compatible weapon."""
        if self._market.is_listed(ammo):
            self._flash("Item is on market!", "warn")
            return

        compatible = [
            item for item in self._player.stash.items
            if isinstance(item, Weapon) and item.caliber == ammo.caliber
        ]

        if not compatible:
            self._log(f"No compatible weapons for {ammo.caliber} ammo!")
            return

        if len(compatible) == 1:
            self._load_ammo(compatible[0], ammo)
        else:
            self._start_ammo_selection(ammo, compatible)

    def _load_ammo(self, weapon: 'Weapon', ammo: 'Ammo') -> None:
        """Load ammo into a specific weapon."""
        space = weapon.magazine_size - weapon.loaded_ammo
        if space <= 0:
            self._log(f"{weapon.name} magazine is full!")
            return

        loaded = min(space, ammo.stack_size)
        weapon.loaded_ammo += loaded
        ammo.stack_size -= loaded
        self._log(f"Loaded {loaded} rounds into {weapon.name}")

        if ammo.stack_size <= 0:
            self._mark_removal(ammo)
