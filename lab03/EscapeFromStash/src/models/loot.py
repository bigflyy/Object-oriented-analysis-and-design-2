"""Base Loot class - abstract base for all items."""
from abc import ABC, abstractmethod
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from src.visitors import Visitor


class Loot(ABC):
    """Abstract base class for all loot items.
    
    Stores common item data and declares the Accept method for the Visitor pattern.
    """
    
    def __init__(self, size_x: int = 1, size_y: int = 1, base_price: float = 0.0, name: str = "Item"):
        self._size_x = size_x
        self._size_y = size_y
        self._base_price = base_price
        self._name = name
        self._health = size_x * size_y  # HP = slot count
        self._max_health = self._health
        self._rotation = 0  # Visual rotation angle in degrees

    @property
    def rotation(self) -> int:
        return self._rotation

    @property
    def size_x(self) -> int:
        return self._size_x

    @property
    def size_y(self) -> int:
        return self._size_y

    @size_x.setter
    def size_x(self, value: int):
        self._size_x = value
        self._max_health = self._size_x * self._size_y

    @size_y.setter
    def size_y(self, value: int):
        self._size_y = value
        self._max_health = self._size_x * self._size_y

    @property
    def health(self) -> int:
        return self._health

    @health.setter
    def health(self, value: int):
        self._health = max(0, min(self._max_health, value))

    @property
    def max_health(self) -> int:
        return self._max_health

    def take_damage(self, amount: int) -> bool:
        """Apply damage. Returns True if item is destroyed."""
        self._health -= amount
        if self._health <= 0:
            self._health = 0
            return True
        return False
    
    @property
    def base_price(self) -> float:
        return self._base_price
    
    @property
    def name(self) -> str:
        return self._name
    
    def rotate(self) -> bool:
        """Swap width and height. Returns True if rotation succeeded."""
        self._size_x, self._size_y = self._size_y, self._size_x
        self._rotation = (self._rotation + 90) % 360
        return True
    
    @abstractmethod
    def accept(self, visitor: 'Visitor') -> None:
        """Accept a visitor (double dispatch)."""
        pass
    
    def __repr__(self) -> str:
        return f"{self.__class__.__name__}({self._name}, {self._size_x}x{self._size_y})"
