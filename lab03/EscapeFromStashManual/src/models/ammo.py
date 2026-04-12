"""Ammo loot item."""
from .loot import Loot


class Ammo(Loot):
    """Ammo item with caliber and tier."""

    def __init__(
        self, name: str = "Ammo",
        size_x: int = 1, size_y: int = 1,
        base_price: float = 1000.0,
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

    # --- Без Visitor: логика прямо в классе ---

    def sell(self, market) -> None:
        tier_mod = {"normal": 1.0, "tracer": 1.5, "ap": 2.5}.get(self.tier, 1.0)
        price = self.base_price * tier_mod * self.stack_size
        market.list_item(self, price, 2.0)

    def use(self, game_manager) -> None:
        if game_manager.market.is_listed(self):
            game_manager.flash_feedback("Предмет на продаже!", "warn")
            return
        compatible = [
            item for item in game_manager.player.stash.items
            if item.__class__.__name__ == 'Weapon' and item.caliber == self.caliber
        ]
        if not compatible:
            game_manager.log(f"Нет совместимого оружия для {self.caliber}!")
            return
        if len(compatible) == 1:
            self._load_ammo(compatible[0], game_manager)
        else:
            game_manager.enter_ammo_selection_mode(self, compatible)

    def _load_ammo(self, weapon, game_manager) -> None:
        space = weapon.magazine_size - weapon.loaded_ammo
        if space <= 0:
            game_manager.log(f"Магазин {weapon.name} полон!")
            return
        loaded = min(space, self.stack_size)
        weapon.loaded_ammo += loaded
        self.stack_size -= loaded
        game_manager.log(f"Заряжено {loaded} патронов в {weapon.name}")
        if self.stack_size <= 0:
            game_manager.mark_for_removal(self)
