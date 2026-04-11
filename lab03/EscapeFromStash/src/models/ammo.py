"""Ammo loot item."""
from typing import TYPE_CHECKING
from .loot import Loot

if TYPE_CHECKING:
    from src.visitors import Visitor


class Ammo(Loot):
    """Ammo item with caliber and tier."""
    
    def __init__(
        self,
        name: str = "Ammo",
        size_x: int = 1,
        size_y: int = 1,
        base_price: float = 100.0,
        caliber: str = "5.56x45",
        tier: str = "normal",
        stack_size: int = 30
    ):
        super().__init__(size_x, size_y, base_price, name)
        self._caliber = caliber
        self._tier = tier
        self._stack_size = stack_size
    
    @property
    def caliber(self) -> str:
        return self._caliber
    
    @property
    def tier(self) -> str:
        return self._tier
    
    @property
    def stack_size(self) -> int:
        return self._stack_size
    
    @stack_size.setter
    def stack_size(self, value: int):
        self._stack_size = max(0, value)
    
    def accept(self, visitor: 'Visitor') -> None:
        visitor.visit_ammo(self)
