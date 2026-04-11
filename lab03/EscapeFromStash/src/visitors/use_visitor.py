"""UseVisitor - handles use logic for all loot types."""
from typing import TYPE_CHECKING, List, Optional
from src.visitors import Visitor
from src.models import Weapon, Consumable, Armor, Ammo

if TYPE_CHECKING:
    from src.managers.game_manager import GameManager


class UseVisitor(Visitor):
    """Visitor that handles item usage logic.
    
    Requires references to the game manager for state manipulation.
    """
    
    def __init__(self, game: 'GameManager'):
        self._game = game
    
    def visit_weapon(self, weapon: 'Weapon') -> None:
        """Fire weapon: requires loaded ammo, destroys target item in line of fire."""
        if self._game.market.is_listed(weapon):
            self._game.flash_feedback("Item is on market!", "warn")
            return

        if weapon.loaded_ammo <= 0:
            self._game.log("Weapon has no ammo!")
            self._game.flash_feedback("No ammo!", "warn")
            return

        if weapon.durability <= 0:
            self._game.log("Weapon is broken!")
            self._game.flash_feedback("Weapon broken!", "warn")
            return
        
        # Enter targeting mode - player selects direction then target
        self._game.enter_targeting_mode(weapon)
    
    def visit_consumable(self, consumable: 'Consumable') -> None:
        """Eat consumable: restores hunger."""
        if self._game.market.is_listed(consumable):
            self._game.flash_feedback("Item is on market!", "warn")
            return

        player = self._game.player
        player.hunger += consumable.calories
        self._game.log(f"Ate {consumable.name}, +{consumable.calories} hunger")
        # Remove from stash (handled by game manager after visitor returns)
        self._game.mark_for_removal(consumable)
    
    def visit_armor(self, armor: 'Armor') -> None:
        """Equip armor on its designated body zones."""
        if self._game.market.is_listed(armor):
            self._game.flash_feedback("Item is on market!", "warn")
            return

        player = self._game.player
        
        for zone in armor.zones:
            old_armor = player.get_equipped_armor(zone)
            if old_armor is not None and old_armor is not armor:
                # Try to move old armor back to stash
                if self._game.stash.can_fit(old_armor):
                    self._game.stash.add_item(old_armor)
                    self._game.log(f"Unequipped {old_armor.name}")
                else:
                    self._game.log(f"No space for {old_armor.name}, it is destroyed!")
                player.equip_armor(None, zone)
        
        player.equip_armor(armor, armor.zones[0])
        zone_text = " + ".join(armor.zones)
        self._game.log(f"Equipped {armor.name} on {zone_text}")
        self._game.mark_for_removal(armor)
    
    def visit_ammo(self, ammo: 'Ammo') -> None:
        """Load ammo into a compatible weapon."""
        if self._game.market.is_listed(ammo):
            self._game.flash_feedback("Item is on market!", "warn")
            return

        # Find all compatible weapons in stash
        compatible = [
            item for item in self._game.stash.items
            if isinstance(item, Weapon) and item.caliber == ammo.caliber
        ]
        
        if not compatible:
            self._game.log(f"No compatible weapons for {ammo.caliber} ammo!")
            return
        
        if len(compatible) == 1:
            self._load_ammo(compatible[0], ammo)
        else:
            # Enter ammo selection mode
            self._game.enter_ammo_selection_mode(ammo, compatible)
    
    def _load_ammo(self, weapon: 'Weapon', ammo: 'Ammo') -> None:
        """Load ammo into a specific weapon."""
        space = weapon.magazine_size - weapon.loaded_ammo
        if space <= 0:
            self._game.log(f"{weapon.name} magazine is full!")
            return
        
        loaded = min(space, ammo.stack_size)
        weapon.loaded_ammo += loaded
        ammo.stack_size -= loaded
        self._game.log(f"Loaded {loaded} rounds into {weapon.name}")
        
        if ammo.stack_size <= 0:
            self._game.mark_for_removal(ammo)
