# Starship Fleet Builder — Prototype Pattern Demo

A Windows Forms application demonstrating the **Prototype design pattern** through an interactive starship fleet builder. Create ships from prototypes, customize their properties and weapons, clone them into a fleet, and see real-time visual previews rendered with GDI+.

## How to Build and Run

```bash
dotnet build WinFormsApp1/WinFormsApp1.csproj
dotnet run --project WinFormsApp1/WinFormsApp1.csproj
```

Requires .NET 8.0 SDK. No external dependencies or assets needed.

## Features

### Prototype Selection
Three base ship prototypes with distinct shapes and default stats:
- **Fighter** — fast, light hull, arrow/triangle shape with swept wings
- **Cruiser** — balanced stats, elongated hexagon shape with a bridge section
- **Bomber** — heavy hull, slow, wide rectangular body with top/bottom fins

### Property Editing
All properties update the ship preview in real-time:
- **Name** — free text
- **Hull** (10–200), **Shield** (0–150), **Speed** (10–200) — numeric spinners
- **Color** — 10 preset colors applied to the entire ship body
- **Weapon Type** — dropdown with 5 types (see below)
- **Damage** (5–100) — numeric spinner

### Weapon System
Each weapon type draws distinct visual attachments on the ship, positioned correctly using per-ship-type mount points:

| Weapon | Visual |
|--------|--------|
| Laser Cannon | Twin red laser lines from the nose with glow tips |
| Plasma Turret | Purple plasma orbs on top and bottom of the hull |
| Missile Rack | Four orange missile triangles at wing positions |
| Torpedo Bay | Cyan torpedo ovals above and below the body |
| Ion Beam | Spreading cyan beam cone from the nose with emitter ring |

### Clone to Fleet
Clicking **"Clone to Fleet"** calls `Clone()` on the current ship (Prototype pattern), creating an independent copy added to the fleet list.

### Deep Copy Demo
Demonstrates that `Clone()` performs a **deep copy**: clones a ship, modifies the original's weapon damage, and shows via MessageBox that the clone's weapon is unaffected. This proves the `WeaponSystem` reference type is independently copied, not shared.

### Ship Info Panel
Displays the current ship's stats with colored progress bars:
- Hull (green), Shield (blue), Speed (yellow), Damage (orange-red)

### Fleet Battle ⚔
**NEW!** Click **"Battle!"** to fight your fleet against a randomly generated enemy fleet:
- **Auto-combat**: Turn-based combat runs automatically
- **Speed = Initiative**: Faster ships attack first each round
- **Damage mechanics**: Attacks reduce shields first, then hull
- **Battle log**: Real-time event log shows each attack, damage dealt, and ships destroyed
- **Victory condition**: Last fleet standing wins
- **Real consequences**: ⚠️ Damaged ships keep their reduced hull/shield values! Destroyed ships are permanently removed from your fleet!

### Repair System 🔧
**NEW!** Click **"Repair Fleet"** to restore damaged ships:
- **Full restoration**: All ships repaired to their maximum Hull and Shield values
- **Max values tracked**: Each ship remembers its original max HP when cloned
- **Strategic resource**: Use between battles to prepare for the next fight
- **Damage report**: Shows how many ships needed repairs

This makes every stat meaningful — hull is HP, shields are armor, speed determines turn order, and damage is attack power. Build a balanced fleet to maximize your chances! Battles have permanent consequences, but you can repair between fights.

## Architecture

### Prototype Pattern Structure

```
IStarship (interface)          — Clone(), GetInfo(), properties
  └── Starship (abstract)      — shared property storage, GetInfo()
        ├── Fighter             — Clone() returns new Fighter with deep-copied weapon
        ├── Cruiser             — Clone() returns new Cruiser with deep-copied weapon
        └── Bomber              — Clone() returns new Bomber with deep-copied weapon

WeaponSystem (reference type)  — Type (enum), Damage, Clone()
WeaponType (enum)              — LaserCannon, PlasmaTurret, MissileRack, TorpedoBay, IonBeam
```

Each concrete ship's `Clone()` creates a new instance and calls `Weapon.Clone()` to ensure deep copy of the reference type.

### Rendering

**ShipRenderer** — static class that draws ships using GDI+ polygons. Each ship type has a dedicated drawing method producing a distinct geometric shape.

**ShipMounts** — a struct defining weapon attachment points per ship type (nose, body edges, wing tips). Weapon drawing methods use these mount points so attachments always sit correctly on each ship's hull regardless of shape.

**DoubleBufferedPanel** — custom `Panel` subclass with `DoubleBuffered`, `OptimizedDoubleBuffer`, and `ResizeRedraw` styles enabled. Eliminates flicker during redraws and ensures clean rendering on window resize.

### UI Layout

