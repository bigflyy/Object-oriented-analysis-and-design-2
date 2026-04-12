"""Armor loot item."""
from .loot import Loot


class Armor(Loot):
    """Armor item that protects specific body zones."""

    def __init__(
        self, name: str = "Armor",
        size_x: int = 2, size_y: int = 2,
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

    @property
    def zone(self) -> str:
        return self._zones[0] if self._zones else "thorax"

    @property
    def material_modifier(self) -> float:
        return self._material_modifier

    # --- Без Visitor: логика прямо в классе ---

    def sell(self, market) -> None:
        durability_ratio = self.current_durability / self.max_durability
        price = self.base_price * durability_ratio * self.material_modifier
        market.list_item(self, price, 8.0)

    def use(self, game_manager) -> None:
        if game_manager.market.is_listed(self):
            game_manager.flash_feedback("Предмет на продаже!", "warn")
            return
        player = game_manager.player
        stash = game_manager.player.stash
        for zone in self.zones:
            old_armor = player.get_equipped_armor(zone)
            if old_armor is not None and old_armor is not self:
                if stash.can_fit(old_armor):
                    stash.add_item(old_armor)
                    game_manager.log(f"Снято: {old_armor.name}")
                else:
                    game_manager.log(f"Нет места для {old_armor.name}, уничтожена!")
                player.equip_armor(None, zone)
        player.equip_armor(self, self.zones[0])
        zone_text = " + ".join(self.zones)
        game_manager.log(f"Надето: {self.name} на {zone_text}")
        game_manager.mark_for_removal(self)
