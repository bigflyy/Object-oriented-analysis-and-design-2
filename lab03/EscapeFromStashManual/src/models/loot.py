"""Base Loot class - abstract base for all items."""
from abc import ABC, abstractmethod
from typing import Any


class Loot(ABC):
    """Abstract base class for all loot items.

    Each concrete class implements sell() and use() directly.
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
    def sell(self, market) -> None:
        """Sell this item on the flea market."""
        pass

    @abstractmethod
    def use(self, game_manager) -> None:
        """Use this item."""
        pass
