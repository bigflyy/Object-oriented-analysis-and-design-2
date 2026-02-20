// =============================================================================
// Starship.cs — Абстрактный базовый класс звёздного корабля
// =============================================================================
// Этот класс реализует общую логику для всех типов кораблей:
//   - Хранит все свойства (имя, корпус, щит, скорость, цвет, оружие)
//   - Реализует метод GetInfo() для текстового описания
//   - Объявляет абстрактные члены: ShipType и Clone()
//
// Конкретные типы кораблей (Fighter, Cruiser, Bomber) наследуют этот класс
// и реализуют свои версии Clone() — каждый создаёт экземпляр СВОЕГО типа.
// =============================================================================

namespace Prototype.Models
{
    /// Абстрактный базовый класс для всех типов кораблей (Прототип).
    public abstract class Starship
    {
        // --- Свойства корабля ---

        public string Name { get; set; }
        public int HullStrength { get; set; }
        public int MaxHull { get; set; }
        public int ShieldLevel { get; set; }
        public int MaxShield { get; set; }
        public int Speed { get; set; }
        public Color ShipColor { get; set; }
        public WeaponSystem Weapon { get; set; }

        /// Абстрактное свойство — каждый подкласс возвращает свой тип:
        /// "Fighter", "Cruiser" или "Bomber".
        /// Используется при отрисовке для выбора формы корабля.
        public abstract string ShipType { get; }

        /// Защищённый конструктор — вызывается только из конструкторов подклассов.
        protected Starship(string name, int hull, int maxHull, int shield, int maxShield,
            int speed, Color color, WeaponSystem weapon)
        {
            Name = name;
            HullStrength = hull;
            MaxHull = maxHull;
            ShieldLevel = shield;
            MaxShield = maxShield;
            Speed = speed;
            ShipColor = color;
            Weapon = weapon;
        }

        /// Абстрактный метод клонирования — СУТЬ ПАТТЕРНА ПРОТОТИП.
        /// Каждый подкласс реализует его, создавая новый экземпляр своего типа
        /// и вызывая Weapon.Clone() для глубокого копирования оружия.
        public abstract Starship Clone();

        /// Возвращает строку с полной информацией о корабле.
        /// Формат: Тип "Имя" | Корпус:XX Щит:XX Скорость:XX | Оружие (Урон:XX)
        /// Используется для отображения в списке флота (ListBox).
        public string GetInfo()
        {
            return $"{ShipType} \"{Name}\" | Hull:{HullStrength} Shield:{ShieldLevel} Speed:{Speed} | {Weapon}";
        }

        /// Ремонтирует корабль — восстанавливает Hull и Shield до максимальных значений.
        /// Используется между боями для восстановления повреждённых кораблей.
        public void Repair()
        {
            HullStrength = MaxHull;
            ShieldLevel = MaxShield;
        }
    }
}
