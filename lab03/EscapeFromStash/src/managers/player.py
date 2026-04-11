"""Player class - manages character state."""
from typing import Dict, Optional
from src.models import Armor


class Player:
    """Player character with HP zones, hunger, money, and equipped armor."""
    
    # Zone HP values
    ZONE_HP = {
        "head": 35,
        "thorax": 80,
        "stomach": 70,
        "legs": 65,
    }
    
    CRITICAL_ZONES = {"head", "thorax"}
    
    def __init__(self, starting_money: float = 150000.0):
        self._hp: Dict[str, int] = dict(self.ZONE_HP)
        self._hunger: float = 100.0
        self._money: float = starting_money
        self._equipped_armor: Dict[str, Optional[Armor]] = {
            "head": None,
            "thorax": None,
            "stomach": None,
            "legs": None,
        }
        self.rents_paid: int = 0
    
    @property
    def hunger(self) -> float:
        return self._hunger
    
    @hunger.setter
    def hunger(self, value: float):
        self._hunger = max(0.0, min(100.0, value))
    
    @property
    def money(self) -> float:
        return self._money
    
    @money.setter
    def money(self, value: float):
        self._money = value
    
    def get_hp(self, zone: str) -> int:
        return self._hp.get(zone, 0)
    
    def set_hp(self, zone: str, value: int):
        if zone in self._hp:
            self._hp[zone] = max(0, value)
    
    def damage_zone(self, zone: str, damage: int) -> bool:
        """Damage a body zone. Returns True if player died."""
        if zone not in self._hp:
            return False

        # Check if any equipped armor covers this zone
        for equipped in self._equipped_armor.values():
            if equipped is not None and zone in equipped.zones:
                equipped.current_durability -= damage
                if equipped.current_durability <= 0:
                    for slot, arm in list(self._equipped_armor.items()):
                        if arm is equipped:
                            self._equipped_armor[slot] = None
                return False
        
        # No armor - apply damage to HP
        self._hp[zone] -= damage
        
        # If non-critical zone reaches 0, overflow goes to critical zones
        if zone not in self.CRITICAL_ZONES and self._hp[zone] <= 0:
            overflow = -self._hp[zone]
            self._hp[zone] = 0
            # Distribute overflow to critical zones 50/50
            for crit_zone in self.CRITICAL_ZONES:
                self._hp[crit_zone] -= overflow // 2
        
        return not self.is_alive()
    
    def is_alive(self) -> bool:
        """Player is alive if all critical zones have HP > 0."""
        return all(self._hp[zone] > 0 for zone in self.CRITICAL_ZONES)
    
    def get_equipped_armor(self, zone: str) -> Optional[Armor]:
        return self._equipped_armor.get(zone)
    
    def equip_armor(self, armor: Armor, zone: str):
        self._equipped_armor[zone] = armor
    
    def apply_starvation(self, damage: int):
        """Apply starvation damage to all zones."""
        for zone in self._hp:
            self._hp[zone] -= damage
    
    def get_hp_display(self) -> Dict[str, str]:
        """Return HP display info with status indicators."""
        result = {}
        for zone, max_hp in self.ZONE_HP.items():
            current = self._hp[zone]
            pct = current / max_hp
            if pct > 0.7:
                status = "green"
            elif pct > 0.3:
                status = "yellow"
            else:
                status = "red"
            result[zone] = f"{current}/{max_hp} ({status})"
        return result