```
┌──────────────┬─────────────────────────────────────┐
│  Left Panel  │  Ship Preview    │  Ship Info Panel  │
│  (fixed 290) │  (GDI+ drawing)  │  (stats + bars)   │
│              │                  │                   │
│  Prototype   ├──────────────────┴───────────────────┤
│  Properties  │                                      │
│  Weapon      │  Fleet (Cloned Ships) — ListBox      │
│  Buttons     │                                      │
└──────────────┴──────────────────────────────────────┘
```

- Left panel: fixed width, absolute positioning
- Right side: `Dock.Fill` with `TableLayoutPanel` for the top row, responsive to window resize
- Dark theme (background RGB 20,20,35)

### File Structure

```
WinFormsApp1/
├── Models/
│   ├── IStarship.cs         # Prototype interface
│   ├── Starship.cs          # Abstract base class
│   ├── Fighter.cs           # Concrete prototype
│   ├── Cruiser.cs           # Concrete prototype
│   ├── Bomber.cs            # Concrete prototype
│   └── WeaponSystem.cs      # Weapon type enum + weapon class with Clone()
├── BattleEngine.cs          # Fleet battle logic and enemy AI
├── ShipRenderer.cs          # GDI+ drawing + ShipMounts struct
├── DoubleBufferedPanel.cs   # Flicker-free panel
├── Form1.cs                 # Event handlers, fleet logic, battle
├── Form1.Designer.cs        # UI layout and control setup
├── Program.cs               # Entry point
└── WinFormsApp1.csproj      # .NET 8.0 Windows Forms project
```

---

# Строитель Звёздного Флота — Демонстрация паттерна «Прототип»

Приложение на Windows Forms, демонстрирующее **паттерн проектирования «Прототип»** через интерактивный конструктор звёздного флота. Создавайте корабли из прототипов, настраивайте их свойства и оружие, клонируйте их во флот и наблюдайте визуальные превью в реальном времени, отрисованные с помощью GDI+.

## Как собрать и запустить

```bash
dotnet build WinFormsApp1/WinFormsApp1.csproj
dotnet run --project WinFormsApp1/WinFormsApp1.csproj
```

Требуется .NET 8.0 SDK. Внешние зависимости и ресурсы не нужны.

## Возможности

### Выбор прототипа
Три базовых прототипа кораблей с уникальными формами и характеристиками по умолчанию:
- **Истребитель (Fighter)** — быстрый, лёгкий корпус, форма стрелы/треугольника со стреловидными крыльями
- **Крейсер (Cruiser)** — сбалансированные характеристики, форма удлинённого шестиугольника с секцией мостика
- **Бомбардировщик (Bomber)** — тяжёлый корпус, медленный, широкий прямоугольный корпус с верхним/нижним плавниками

### Редактирование свойств
Все свойства обновляют превью корабля в реальном времени:
- **Имя** — свободный ввод текста
- **Корпус** (10–200), **Щит** (0–150), **Скорость** (10–200) — числовые счётчики
- **Цвет** — 10 предустановленных цветов, применяемых ко всему корпусу
- **Тип оружия** — выпадающий список с 5 типами (см. ниже)
- **Урон** (5–100) — числовой счётчик

### Система вооружения
Каждый тип оружия рисует уникальные визуальные элементы на корабле, позиционируемые с помощью точек крепления для каждого типа корабля:

| Оружие | Визуал |
|--------|--------|
| Лазерная пушка (Laser Cannon) | Двойные красные лазерные лучи из носа со светящимися кончиками |
| Плазменная турель (Plasma Turret) | Фиолетовые плазменные сферы сверху и снизу корпуса |
| Ракетная установка (Missile Rack) | Четыре оранжевых ракеты-треугольника на позициях крыльев |
| Торпедный отсек (Torpedo Bay) | Бирюзовые торпедные овалы над и под корпусом |
| Ионный луч (Ion Beam) | Расширяющийся бирюзовый конус из носа с кольцом эмиттера |

### Клонирование во флот
Нажатие кнопки **«Clone to Fleet»** вызывает `Clone()` на текущем корабле (паттерн Прототип), создавая независимую копию, добавляемую в список флота.

### Демонстрация глубокого копирования
Демонстрирует, что `Clone()` выполняет **глубокое копирование**: клонирует корабль, изменяет урон оружия оригинала и показывает через MessageBox, что оружие клона не затронуто. Это доказывает, что ссылочный тип `WeaponSystem` копируется независимо, а не используется совместно.

### Панель информации о корабле
Отображает характеристики текущего корабля с цветными прогресс-барами:
- Корпус (зелёный), Щит (голубой), Скорость (жёлтый), Урон (оранжево-красный)

