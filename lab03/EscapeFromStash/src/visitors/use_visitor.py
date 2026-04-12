"""UseVisitor - handles use logic for all loot types."""
from typing import Callable, List, Any
from src.visitors import Visitor
from src.models import Weapon, Consumable, Armor, Ammo


class UseContext:
    """Bridge between UseVisitor and GameManager state.
    
    This breaks the circular dependency: UseVisitor no longer imports GameManager.
    """
    def __init__(self, player: Any, stash: Any, market: Any, 
                 log_fn: Callable[[str], None], flash_fn: Callable[[str, str], None],
                 targeting_fn: Callable[[Any], None], ammo_select_fn: Callable[[Any, List[Any]], None],
                 mark_removal_fn: Callable[[Any], None]):
        self.player = player
        self.stash = stash
        self.market = market
        self.log = log_fn
        self.flash = flash_fn
        self.start_targeting = targeting_fn
        self.start_ammo_selection = ammo_select_fn
        self.mark_for_removal = mark_removal_fn


class UseVisitor(Visitor):
    """Visitor that handles item usage logic."""
    
    def __init__(self, ctx: UseContext):
        self._ctx = ctx
    
    def visit_weapon(self, weapon: 'Weapon') -> None:
        """Fire weapon: requires loaded ammo, destroys target item in line of fire."""
        if self._ctx.market.is_listed(weapon):
            self._ctx.flash("Item is on market!", "warn")
            return

        if weapon.loaded_ammo <= 0:
            self._ctx.log("Weapon has no ammo!")
            self._ctx.flash("No ammo!", "warn")
            return

        if weapon.durability <= 0:
            self._ctx.log("Weapon is broken!")
            self._ctx.flash("Weapon broken!", "warn")
            return

        # Enter targeting mode - player selects target
        self._ctx.start_targeting(weapon)

    def visit_consumable(self, consumable: 'Consumable') -> None:
        """Eat consumable: restores hunger."""
        if self._ctx.market.is_listed(consumable):
            self._ctx.flash("Item is on market!", "warn")
            return

        self._ctx.player.hunger += consumable.calories
        self._ctx.log(f"Ate {consumable.name}, +{consumable.calories} hunger")
        self._ctx.mark_for_removal(consumable)

    def visit_armor(self, armor: 'Armor') -> None:
        """Equip armor on its designated body zones."""
        if self._ctx.market.is_listed(armor):
            self._ctx.flash("Item is on market!", "warn")
            return

        player = self._ctx.player
        stash = self._ctx.stash
        
        for zone in armor.zones:
            old_armor = player.get_equipped_armor(zone)
            if old_armor is not None and old_armor is not armor:
                if stash.can_fit(old_armor):
                    stash.add_item(old_armor)
                    self._ctx.log(f"Unequipped {old_armor.name}")
                else:
                    self._ctx.log(f"No space for {old_armor.name}, it is destroyed!")
                player.equip_armor(None, zone)

        player.equip_armor(armor, armor.zones[0])
        zone_text = " + ".join(armor.zones)
        self._ctx.log(f"Equipped {armor.name} on {zone_text}")
        self._ctx.mark_for_removal(armor)

    def visit_ammo(self, ammo: 'Ammo') -> None:
        """Load ammo into a compatible weapon."""
        if self._ctx.market.is_listed(ammo):
            self._ctx.flash("Item is on market!", "warn")
            return

        compatible = [
            item for item in self._ctx.stash.items
            if isinstance(item, Weapon) and item.caliber == ammo.caliber
        ]

        if not compatible:
            self._ctx.log(f"No compatible weapons for {ammo.caliber} ammo!")
            return

        if len(compatible) == 1:
            self._load_ammo(compatible[0], ammo)
        else:
            self._ctx.start_ammo_selection(ammo, compatible)

    def _load_ammo(self, weapon: 'Weapon', ammo: 'Ammo') -> None:
        """Load ammo into a specific weapon."""
        space = weapon.magazine_size - weapon.loaded_ammo
        if space <= 0:
            self._ctx.log(f"{weapon.name} magazine is full!")
            return

        loaded = min(space, ammo.stack_size)
        weapon.loaded_ammo += loaded
        ammo.stack_size -= loaded
        self._ctx.log(f"Loaded {loaded} rounds into {weapon.name}")

        if ammo.stack_size <= 0:
            self._ctx.mark_for_removal(ammo)
