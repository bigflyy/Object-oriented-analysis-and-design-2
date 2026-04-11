"""Consumable loot item."""
from typing import TYPE_CHECKING
from .loot import Loot

if TYPE_CHECKING:
    from src.visitors import Visitor


class Consumable(Loot):
    """Consumable item that restores hunger."""
    
    def __init__(
        self,
        name: str = "Food",
        size_x: int = 1,
        size_y: int = 1,
        base_price: float = 5000.0,
        calories: int = 60
    ):
        super().__init__(size_x, size_y, base_price, name)
        self._calories = calories
    
    @property
    def calories(self) -> int:
        return self._calories
    
    def accept(self, visitor: 'Visitor') -> None:
        visitor.visit_consumable(self)
