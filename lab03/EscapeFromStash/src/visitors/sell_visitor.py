"""SellVisitor - handles selling logic for all loot types."""
from typing import Optional
from src.visitors import Visitor
from src.models import Weapon, Consumable, Armor, Ammo
from src.managers.flea_market import FleaMarketManager


class SellVisitor(Visitor):
    """Visitor that calculates sell prices and lists items on the flea market."""
    
    def __init__(self, market_manager: 'FleaMarketManager'):
        self._market = market_manager
    
    # Price per calorie for consumables
    _PRICE_PER_CALORIE = 150.0
    
    # Tier modifiers for ammo
    _TIER_MODIFIERS = {
        "normal": 1.0,
        "ap": 2.5,
    }
    
    # Sell times (in seconds) for each type
    _SELL_TIMES = {
        "weapon": 15.0,
        "consumable": 3.0,
        "armor": 10.0,
        "ammo": 8.0,
    }
    
    def visit_weapon(self, weapon: 'Weapon') -> None:
        price = weapon.base_price * (weapon.durability / 100.0)
        sell_time = self._SELL_TIMES["weapon"]
        self._market.list_item(weapon, price, sell_time)
    
    def visit_consumable(self, consumable: 'Consumable') -> None:
        price = self._PRICE_PER_CALORIE * consumable.calories
        sell_time = self._SELL_TIMES["consumable"]
        self._market.list_item(consumable, price, sell_time)
    
    def visit_armor(self, armor: 'Armor') -> None:
        durability_ratio = armor.current_durability / armor.max_durability
        price = armor.base_price * durability_ratio * armor.material_modifier
        sell_time = self._SELL_TIMES["armor"]
        self._market.list_item(armor, price, sell_time)
    
    def visit_ammo(self, ammo: 'Ammo') -> None:
        tier_mod = self._TIER_MODIFIERS.get(ammo.tier, 1.0)
        price = ammo.base_price * tier_mod * ammo.stack_size
        sell_time = self._SELL_TIMES["ammo"]
        self._market.list_item(ammo, price, sell_time)
