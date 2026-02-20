// =============================================================================
// Form1.cs — Основная логика формы (обработчики событий и управление флотом)
// =============================================================================
// Этот файл содержит бизнес-логику приложения:
//   - Загрузка прототипов кораблей (Fighter, Cruiser, Bomber)
//   - Обработка изменений свойств (имя, корпус, щит, скорость, цвет, оружие)
//   - Клонирование корабля в флот (паттерн Прототип — метод Clone)
//   - Демонстрация глубокого копирования (Deep Copy Demo)
//   - Отрисовка предпросмотра корабля и панели информации
//
// Визуальная часть формы (расположение контролов) — в Form1.Designer.cs.
// =============================================================================

using Prototype.Models;

namespace Prototype
{
    public partial class Form1 : Form
    {
        // Верфь — клиент паттерна Прототип. Хранит прототипы и клонирует их.
        private readonly Shipyard _shipyard = new();

        // Текущий корабль, отображаемый в редакторе (прототип-образец из верфи).
        // null! — потому что инициализируется в LoadPrototype(), вызываемом из конструктора.
        private Starship _currentShip = null!;

        // Список клонированных кораблей (флот).
        // Каждый элемент — независимая глубокая копия, созданная через Clone().
        private readonly List<Starship> _fleet = new();

        // Флаг для предотвращения рекурсивных обновлений:
        // когда мы программно устанавливаем значения контролов (в LoadPrototype),
        // не нужно срабатывать обработчикам OnPropertyChanged.
        private bool _updatingUI;

        // Массив типов оружия — соответствует порядку элементов в ComboBox cmbWeapon.
        // Используется для преобразования индекса ComboBox в значение WeaponType.
        private static readonly WeaponType[] WeaponTypes = {
            WeaponType.LaserCannon,   // индекс 0 — "Laser Cannon"
            WeaponType.PlasmaTurret,  // индекс 1 — "Plasma Turret"
            WeaponType.MissileRack,   // индекс 2 — "Missile Rack"
            WeaponType.TorpedoBay,    // индекс 3 — "Torpedo Bay"
            WeaponType.IonBeam        // индекс 4 — "Ion Beam"
        };

        /// Конструктор формы.
        /// InitializeComponent() создаёт все визуальные элементы (из Designer.cs).
        /// LoadPrototype() загружает корабль по умолчанию (Fighter).
        public Form1()
        {
            InitializeComponent();
            LoadPrototype();
        }

        // =====================================================================
        // ЗАГРУЗКА ПРОТОТИПА — создаёт корабль с настройками по умолчанию
        // =====================================================================

        /// Создаёт новый корабль-прототип в зависимости от выбранного RadioButton.
        /// Затем синхронизирует все контролы интерфейса с его свойствами.
        ///
        /// Флаг _updatingUI предотвращает срабатывание OnPropertyChanged
        /// при программной установке значений (иначе была бы бесконечная рекурсия).
        private void LoadPrototype()
        {
            _updatingUI = true;  // Блокируем обработчики изменений

            // Получаем прототип-образец из верфи в зависимости от выбранного типа
            string type = rbFighter.Checked ? "Fighter"
                        : rbCruiser.Checked ? "Cruiser"
                        : "Bomber";
            _currentShip = _shipyard.GetPrototype(type);

            // Синхронизируем контролы интерфейса с данными корабля.
            // Clamp — ограничивает значение пределами NumericUpDown,
            // чтобы не было исключения при выходе за Min/Max.
            txtName.Text = _currentShip.Name;
            nudHull.Value = Clamp(_currentShip.HullStrength, (int)nudHull.Minimum, (int)nudHull.Maximum);
            nudShield.Value = Clamp(_currentShip.ShieldLevel, (int)nudShield.Minimum, (int)nudShield.Maximum);
            nudSpeed.Value = Clamp(_currentShip.Speed, (int)nudSpeed.Minimum, (int)nudSpeed.Maximum);
            nudDamage.Value = Clamp(_currentShip.Weapon.Damage, (int)nudDamage.Minimum, (int)nudDamage.Maximum);

            // Устанавливаем тип оружия в ComboBox по индексу из массива WeaponTypes
            cmbWeapon.SelectedIndex = Array.IndexOf(WeaponTypes, _currentShip.Weapon.Type);

            // Устанавливаем цвет в ComboBox по названию цвета
            string colorName = _currentShip.ShipColor.Name;
            int idx = cmbColor.Items.IndexOf(colorName);
            cmbColor.SelectedIndex = idx >= 0 ? idx : 0;

            _updatingUI = false;  // Разблокируем обработчики

            // Перерисовываем панели предпросмотра и информации
            panelPreview.Invalidate();
            panelInfo.Invalidate();
        }