### Бой флотов ⚔
**НОВОЕ!** Нажмите **«Battle!»**, чтобы вступить в бой со случайным вражеским флотом:
- **Автоматический бой**: Пошаговый бой происходит автоматически
- **Скорость = инициатива**: Быстрые корабли атакуют первыми в каждом раунде
- **Механика урона**: Атаки сначала снижают щит, затем корпус
- **Лог боя**: Отображает каждую атаку, нанесённый урон и уничтоженные корабли
- **Условие победы**: Побеждает последний выживший флот
- **Реальные последствия**: ⚠️ Повреждённые корабли сохраняют сниженные значения корпуса/щита! Уничтоженные корабли навсегда удаляются из флота!

### Система ремонта 🔧
**НОВОЕ!** Нажмите **«Repair Fleet»**, чтобы восстановить повреждённые корабли:
- **Полное восстановление**: Все корабли ремонтируются до максимальных значений Корпуса и Щита
- **Отслеживание максимумов**: Каждый корабль помнит свои исходные макс. значения при клонировании
- **Стратегический ресурс**: Используйте между боями для подготовки к следующему сражению
- **Отчёт о повреждениях**: Показывает, сколько кораблей нуждалось в ремонте

Это делает все характеристики значимыми — корпус это HP, щит это броня, скорость определяет порядок ходов, урон это атака. Создайте сбалансированный флот для победы! Бой имеет постоянные последствия, но вы можете ремонтировать корабли между боями.

## Архитектура

### Структура паттерна «Прототип»

```
IStarship (интерфейс)           — Clone(), GetInfo(), свойства
  └── Starship (абстрактный)    — общее хранилище свойств, GetInfo()
        ├── Fighter              — Clone() создаёт новый Fighter с глубокой копией оружия
        ├── Cruiser              — Clone() создаёт новый Cruiser с глубокой копией оружия
        └── Bomber               — Clone() создаёт новый Bomber с глубокой копией оружия

WeaponSystem (ссылочный тип)    — Type (перечисление), Damage, Clone()
WeaponType (перечисление)       — LaserCannon, PlasmaTurret, MissileRack, TorpedoBay, IonBeam
```

Метод `Clone()` каждого конкретного корабля создаёт новый экземпляр и вызывает `Weapon.Clone()` для обеспечения глубокого копирования ссылочного типа.

### Отрисовка

**ShipRenderer** — статический класс, рисующий корабли с помощью полигонов GDI+. Каждый тип корабля имеет свой метод отрисовки, создающий уникальную геометрическую форму.

**ShipMounts** — структура, определяющая точки крепления оружия для каждого типа корабля (нос, края корпуса, кончики крыльев). Методы рисования оружия используют эти точки, чтобы элементы всегда правильно располагались на корпусе корабля, независимо от его формы.

**DoubleBufferedPanel** — наследник `Panel` с включёнными стилями `DoubleBuffered`, `OptimizedDoubleBuffer` и `ResizeRedraw`. Устраняет мерцание при перерисовке и обеспечивает чистую отрисовку при изменении размера окна.

### Разметка интерфейса

```
┌──────────────┬─────────────────────────────────────┐
│ Левая панель │ Превью корабля  │ Панель информации  │
│ (фикс. 290) │ (GDI+ рисунок) │ (стат. + бары)     │
│              │                 │                    │
│  Прототип    ├─────────────────┴────────────────────┤
│  Свойства    │                                      │
│  Оружие      │  Флот (клонированные корабли) — ListBox│
│  Кнопки      │                                      │
└──────────────┴──────────────────────────────────────┘
```

- Левая панель: фиксированная ширина, абсолютное позиционирование
- Правая часть: `Dock.Fill` с `TableLayoutPanel` для верхнего ряда, адаптивная к изменению размера окна
- Тёмная тема (фон RGB 20,20,35)

### Структура файлов

```
WinFormsApp1/
├── Models/
│   ├── IStarship.cs         # Интерфейс прототипа
│   ├── Starship.cs          # Абстрактный базовый класс
│   ├── Fighter.cs           # Конкретный прототип (Истребитель)
│   ├── Cruiser.cs           # Конкретный прототип (Крейсер)
│   ├── Bomber.cs            # Конкретный прототип (Бомбардировщик)
│   └── WeaponSystem.cs      # Перечисление типов оружия + класс с Clone()
├── BattleEngine.cs          # Логика боя флотов и генерация врагов
├── ShipRenderer.cs          # Отрисовка GDI+ + структура ShipMounts
├── DoubleBufferedPanel.cs   # Панель без мерцания
├── Form1.cs                 # Обработчики событий, логика флота, бой
├── Form1.Designer.cs        # Разметка интерфейса и настройка контролов
├── Program.cs               # Точка входа
└── WinFormsApp1.csproj      # Проект .NET 8.0 Windows Forms
```
