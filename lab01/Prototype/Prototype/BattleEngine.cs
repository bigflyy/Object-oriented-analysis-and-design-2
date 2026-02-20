// =============================================================================
// BattleEngine.cs — Движок пошагового боя флотов
// =============================================================================
// Этот класс управляет автоматическим боем между двумя флотами:
//   - Сортирует корабли по скорости (инициатива)
//   - Проводит пошаговый бой: каждый корабль атакует случайного врага
//   - Урон сначала снижает щит, затем корпус
//   - Генерирует лог событий боя (кто атаковал, сколько урона, кто уничтожен)
//   - Определяет победителя (последний выживший флот)
// =============================================================================

using Prototype.Models;

namespace Prototype
{
    /// Событие одного действия в бою (атака, уничтожение, победа).
    public class BattleEvent
    {
        public string Message { get; set; } = "";  // Текст события для лога
        public bool IsVictory { get; set; }        // true = бой завершён, объявлен победитель
        public int RoundNumber { get; set; }       // Номер раунда (0 = начало/конец боя)
        public Starship? HitTarget { get; set; }  // Корабль, который был атакован (для подсветки)
    }

    /// Движок пошагового боя между двумя флотами.
    /// Бой происходит автоматически, возвращается список событий.
    public class BattleEngine
    {
        private Random _rnd = new Random();

        /// Добавляет событие в лог и вызывает async callback для обновления UI.
        private async Task AddEventAsync(List<BattleEvent> log, BattleEvent evt, Func<BattleEvent, Task>? onEvent)
        {
            log.Add(evt);
            if (onEvent != null)
                await onEvent(evt);
        }

        /// Запускает бой между двумя флотами (синхронная версия без UI обновлений).
        public List<BattleEvent> RunBattle(List<Starship> playerFleet, List<Starship> enemyFleet)
        {
            return RunBattleAsync(playerFleet, enemyFleet, null).GetAwaiter().GetResult();
        }

        /// Запускает бой между двумя флотами (асинхронно с возможностью обновления UI).
        /// Возвращает список событий (лог боя).
        /// ВНИМАНИЕ: Бой модифицирует исходные флоты!
        /// Уничтоженные корабли будут удалены из списков,
        /// повреждённые корабли сохранят сниженные Hull и Shield.
        public async Task<List<BattleEvent>> RunBattleAsync(List<Starship> playerFleet, List<Starship> enemyFleet,
            Func<BattleEvent, Task>? onEvent = null)
        {
            var log = new List<BattleEvent>();

            // Проверяем, что оба флота не пусты
            if (playerFleet.Count == 0 || enemyFleet.Count == 0)
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "Cannot start battle: one or both fleets are empty!",
                    IsVictory = false,
                    RoundNumber = 0
                }, onEvent);
                return log;
            }

            await AddEventAsync(log, new BattleEvent { Message = "=== BATTLE START ===", RoundNumber = 0 }, onEvent);
            await AddEventAsync(log, new BattleEvent { Message = $"Player Fleet: {playerFleet.Count} ships", RoundNumber = 0 }, onEvent);
            await AddEventAsync(log, new BattleEvent { Message = $"Enemy Fleet: {enemyFleet.Count} ships", RoundNumber = 0 }, onEvent);
            await AddEventAsync(log, new BattleEvent { Message = "", RoundNumber = 0 }, onEvent);

            // Работаем напрямую с исходными флотами (БЕЗ клонирования)
            // Это позволяет сохранить урон и удалить уничтоженные корабли
            var playerShips = playerFleet;
            var enemyShips = enemyFleet;

            int round = 1;

            // Главный цикл боя — пока оба флота живы
            while (playerShips.Any() && enemyShips.Any())
            {
                await AddEventAsync(log, new BattleEvent { Message = $"--- Round {round} ---", RoundNumber = round }, onEvent);

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

                    // Выбираем случайную цель из вражеского флота
                    var targetFleet = isPlayer ? enemyShips : playerShips;
                    if (targetFleet.Count == 0) break;  // враги уничтожены

                    var target = targetFleet[_rnd.Next(targetFleet.Count)];
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

                    string attackerTeam = isPlayer ? "[PLAYER]" : "[ENEMY]";
                    string targetTeam = isPlayer ? "[ENEMY]" : "[PLAYER]";

                    await AddEventAsync(log, new BattleEvent
                    {
                        Message = $"{attackerTeam} {ship.Name} ({ship.ShipType}) attacks " +
                                  $"{targetTeam} {target.Name} for {actualDamage} damage!",
                        RoundNumber = round,
                        HitTarget = target  // Указываем, какой корабль был атакован
                    }, onEvent);

                    // Проверяем, уничтожена ли цель
                    if (target.HullStrength <= 0)
                    {
                        await AddEventAsync(log, new BattleEvent
                        {
                            Message = $"  >> {targetTeam} {target.Name} DESTROYED!",
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
                            Message = $"  >> {target.Name} remaining: Hull {target.HullStrength}, Shield {target.ShieldLevel}",
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
            await AddEventAsync(log, new BattleEvent { Message = "=== BATTLE END ===", RoundNumber = 0 }, onEvent);
            if (playerShips.Any() && !enemyShips.Any())
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "VICTORY! Player fleet wins!",
                    IsVictory = true,
                    RoundNumber = 0
                }, onEvent);
                await AddEventAsync(log, new BattleEvent
                {
                    Message = $"Surviving ships: {playerShips.Count}",
                    RoundNumber = 0
                }, onEvent);
            }
            else if (!playerShips.Any() && enemyShips.Any())
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "DEFEAT! Enemy fleet wins!",
                    IsVictory = true,
                    RoundNumber = 0
                }, onEvent);
                await AddEventAsync(log, new BattleEvent
                {
                    Message = $"Enemy survivors: {enemyShips.Count}",
                    RoundNumber = 0
                }, onEvent);
            }
            else
            {
                await AddEventAsync(log, new BattleEvent
                {
                    Message = "DRAW! Both fleets destroyed!",
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
                var weaponType = (WeaponType)_rnd.Next(5);  // случайный тип оружия
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
