// BattleEngine.cs — Движок пошагового боя флотов
// Этот класс управляет автоматическим боем между двумя флотами:
//   - Сортирует корабли по скорости (инициатива)
//   - Проводит пошаговый бой: каждый корабль атакует случайного врага
//   - Урон сначала снижает щит, затем корпус
//   - Генерирует лог событий боя (кто атаковал, сколько урона, кто уничтожен)
//   - Определяет победителя (последний выживший флот)

using Prototype.Models;

namespace Prototype
{
    /// Событие одного действия в бою (атака, уничтожение, победа).
    public class BattleEvent
    {
        public string Message { get; set; } = "";  // Текст события для лога
        public bool IsVictory { get; set; }        // true = бой завершён, объявлен победитель
        public int RoundNumber { get; set; }       // Номер раунда (0 = начало/конец боя)
        public Starship Target { get; set; }       // Корабль-цель (для подсветки)
        public bool IsMiss { get; set; }           // true = промах, false = попадание
    }

    /// Движок пошагового боя между двумя флотами.
    /// Бой происходит автоматически, возвращается список событий.
    public class BattleEngine
    {
        private Random _rnd = new Random();

        /// Добавляет событие в лог и вызывает async callback для обновления UI.
        private async Task AddEventAsync(List<BattleEvent> log, BattleEvent evt, Func<BattleEvent, Task> onEvent)
        {
            log.Add(evt);
            if (onEvent != null)
                await onEvent(evt);
        }

