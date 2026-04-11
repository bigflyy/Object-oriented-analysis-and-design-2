"""Armor loot item."""
from typing import TYPE_CHECKING
from .loot import Loot

if TYPE_CHECKING:
    from src.visitors import Visitor


class Armor(Loot):
    """Armor item that protects a specific body zone."""
    
    def __init__(
        self,
        name: str = "Armor",
        size_x: int = 2,
        size_y: int = 2,
        base_price: float = 80000.0,
        current_durability: int = 100,
        max_durability: int = 100,
        zones: tuple = ("thorax",),
        material_modifier: float = 1.0
    ):
        super().__init__(size_x, size_y, base_price, name)
        self._current_durability = current_durability
        self._max_durability = max_durability
        self._zones = list(zones)
        self._material_modifier = material_modifier
    
    @property
    def current_durability(self) -> int:
        return self._current_durability
    
    @current_durability.setter
    def current_durability(self, value: int):
        self._current_durability = max(0, value)
    
    @property
    def max_durability(self) -> int:
        return self._max_durability
    
    @property
    def zones(self) -> list:
        return list(self._zones)
    
    # Backwards compat: zone property for single-zone armors
    @property
    def zone(self) -> str:
        return self._zones[0] if self._zones else "thorax"
    
    @property
    def material_modifier(self) -> float:
        return self._material_modifier
    
    def accept(self, visitor: 'Visitor') -> None:
        visitor.visit_armor(self)
