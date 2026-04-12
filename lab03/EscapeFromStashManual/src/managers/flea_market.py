"""Flea Market manager - handles selling with 3 slots and timed sales."""
from typing import List, Optional, Tuple
from src.models import Loot


class FleaMarketSlot:
    """Represents a single flea market listing."""
    
    def __init__(self, item: Loot, price: float, sell_time: float):
        self.item = item
        self.price = price
        self.sell_time = sell_time
        self.elapsed_time = 0.0
        self.sold = False
    
    def update(self, dt: float):
        """Update the slot timer."""
        if not self.sold:
            self.elapsed_time += dt
            if self.elapsed_time >= self.sell_time:
                self.sold = True
    
    @property
    def progress(self) -> float:
        """Sell progress from 0.0 to 1.0."""
        if self.sell_time <= 0:
            return 1.0
        return min(1.0, self.elapsed_time / self.sell_time)

    @property
    def time_left(self) -> float:
        """Time remaining in seconds."""
        return max(0, self.sell_time - self.elapsed_time)


class FleaMarketManager:
    """Manages the flea market with 3 slots and timed sales."""
    
    MAX_SLOTS = 3
    
    def __init__(self):
        self._slots: List[Optional[FleaMarketSlot]] = [None] * self.MAX_SLOTS
    
    @property
    def slots(self) -> List[Optional[FleaMarketSlot]]:
        return list(self._slots)
    
    @property
    def available_slots(self) -> int:
        return sum(1 for s in self._slots if s is None or s.sold)
    
    def list_item(self, item: Loot, price: float, sell_time: float) -> bool:
        """List an item for sale. Returns True if successfully listed."""
        # Can't list an item that's already on sale
        if self.is_listed(item):
            return False

        # First, clear sold slots
        self._collect_sold_items()
        
        # Find empty slot
        for i in range(self.MAX_SLOTS):
            if self._slots[i] is None:
                self._slots[i] = FleaMarketSlot(item, price, sell_time)
                return True
        
        return False  # No slots available

    def is_listed(self, item: Loot) -> bool:
        """Check if an item is currently on the market."""
        for slot in self._slots:
            if slot is not None and slot.item is item:
                return True
        return False

    def update(self, dt: float) -> List[Tuple[Loot, float]]:
        """Update all slots. Returns list of (sold_item, earnings)."""
        earnings = []
        self._collect_sold_items()
        
        for slot in self._slots:
            if slot is not None and not slot.sold:
                slot.update(dt)
                if slot.sold:
                    earnings.append((slot.item, slot.price))
        
        return earnings
    
    def _collect_sold_items(self):
        """Remove sold items from slots."""
        for i in range(self.MAX_SLOTS):
            if self._slots[i] is not None and self._slots[i].sold:
                self._slots[i] = None
    
    def get_slot_info(self, index: int) -> Optional['FleaMarketSlot']:
        """Get info about a specific slot."""
        if 0 <= index < self.MAX_SLOTS:
            return self._slots[index]
        return None