        // =====================================================================
        // ОБРАБОТЧИКИ СОБЫТИЙ
        // =====================================================================

        /// Обработчик переключения типа прототипа (RadioButton).
        /// Вызывается при нажатии на Fighter / Cruiser / Bomber.
        /// Перезагружает корабль с настройками по умолчанию для нового типа.
        private void OnPrototypeChanged(object? sender, EventArgs e)
        {
            // Проверяем, что это RadioButton и он выбран
            // (событие CheckedChanged срабатывает и при снятии, и при установке)
            if (sender is RadioButton rb && rb.Checked)
                LoadPrototype();
        }

        /// Обработчик изменения любого свойства корабля
        /// (имя, корпус, щит, скорость, урон, тип оружия, цвет).
        /// Считывает значения из контролов и обновляет объект _currentShip.
        /// Затем перерисовывает панели.
        ///
        /// Флаг _updatingUI не даёт этому методу сработать, когда мы
        /// программно меняем значения контролов в LoadPrototype().
        private void OnPropertyChanged(object? sender, EventArgs e)
        {
            if (_updatingUI || _currentShip == null) return;

            // Считываем значения из контролов интерфейса
            string name = txtName.Text;
            int hull = (int)nudHull.Value;
            int shield = (int)nudShield.Value;
            int speed = (int)nudSpeed.Value;
            int damage = (int)nudDamage.Value;
            var weaponType = cmbWeapon.SelectedIndex >= 0
                ? WeaponTypes[cmbWeapon.SelectedIndex]
                : _currentShip.Weapon.Type;
            Color color = cmbColor.SelectedItem is string colorStr
                ? Color.FromName(colorStr)
                : _currentShip.ShipColor;

            // Делегируем настройку прототипа верфи
            _shipyard.CustomizePrototype(_currentShip.ShipType,
                name, hull, shield, speed, color, weaponType, damage);

            // Перерисовываем обе панели
            panelPreview.Invalidate();  // предпросмотр корабля
            panelInfo.Invalidate();     // панель характеристик
        }

        // =====================================================================
        // КНОПКИ
        // =====================================================================

        /// Кнопка «Clone to Fleet» — КЛОНИРОВАНИЕ (паттерн Прототип).
        /// Верфь клонирует текущий прототип, создавая глубокую копию.
        /// Добавляет клон в список флота. Клон полностью независим от прототипа.
        private void BtnClone_Click(object? sender, EventArgs e)
        {
            string type = _currentShip.ShipType;
            Starship clone = _shipyard.BuildShip(type);  // Верфь клонирует прототип!
            _fleet.Add(clone);
            RefreshFleetDisplay();  // Обновляем отображение
        }

