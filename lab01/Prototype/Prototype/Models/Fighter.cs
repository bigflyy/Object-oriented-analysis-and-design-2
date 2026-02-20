// =============================================================================
// Fighter.cs — Конкретный прототип: Истребитель
// =============================================================================
// Быстрый лёгкий корабль с треугольной формой и стреловидными крыльями.
// Характеристики по умолчанию: высокая скорость, низкий корпус.
//
// Реализует метод Clone() — создаёт новый экземпляр Fighter
// с глубокой копией WeaponSystem через Weapon.Clone().
// =============================================================================

namespace Prototype.Models
{
    /// Истребитель — конкретный прототип.
    /// Форма: треугольник/стрела с крыльями (рисуется в ShipRenderer.DrawFighter).
    /// По умолчанию: быстрый (Speed=180), лёгкий корпус (Hull=60).
    public class Fighter : Starship
    {
        public override string ShipType => "Fighter";

        public Fighter(string name, int hull, int maxHull, int shield, int maxShield,
            int speed, Color color, WeaponSystem weapon)
            : base(name, hull, maxHull, shield, maxShield, speed, color, weapon)
        {
        }

        /// Клонирование (паттерн Прототип) — создаёт НОВЫЙ Fighter
        /// с копиями всех значений. Weapon.Clone() обеспечивает
        /// ГЛУБОКОЕ копирование ссылочного типа WeaponSystem.
        ///
        /// Сохраняет максимальные значения Hull и Shield для возможности ремонта.
        public override Starship Clone()
        {
            return new Fighter(Name, HullStrength, MaxHull, ShieldLevel, MaxShield,
                Speed, ShipColor, Weapon.Clone());
        }
    }
}
