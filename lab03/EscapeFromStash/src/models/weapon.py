"""Weapon loot item."""
from typing import TYPE_CHECKING
from .loot import Loot

if TYPE_CHECKING:
    from src.visitors import Visitor


class Weapon(Loot):
    """Weapon item with durability, caliber, and magazine."""
    
    def __init__(
        self,
        name: str = "Weapon",
        size_x: int = 2,
        size_y: int = 1,
        base_price: float = 50000.0,
        durability: int = 100,
        caliber: str = "5.56x45",
        magazine_size: int = 30,
        loaded_ammo: int = 0,

    ):
        super().__init__(size_x, size_y, base_price, name)
        self._durability = durability
        self._caliber = caliber
        self._magazine_size = magazine_size
        self._loaded_ammo = loaded_ammo

    
    @property
    def durability(self) -> int:
        return self._durability
    
    @durability.setter
    def durability(self, value: int):
        self._durability = max(0, min(100, value))
    
    @property
    def caliber(self) -> str:
        return self._caliber
    
    @property
    def magazine_size(self) -> int:
        return self._magazine_size
    
    @property
    def loaded_ammo(self) -> int:
        return self._loaded_ammo
    
    @loaded_ammo.setter
    def loaded_ammo(self, value: int):
        self._loaded_ammo = max(0, min(self._magazine_size, value))
    
    @property
    def direction(self) -> str:
        return self._direction
    
    @direction.setter
    def direction(self, value: str):
        if value in ("up", "down", "left", "right"):
            self._direction = value
    
    def accept(self, visitor: 'Visitor') -> None:
        visitor.visit_weapon(self)
