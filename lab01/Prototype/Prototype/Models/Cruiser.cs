// Cruiser.cs — Конкретный прототип: Крейсер
// Сбалансированный корабль с формой удлинённого шестиугольника и мостиком.
// Характеристики по умолчанию: средние значения всех параметров.
//
// Реализует метод Clone() — создаёт новый экземпляр Cruiser
// с глубокой копией WeaponSystem через Weapon.Clone().

namespace Prototype.Models
{
    public class Cruiser : Starship
    {
        public override string ShipType => "Cruiser";

        public Cruiser(string name, int hull, int maxHull, int shield, int maxShield,
            int speed, Color color, WeaponSystem weapon)
            : base(name, hull, maxHull, shield, maxShield, speed, color, weapon)
        {
        }

        /// Клонирование (паттерн Прототип) — создаёт НОВЫЙ Cruiser
        /// с копиями всех значений и глубокой копией WeaponSystem (новый объект, а не ссылка на тот же самый).
        /// Сохраняет максимальные значения Hull и Shield для возможности ремонта.
        public override Starship Clone()
        {
            return new Cruiser(Name, HullStrength, MaxHull, ShieldLevel, MaxShield,
                Speed, ShipColor, Weapon.Clone());
        }
    }
}
