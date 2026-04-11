"""Visitor pattern interfaces."""
from abc import ABC, abstractmethod
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from src.models import Weapon, Consumable, Armor, Ammo


class Visitor(ABC):
    """Abstract Visitor interface.
    
    Declares a visit method for each concrete loot type.
    """
    
    @abstractmethod
    def visit_weapon(self, weapon: 'Weapon') -> None:
        pass
    
    @abstractmethod
    def visit_consumable(self, consumable: 'Consumable') -> None:
        pass
    
    @abstractmethod
    def visit_armor(self, armor: 'Armor') -> None:
        pass
    
    @abstractmethod
    def visit_ammo(self, ammo: 'Ammo') -> None:
        pass