        /// Кнопка «Deep Copy Demo» — демонстрация глубокого копирования.
        ///
        /// Алгоритм:
        /// 1. Клонируем текущий корабль → "Original"
        /// 2. Клонируем "Original" → "Clone"
        /// 3. Меняем урон у оригинала на 999
        /// 4. Проверяем: урон клона НЕ изменился → глубокая копия работает!
        ///
        /// Если бы Clone() был поверхностным, оригинал и клон делили бы
        /// один объект WeaponSystem, и урон изменился бы у обоих.
        private void BtnDeepCopyDemo_Click(object? sender, EventArgs e)
        {
            // Шаг 1: Создаём оригинал (клонируем текущий, чтобы не менять его)
            Starship original = _currentShip.Clone();
            original.Name = "Original";
            original.Weapon.Damage = 50;

            // Шаг 2: Клонируем оригинал
            Starship clone = original.Clone();
            clone.Name = "Clone";

            // Шаг 3: Запоминаем урон до изменения, затем меняем у оригинала
            int originalDamageBefore = original.Weapon.Damage;
            original.Weapon.Damage = 999;  // Меняем ТОЛЬКО у оригинала

            // Шаг 4: Формируем сообщение с результатами
            string message =
                $"Deep Copy Demonstration:\n\n" +
                $"1. Created original ship with Weapon Damage = {originalDamageBefore}\n" +
                $"2. Cloned it (deep copy via Clone())\n" +
                $"3. Changed original's Weapon Damage to {original.Weapon.Damage}\n\n" +
                $"Result:\n" +
                $"  Original weapon damage: {original.Weapon.Damage}\n" +
                $"  Clone weapon damage:    {clone.Weapon.Damage}\n\n" +
                // Проверяем: если урон клона != урону оригинала — глубокая копия работает
                (clone.Weapon.Damage != original.Weapon.Damage
                    ? "Deep copy works! The clone's WeaponSystem is independent."
                    : "Shallow copy! The clone shares the same WeaponSystem reference.");

            MessageBox.Show(message, "Prototype Pattern — Deep Copy Demo",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// Кнопка «Clear Fleet» — очищает список флота и вражеского флота.
        private void BtnClearFleet_Click(object? sender, EventArgs e)
        {
            _fleet.Clear();
            RefreshFleetDisplay();

            // НОВОЕ: Также очищаем вражескую панель
            enemyFleetPanel.Clear();

            // Очищаем лог боя
            txtBattleLog.Text = "Click 'Battle!' to start a fleet battle.\r\n\r\nYour fleet will fight against a randomly generated enemy fleet.\r\n\r\nSpeed determines turn order.\r\nDamage reduces shields first, then hull.";
        }

        /// Кнопка «Battle!» — запускает бой флота против случайных врагов.
        ///
        /// Алгоритм:
        /// 1. Проверяем, есть ли корабли во флоте игрока
        /// 2. Генерируем случайный вражеский флот того же размера
        /// 3. Переключаемся на вкладку Fleet Command для визуализации боя
        /// 4. Отображаем вражеский флот визуально (карточки с HP/Shield)
        /// 5. Запускаем автоматический пошаговый бой (BattleEngine)
        /// 6. НОВОЕ: Отображаем бой раунд за раундом с задержкой между раундами
        /// 7. Обновляем визуальные панели флотов после каждого раунда
        /// 8. ОБНОВЛЯЕМ флот — урон сохраняется, уничтоженные корабли удаляются
        ///
        /// Механика боя:
        /// - Скорость определяет порядок ходов (быстрые корабли ходят первыми)
        /// - Каждый корабль атакует случайного врага
        /// - Урон сначала снижает щит, затем корпус
        /// - Уничтоженные корабли удаляются ИЗ ФЛОТА (реальные последствия!)
        /// - Бой продолжается до полного уничтожения одного из флотов
        private async void BtnBattle_Click(object? sender, EventArgs e)
        {
            // Проверяем, есть ли корабли во флоте
            if (_fleet.Count == 0)
            {
                MessageBox.Show("Your fleet is empty! Clone some ships first.",
                    "No Fleet", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Сохраняем исходный размер флота для статистики
            int initialFleetSize = _fleet.Count;

            // Создаём движок боя и генерируем врагов
            var battleEngine = new BattleEngine();
            var enemyFleet = battleEngine.GenerateEnemyFleet(_fleet.Count);

            // Переключаемся на вкладку Fleet Command
            tabControl.SelectedIndex = 1;

            // Отображаем вражеский флот визуально (до боя, в полной силе)
            enemyFleetPanel.SetFleet(enemyFleet);
            playerFleetPanel.SetFleet(_fleet);

            // Очищаем лог и показываем начальную информацию
            txtBattleLog.Text = "=== ENEMY FLEET DETECTED ===\r\n" +
                                $"Enemy ships: {enemyFleet.Count}\r\n" +
                                "Preparing for battle...\r\n\r\n";

            // Отключаем кнопку Battle на время боя
            btnBattle.Enabled = false;

            // Даём пользователю время посмотреть на вражеский флот (2 секунды)
            await Task.Delay(2000);

            // Переменная для отслеживания текущего раунда
            int lastRound = 0;
            int lastPlayerCount = _fleet.Count;
            int lastEnemyCount = enemyFleet.Count;

            // Запускаем бой с callback для обновления UI в реальном времени
            var battleLog = await battleEngine.RunBattleAsync(_fleet, enemyFleet, async (evt) =>
            {
                // Пропускаем начальные сообщения (уже показаны выше)
                if (evt.Message.Contains("BATTLE START") || evt.Message.Contains("Fleet:"))
                    return;

                // Добавляем сообщение в лог
                txtBattleLog.AppendText(evt.Message + "\r\n");
                txtBattleLog.SelectionStart = txtBattleLog.Text.Length;
                txtBattleLog.ScrollToCaret();

                // ОПТИМИЗАЦИЯ: Пересоздаём карточки только если количество кораблей изменилось (корабль уничтожен)
                // Иначе просто обновляем существующие карточки
                bool shipsDestroyed = _fleet.Count != lastPlayerCount || enemyFleet.Count != lastEnemyCount;

                if (shipsDestroyed)
                {
                    // Корабль уничтожен - пересоздаём все карточки
                    playerFleetPanel.SetFleet(_fleet);
                    enemyFleetPanel.SetFleet(enemyFleet);
                    lastPlayerCount = _fleet.Count;
                    lastEnemyCount = enemyFleet.Count;
                }
                else
                {
                    // Только урон - обновляем существующие карточки (без пересоздания)
                    playerFleetPanel.RefreshCards();
                    enemyFleetPanel.RefreshCards();
                }

                // Обновляем форму для перерисовки
                Application.DoEvents();

                // НОВОЕ: Если это атака, подсвечиваем атакованный корабль красным
                // (делаем это ПОСЛЕ обновления, чтобы карточки были готовы)
                if (evt.HitTarget != null)
                {
                    // Определяем, какая панель содержит цель (игрок или враг)
                    if (_fleet.Contains(evt.HitTarget))
                    {
                        // Атакован корабль игрока - НЕ ждём завершения flash (пусть идёт параллельно)
                        _ = playerFleetPanel.FlashShip(evt.HitTarget);
                    }
                    else if (enemyFleet.Contains(evt.HitTarget))
                    {
                        // Атакован вражеский корабль
                        _ = enemyFleetPanel.FlashShip(evt.HitTarget);
                    }
                }

                // Если начался новый раунд, делаем паузу
                if (evt.RoundNumber > 0 && evt.RoundNumber != lastRound)
                {
                    lastRound = evt.RoundNumber;
                    if (lastRound > 1)  // Пауза после завершения предыдущего раунда
                    {
                        await Task.Delay(1500);  // 1.5 секунды между раундами
                    }
                }
                else if (!string.IsNullOrEmpty(evt.Message) && !evt.Message.StartsWith("---"))
                {
                    // Небольшая задержка между атаками (800 мс для видимости подсветки)
                    await Task.Delay(800);
                }
            });

            // Добавляем статистику потерь
            int shipsLost = initialFleetSize - _fleet.Count;
            if (shipsLost > 0)
            {
                txtBattleLog.AppendText($"\r\nYour fleet lost {shipsLost} ship(s) in battle.\r\n");
            }
            if (_fleet.Count > 0)
            {
                txtBattleLog.AppendText($"Surviving ships may be damaged.\r\n");
            }

            // Финальное обновление флотов
            playerFleetPanel.SetFleet(_fleet);
            enemyFleetPanel.SetFleet(enemyFleet);

            // Автоматически прокручиваем вниз
            txtBattleLog.SelectionStart = txtBattleLog.Text.Length;
            txtBattleLog.ScrollToCaret();

            // Включаем кнопку обратно
            btnBattle.Enabled = true;
        }

        /// Кнопка «Repair Fleet» — ремонтирует все корабли во флоте.
        /// Восстанавливает Hull и Shield всех кораблей до их максимальных значений.
        /// Полезно после боя для подготовки к следующему сражению.
        private void BtnRepair_Click(object? sender, EventArgs e)
        {
            if (_fleet.Count == 0)
            {
                MessageBox.Show("Your fleet is empty! Nothing to repair.",
                    "No Fleet", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Считаем количество повреждённых кораблей
            int repairedCount = 0;
            foreach (var ship in _fleet)
            {
                bool wasDamaged = ship.HullStrength < ship.MaxHull || ship.ShieldLevel < ship.MaxShield;
                ship.Repair();
                if (wasDamaged) repairedCount++;
            }

            // Обновляем отображение (визуальные панели покажут восстановленные HP-бары)
            RefreshFleetDisplay();

            // Показываем результат
            if (repairedCount > 0)
            {
                MessageBox.Show($"Repaired {repairedCount} damaged ship(s).\nAll ships restored to full health!",
                    "Repair Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("All ships are already at full health!",
                    "Repair Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// Обновляет отображение флота в ListBox и визуальной панели флота.
        /// Показывает актуальные значения Hull и Shield после боя.
        private void RefreshFleetDisplay()
        {
            // Обновляем старый ListBox (для обратной совместимости)
            lstFleet.Items.Clear();
            foreach (var ship in _fleet)
            {
                lstFleet.Items.Add(ship.GetInfo());
            }

            // НОВОЕ: Обновляем визуальную панель флота с карточками кораблей
            playerFleetPanel.SetFleet(_fleet);
        }

        // =====================================================================
        // ОТРИСОВКА ПАНЕЛЕЙ (события Paint)
        // =====================================================================

        /// Отрисовка панели предпросмотра корабля.
        /// Вызывается системой при Invalidate() или при перерисовке окна.
        ///
        /// Корабль рисуется с ограничением размера (макс 350×250),
        /// чтобы он не растягивался до неприличных размеров на больших окнах.
        /// Позиция центрируется в доступной области.
        private void PanelPreview_Paint(object? sender, PaintEventArgs e)
        {
            if (_currentShip == null) return;

            var client = panelPreview.ClientRectangle;

            // Ограничиваем максимальный размер отрисовки
            int maxW = Math.Min(client.Width, 350);
            int maxH = Math.Min(client.Height - 30, 250);  // -30 для имени снизу

            // Центрируем область рисования
            int drawX = client.X + (client.Width - maxW) / 2;
            int drawY = client.Y + (client.Height - 30 - maxH) / 2;
            var drawRect = new Rectangle(drawX, drawY, maxW, maxH + 30);

            // Передаём рисование в ShipRenderer
            ShipRenderer.DrawShip(e.Graphics, _currentShip, drawRect);
        }

        /// Отрисовка панели информации о корабле.
        /// Показывает тип, имя и все характеристики с цветными прогресс-барами.
        ///
        /// Прогресс-бары:
        ///   - Корпус (зелёный)     — макс 200
        ///   - Щит (голубой)        — макс 150
        ///   - Скорость (жёлтый)    — макс 200
        ///   - Урон (оранжево-красный) — макс 100
        private void PanelInfo_Paint(object? sender, PaintEventArgs e)
        {
            if (_currentShip == null) return;

            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Шрифты для отрисовки
            using var font = new Font("Consolas", 9.5f);                    // основной текст
            using var headerFont = new Font("Segoe UI", 10, FontStyle.Bold); // заголовок

            // Кисти для текста
            using var brush = new SolidBrush(Color.White);                           // белый текст
            using var valueBrush = new SolidBrush(Color.LightGreen);                 // зелёный для имени
            using var dimBrush = new SolidBrush(Color.FromArgb(170, Color.Silver));  // серый для оружия

            int x = 12, y = 10;                                       // начальная позиция текста
            int barX = 12;                                              // отступ прогресс-баров
            int barMaxW = Math.Max(50, panelInfo.ClientSize.Width - 30); // ширина баров (адаптивная)

            // --- Заголовок: тип корабля ---
            g.DrawString($"Type: {_currentShip.ShipType}", headerFont, brush, x, y);
            y += 26;

            // --- Имя корабля (зелёным цветом) ---
            g.DrawString($"Name: {_currentShip.Name}", font, valueBrush, x, y);
            y += 26;

            // --- Корпус + зелёный прогресс-бар ---
            g.DrawString($"Hull: {_currentShip.HullStrength}", font, brush, x, y);
            y += 18;
            DrawBar(g, barX, y, barMaxW, 12, _currentShip.HullStrength, 200, Color.Green);
            y += 22;

            // --- Щит + голубой прогресс-бар ---
            g.DrawString($"Shield: {_currentShip.ShieldLevel}", font, brush, x, y);
            y += 18;
            DrawBar(g, barX, y, barMaxW, 12, _currentShip.ShieldLevel, 150, Color.DodgerBlue);
            y += 22;

            // --- Скорость + жёлтый прогресс-бар ---
            g.DrawString($"Speed: {_currentShip.Speed}", font, brush, x, y);
            y += 18;
            DrawBar(g, barX, y, barMaxW, 12, _currentShip.Speed, 200, Color.Yellow);
            y += 26;

            // --- Тип оружия (серым, приглушённым цветом) ---
            g.DrawString($"Weapon: {_currentShip.Weapon.Name}", font, dimBrush, x, y);
            y += 22;

            // --- Урон + оранжево-красный прогресс-бар ---
            g.DrawString($"Damage: {_currentShip.Weapon.Damage}", font, brush, x, y);
            y += 18;
            DrawBar(g, barX, y, barMaxW, 12, _currentShip.Weapon.Damage, 100, Color.OrangeRed);
        }

        /// Рисует горизонтальный прогресс-бар.
        ///
        /// Структура бара:
        ///   [████████░░░░░░░░░] ← тёмный фон + цветная заливка + тонкая рамка
        ///
        /// Длина цветной части = (value / maxValue) * maxWidth.
        private static void DrawBar(Graphics g, int x, int y, int maxWidth, int height,
            int value, int maxValue, Color color)
        {
            // Вычисляем заполненность (от 0.0 до 1.0)
            float ratio = Math.Min((float)value / maxValue, 1f);
            int barWidth = (int)(maxWidth * ratio);

            // Тёмный фон (незаполненная часть)
            using var bgBrush = new SolidBrush(Color.FromArgb(40, 40, 60));
            g.FillRectangle(bgBrush, x, y, maxWidth, height);

            // Цветная заливка (заполненная часть)
            using var barBrush = new SolidBrush(Color.FromArgb(200, color));
            g.FillRectangle(barBrush, x, y, barWidth, height);

            // Тонкая рамка вокруг всего бара
            using var borderPen = new Pen(Color.FromArgb(80, Color.White));
            g.DrawRectangle(borderPen, x, y, maxWidth, height);
        }

        /// Вспомогательный метод — ограничивает значение диапазоном [min, max].
        /// Нужен при установке значений NumericUpDown, чтобы не выйти за пределы.
        private static int Clamp(int value, int min, int max) =>
            Math.Max(min, Math.Min(max, value));
    }
}
