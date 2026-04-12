"""Weapon loot item."""
from .loot import Loot


class Weapon(Loot):
    """Weapon item with durability, caliber, and magazine."""

    def __init__(
        self, name: str = "Weapon",
        size_x: int = 2, size_y: int = 1,
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

    # --- Без Visitor: логика прямо в классе ---

    def sell(self, market) -> None:
        price = self.base_price * (self.durability / 100.0)
        market.list_item(self, price, 15.0)

    def use(self, game_manager) -> None:
        if game_manager.market.is_listed(self):
            game_manager.flash_feedback("Предмет на продаже!", "warn")
            return
        if self.loaded_ammo <= 0:
            game_manager.log("Оружие без патронов!")
            game_manager.flash_feedback("Нет патронов!", "warn")
            return
        if self.durability <= 0:
            game_manager.log("Оружие сломано!")
            game_manager.flash_feedback("Оружие сломано!", "warn")
            return
        game_manager.enter_targeting_mode(self)
