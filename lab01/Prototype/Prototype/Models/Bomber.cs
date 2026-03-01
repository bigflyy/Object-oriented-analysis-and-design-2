// Bomber.cs — Конкретный прототип: Бомбардировщик
// Тяжёлый медленный корабль с широким прямоугольным корпусом и плавниками.
// Характеристики по умолчанию: высокая прочность корпуса, низкая скорость.
//
// Реализует метод Clone() — создаёт новый экземпляр Bomber
// с глубокой копией WeaponSystem через Weapon.Clone().

namespace Prototype.Models
{
    public class Bomber : Starship
    {
        public override string ShipType => "Bomber";

        public Bomber(string name, int hull, int maxHull, int shield, int maxShield,
            int speed, Color color, WeaponSystem weapon)
            : base(name, hull, maxHull, shield, maxShield, speed, color, weapon)
        {
        }

        /// Клонирование (паттерн Прототип) — создаёт новый Bomber
        /// с копиями всех значений и глубокой копией WeaponSystem (новый объект, а не ссылка на тот же самый).
        /// Сохраняет максимальные значения Hull и Shield для возможности ремонта.
        public override Starship Clone()
        {
            return new Bomber(Name, HullStrength, MaxHull, ShieldLevel, MaxShield,
                Speed, ShipColor, Weapon.Clone());
        }
    }
}