        /// Запускает бой между двумя флотами (асинхронно с возможностью обновления UI).
        /// Возвращает список событий (лог боя).
        /// Бой модифицирует исходные флоты!
        /// Уничтоженные корабли будут удалены из списков,
        /// повреждённые корабли сохранят сниженные Hull и Shield.
        public async Task<List<BattleEvent>> RunBattleAsync(List<Starship> playerFleet, List<Starship> enemyFleet,
            Func<BattleEvent, Task> onEvent = null,
            Func<List<Starship>, List<Starship>, Task<Dictionary<Starship, Starship>>> onAssignTargets = null)
        {
            var log = new List<BattleEvent>();

            // Проверяем, что оба флота не пусты
            if (playerFleet.Count == 0 || enemyFleet.Count == 0)
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "Невозможно начать бой: один или оба флота пусты!",
                    IsVictory = false,
                    RoundNumber = 0
                }, onEvent);
                return log;
            }

            await AddEventAsync(log, new BattleEvent { Message = "=== НАЧАЛО БОЯ ===", RoundNumber = 0 }, onEvent);
            await AddEventAsync(log, new BattleEvent { Message = $"Флот игрока: {playerFleet.Count} кораблей", RoundNumber = 0 }, onEvent);
            await AddEventAsync(log, new BattleEvent { Message = $"Флот врага: {enemyFleet.Count} кораблей", RoundNumber = 0 }, onEvent);
            await AddEventAsync(log, new BattleEvent { Message = "", RoundNumber = 0 }, onEvent);

            // Работаем напрямую с исходными флотами (БЕЗ клонирования)
            // Это позволяет сохранить урон и удалить уничтоженные корабли
            var playerShips = playerFleet;
            var enemyShips = enemyFleet;

            int round = 1;

            // Главный цикл боя — пока оба флота живы
            while (playerShips.Any() && enemyShips.Any())
            {
                // Назначение целей перед каждым раундом
                Dictionary<Starship, Starship> targetAssignments = null;
                if (onAssignTargets != null)
                    targetAssignments = await onAssignTargets(playerShips, enemyShips);

                await AddEventAsync(log, new BattleEvent { Message = $"--- Раунд {round} ---", RoundNumber = round }, onEvent);

                // Объединяем оба флота и сортируем по скорости (инициатива)
                // Быстрые корабли ходят первыми
                var allShips = playerShips.Select(s => (ship: s, isPlayer: true))
                    .Concat(enemyShips.Select(s => (ship: s, isPlayer: false)))
                    .OrderByDescending(x => x.ship.Speed)  // сортировка по скорости
                    .ThenBy(x => _rnd.Next())               // случайный порядок при равной скорости
                    .ToList();

                // Каждый корабль делает ход
                foreach (var (ship, isPlayer) in allShips)
                {
                    // Проверяем, жив ли атакующий (мог быть уничтожен раньше в этом раунде)
                    bool attackerAlive = (isPlayer && playerShips.Contains(ship)) ||
                                          (!isPlayer && enemyShips.Contains(ship));
                    if (!attackerAlive) continue;

                    // Выбираем флот, который будем атаковать
                    var targetFleet = isPlayer ? enemyShips : playerShips;
                    if (targetFleet.Count == 0) break;  // враги уничтожены

                    // Игрок атакует назначенную цель, враг — случайную
                    Starship target;
                    if (isPlayer && targetAssignments != null
                        && targetAssignments.TryGetValue(ship, out var assigned)
                        && targetFleet.Contains(assigned))
                        target = assigned;
                    else
                        target = targetFleet[_rnd.Next(targetFleet.Count)];

                    // Истребители имеют 60% шанс уклонения
                    if (target.ShipType == "Fighter" && _rnd.Next(100) < 60)
                    {
                        string atkTeam = isPlayer ? "[ИГРОК]" : "[ВРАГ]";
                        string tgtTeam = isPlayer ? "[ВРАГ]" : "[ИГРОК]";
                        await AddEventAsync(log, new BattleEvent
                        {
                            Message = $"{atkTeam} {ship.Name} ({ship.ShipType}) атакует " +
                                      $"{tgtTeam} {target.Name} — ПРОМАХ! (Уклонение истребителя)",
                            RoundNumber = round,
                            Target = target,
                            IsMiss = true
                        }, onEvent);
                        continue;
                    }

                    int damage = ship.Weapon.Damage;

                    // Применяем урон: сначала снижаем щит, затем корпус
                    int actualDamage = 0;
                    if (target.ShieldLevel > 0)
                    {
                        int shieldDamage = Math.Min(damage, target.ShieldLevel);
                        target.ShieldLevel -= shieldDamage;
                        actualDamage = shieldDamage;

                        if (damage > shieldDamage)
                        {
                            int hullDamage = damage - shieldDamage;
                            target.HullStrength -= hullDamage;
                            actualDamage = damage;
                        }
                    }
                    else
                    {
                        target.HullStrength -= damage;
                        actualDamage = damage;
                    }

                    string attackerTeam = isPlayer ? "[ИГРОК]" : "[ВРАГ]";
                    string targetTeam = isPlayer ? "[ВРАГ]" : "[ИГРОК]";

                    await AddEventAsync(log, new BattleEvent
                    {
                        Message = $"{attackerTeam} {ship.Name} ({ship.ShipType}) атакует " +
                                  $"{targetTeam} {target.Name} — {actualDamage} урона!",
                        RoundNumber = round,
                        Target = target
                    }, onEvent);

                    // Проверяем, уничтожена ли цель
                    if (target.HullStrength <= 0)
                    {
                        await AddEventAsync(log, new BattleEvent
                        {
                            Message = $"  >> {targetTeam} {target.Name} УНИЧТОЖЕН!",
                            RoundNumber = round
                        }, onEvent);
                        targetFleet.Remove(target);

                        // Проверяем победу
                        if (targetFleet.Count == 0) break;
                    }
                    else
                    {
                        await AddEventAsync(log, new BattleEvent
                        {
                            Message = $"  >> {target.Name} осталось: Корпус {target.HullStrength}, Щит {target.ShieldLevel}",
                            RoundNumber = round
                        }, onEvent);
                    }
                }

                await AddEventAsync(log, new BattleEvent { Message = "", RoundNumber = round }, onEvent);

                // Проверяем, кто-то победил
                if (playerShips.Count == 0 || enemyShips.Count == 0)
                    break;

                round++;
            }

            // Определяем победителя
            await AddEventAsync(log, new BattleEvent { Message = "=== КОНЕЦ БОЯ ===", RoundNumber = 0 }, onEvent);
            if (playerShips.Any() && !enemyShips.Any())
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "ПОБЕДА! Флот игрока выиграл!",
                    IsVictory = true,
                    RoundNumber = 0
                }, onEvent);
                await AddEventAsync(log, new BattleEvent
                {
                    Message = $"Выживших кораблей: {playerShips.Count}",
                    RoundNumber = 0
                }, onEvent);
            }
            else if (!playerShips.Any() && enemyShips.Any())
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "ПОРАЖЕНИЕ! Флот врага победил!",
                    IsVictory = true,
                    RoundNumber = 0
                }, onEvent);
                await AddEventAsync(log, new BattleEvent
                {
                    Message = $"Выживших врагов: {enemyShips.Count}",
                    RoundNumber = 0
                }, onEvent);
            }
            else
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "НИЧЬЯ! Оба флота уничтожены!",
                    IsVictory = true,
                    RoundNumber = 0
                }, onEvent);
            }

            return log;
        }

        /// Генерирует случайный вражеский флот заданного размера.
        /// Корабли получают случайные характеристики и названия.
        public List<Starship> GenerateEnemyFleet(int size)
        {
            var fleet = new List<Starship>();
            string[] enemyNames = { "Ravager", "Destroyer", "Predator", "Hunter", "Reaper",
                                     "Shadow", "Viper", "Talon", "Fang", "Striker" };
            Color[] enemyColors = { Color.DarkRed, Color.DarkOrange, Color.DarkSlateGray,
                                     Color.DarkGreen, Color.Maroon, Color.Purple };

            for (int i = 0; i < size; i++)
            {
                // Случайный тип корабля
                int shipTypeRoll = _rnd.Next(3);
                string name = enemyNames[_rnd.Next(enemyNames.Length)] + "-" + (i + 1);
                Color color = enemyColors[_rnd.Next(enemyColors.Length)];

                // Случайные характеристики
                int hull = _rnd.Next(50, 180);
                int shield = _rnd.Next(20, 120);
                int speed = _rnd.Next(30, 150);
                var weaponType = (WeaponType)_rnd.Next(5); 
                int damage = _rnd.Next(15, 75);

                var weapon = new WeaponSystem(weaponType, damage);

                Starship ship = shipTypeRoll switch
                {
                    0 => new Fighter(name, hull, hull, shield, shield, speed, color, weapon),
                    1 => new Cruiser(name, hull, hull, shield, shield, speed, color, weapon),
                    _ => new Bomber(name, hull, hull, shield, shield, speed, color, weapon),
                };

                fleet.Add(ship);
            }

            return fleet;
        }
    }
}
