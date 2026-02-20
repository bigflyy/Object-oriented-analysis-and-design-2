// =============================================================================
// Cruiser.cs — Конкретный прототип: Крейсер
// =============================================================================
// Сбалансированный корабль с формой удлинённого шестиугольника и мостиком.
// Характеристики по умолчанию: средние значения всех параметров.
//
// Реализует метод Clone() — создаёт новый экземпляр Cruiser
// с глубокой копией WeaponSystem через Weapon.Clone().
// =============================================================================

namespace Prototype.Models
{
    /// Крейсер — конкретный прототип.
    /// Форма: удлинённый шестиугольник с секцией мостика (рисуется в ShipRenderer.DrawCruiser).
    /// По умолчанию: сбалансированные характеристики (Hull=120, Shield=100, Speed=80).
    public class Cruiser : Starship
    {
        public override string ShipType => "Cruiser";

        public Cruiser(string name, int hull, int maxHull, int shield, int maxShield,
            int speed, Color color, WeaponSystem weapon)
            : base(name, hull, maxHull, shield, maxShield, speed, color, weapon)
        {
        }

        /// Клонирование (паттерн Прототип) — создаёт НОВЫЙ Cruiser
        /// с копиями всех значений и ГЛУБОКОЙ копией WeaponSystem.
        /// Сохраняет максимальные значения Hull и Shield для возможности ремонта.
        public override Starship Clone()
        {
            return new Cruiser(Name, HullStrength, MaxHull, ShieldLevel, MaxShield,
                Speed, ShipColor, Weapon.Clone());
        }
    }
}
