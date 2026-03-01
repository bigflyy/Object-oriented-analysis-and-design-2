namespace Prototype.Models
{
    /// Перечисление типов оружия.
    /// Каждый тип имеет уникальное визуальное отображение на корабле.
    public enum WeaponType
    {
        LaserCannon,   // Лазерная пушка — двойные красные лучи из носа корабля
        PlasmaTurret,  // Плазменная турель — фиолетовые сферы сверху и снизу корпуса
        MissileRack,   // Ракетная установка — оранжевые треугольники на крыльях
        TorpedoBay,    // Торпедный отсек — бирюзовые овалы над и под корпусом
        IonBeam        // Ионный луч — расширяющийся конус из носа корабля
    }

    /// Система вооружения корабля
    /// Содержит тип оружия и значение урона.
    /// Имеет собственный метод Clone() 
    public class WeaponSystem
    {
        public WeaponType Type { get; set; }

        public int Damage { get; set; }

        /// Название оружия. Свойство — определяется по значению Type.
        public string Name => Type switch
        {
            WeaponType.LaserCannon => "Laser Cannon",    // Лазерная пушка
            WeaponType.PlasmaTurret => "Plasma Turret",   // Плазменная турель
            WeaponType.MissileRack => "Missile Rack",     // Ракетная установка
            WeaponType.TorpedoBay => "Torpedo Bay",       // Торпедный отсек
            WeaponType.IonBeam => "Ion Beam",              // Ионный луч
            _ => "Unknown"                                 // Неизвестное оружие
        };
        public WeaponSystem(WeaponType type, int damage)
        {
            Type = type;
            Damage = damage;
        }

        /// Клонирование (глубокая копия) — создаёт новый объект WeaponSystem
        /// с теми же значениями Type и Damage.
        public WeaponSystem Clone()
        {
            return new WeaponSystem(Type, Damage);
        }

        public override string ToString() => $"{Name} (Dmg:{Damage})";
    }
}
