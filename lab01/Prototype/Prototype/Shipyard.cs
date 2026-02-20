// =============================================================================
// Shipyard.cs — Клиент паттерна Прототип (Prototype Client)
// =============================================================================
// Верфь хранит набор прототипов-образцов (Fighter, Cruiser, Bomber)
// и создаёт новые корабли путём клонирования этих прототипов.
// Это и есть суть паттерна: новые объекты создаются через Clone(),
// а не через прямой вызов конструкторов.
// =============================================================================

using Prototype.Models;

namespace Prototype
{
    /// Верфь — клиент паттерна Прототип.
    /// Хранит словарь прототипов и создаёт корабли путём их клонирования.
    public class Shipyard
    {
        // Словарь прототипов: ключ — тип корабля, значение — образец для клонирования
        private readonly Dictionary<string, Starship> _prototypes = new();

        /// Создаёт верфь с тремя прототипами по умолчанию.
        public Shipyard()
        {
            _prototypes["Fighter"] = new Fighter("Alpha", 60, 60, 30, 30, 180, Color.LightSkyBlue,
                new WeaponSystem(WeaponType.LaserCannon, 25));
            _prototypes["Cruiser"] = new Cruiser("Titan", 120, 120, 100, 100, 80, Color.Gold,
                new WeaponSystem(WeaponType.PlasmaTurret, 50));
            _prototypes["Bomber"] = new Bomber("Thunder", 150, 150, 60, 60, 50, Color.Salmon,
                new WeaponSystem(WeaponType.TorpedoBay, 80));
        }

        /// Возвращает прототип-образец для просмотра и редактирования в UI.
        public Starship GetPrototype(string type) => _prototypes[type];

        /// Настраивает прототип — задаёт характеристики перед клонированием.
        public void CustomizePrototype(string type, string name, int hull, int shield,
            int speed, Color color, WeaponType weaponType, int damage)
        {
            var proto = _prototypes[type];
            proto.Name = name;
            proto.HullStrength = hull;
            proto.MaxHull = hull;
            proto.ShieldLevel = shield;
            proto.MaxShield = shield;
            proto.Speed = speed;
            proto.ShipColor = color;
            proto.Weapon.Type = weaponType;
            proto.Weapon.Damage = damage;
        }

        /// Создаёт новый корабль путём клонирования прототипа — СУТЬ ПАТТЕРНА.
        public Starship BuildShip(string type) => _prototypes[type].Clone();
    }
}
