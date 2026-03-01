using Prototype.Models;

namespace Prototype
{
    public class ShipyardManual
    {
        // Храним текущие настройки для каждого типа, но не как прототипы —
        // просто как набор параметров. Нет механизма клонирования.
        private readonly Dictionary<string, Starship> _templates = new();

        public ShipyardManual()
        {
            _templates["Fighter"] = new Fighter("Alpha", 60, 60, 30, 30, 180, Color.LightSkyBlue,
                new WeaponSystem(WeaponType.LaserCannon, 25));
            _templates["Cruiser"] = new Cruiser("Titan", 120, 120, 100, 100, 80, Color.Gold,
                new WeaponSystem(WeaponType.PlasmaTurret, 50));
            _templates["Bomber"] = new Bomber("Thunder", 150, 150, 60, 60, 50, Color.Salmon,
                new WeaponSystem(WeaponType.TorpedoBay, 80));
        }

        public Starship GetPrototype(string type) => _templates[type];

        public void CustomizePrototype(string type, string name, int hull, int shield,
            int speed, Color color, WeaponType weaponType, int damage)
        {
            var tmpl = _templates[type];
            tmpl.Name = name;
            tmpl.HullStrength = hull;
            tmpl.MaxHull = hull;
            tmpl.ShieldLevel = shield;
            tmpl.MaxShield = shield;
            tmpl.Speed = speed;
            tmpl.ShipColor = color;
            tmpl.Weapon.Type = weaponType;
            tmpl.Weapon.Damage = damage;
        }

        // главное отличие: вместо Clone() — создаём новый объект напрямую.
        // Приходится проверять тип и вызывать нужный конструктор вручную.
        // При добавлении нового типа корабля этот switch нужно менять.
        // Оружие тоже создаём заново вручную — нет автоматического глубокого копирования.
        public Starship BuildShip(string type)
        {
            var tmpl = _templates[type];

            return type switch
            {
                "Fighter" => new Fighter(tmpl.Name, tmpl.HullStrength, tmpl.MaxHull,
                    tmpl.ShieldLevel, tmpl.MaxShield, tmpl.Speed, tmpl.ShipColor,
                    new WeaponSystem(tmpl.Weapon.Type, tmpl.Weapon.Damage)),

                "Cruiser" => new Cruiser(tmpl.Name, tmpl.HullStrength, tmpl.MaxHull,
                    tmpl.ShieldLevel, tmpl.MaxShield, tmpl.Speed, tmpl.ShipColor,
                    new WeaponSystem(tmpl.Weapon.Type, tmpl.Weapon.Damage)),

                "Bomber" => new Bomber(tmpl.Name, tmpl.HullStrength, tmpl.MaxHull,
                    tmpl.ShieldLevel, tmpl.MaxShield, tmpl.Speed, tmpl.ShipColor,
                    new WeaponSystem(tmpl.Weapon.Type, tmpl.Weapon.Damage)),

                _ => throw new ArgumentException($"Unknown ship type: {type}")
            };
        }
    }
}
