"""Consumable loot item."""
from .loot import Loot


class Consumable(Loot):
    """Consumable item that restores hunger."""

    def __init__(
        self, name: str = "Food",
        size_x: int = 1, size_y: int = 1,
        base_price: float = 5000.0,
        calories: int = 60
    ):
        super().__init__(size_x, size_y, base_price, name)
        self._calories = calories

    @property
    def calories(self) -> int:
        return self._calories

    # --- Без Visitor: логика прямо в классе ---

    def sell(self, market) -> None:
        price = 150.0 * self.calories
        market.list_item(self, price, 3.0)

    def use(self, game_manager) -> None:
        if game_manager.market.is_listed(self):
            game_manager.flash_feedback("Предмет на продаже!", "warn")
            return
        game_manager.player.hunger += self.calories
        game_manager.log(f"Съедено {self.name}, +{self.calories} голод")
        game_manager.mark_for_removal(self)
