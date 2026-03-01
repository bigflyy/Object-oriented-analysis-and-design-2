// Form1.cs — Основная логика формы (обработчики событий и управление флотом)
// Этот файл содержит логику приложения:
//   - Загрузка прототипов кораблей (Fighter, Cruiser, Bomber)
//   - Обработка изменений свойств (имя, корпус, щит, скорость, цвет, оружие)
//   - Клонирование корабля в флот (паттерн Прототип — метод Clone)
//   - Демонстрация глубокого копирования
//   - Отрисовка предпросмотра корабля и панели информации
//
// Визуальная часть формы (расположение элементов) — в Form1.Designer.cs.

using Prototype.Models;
using Prototype.UI;

namespace Prototype
{
    public partial class Form1 : Form
    {
        // Верфь — клиент паттерна Прототип. Хранит прототипы и клонирует их.
        private readonly ShipyardManual _shipyard = new();

        // Текущий корабль, отображаемый в редакторе (прототип-образец из верфи).
        // null! — потому что инициализируется в LoadPrototype(), вызываемом из конструктора.
        private Starship _currentShip = null!;

        // Список клонированных кораблей (флот).
        // Каждый элемент — независимая глубокая копия, созданная через Clone().
        private readonly List<Starship> _fleet = new();

        // Флаг для предотвращения рекурсивных обновлений:
        // когда мы программно устанавливаем значения элементов (в LoadPrototype),
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
            InitializeCustomControls();
            LoadPrototype();
        }

        /// Создаёт элементы, которые VS Designer не может обработать:
        /// надписи и числовые поля (созданные через вспомогательные методы),
        /// а также кастомные элементы (DoubleBufferedPanel, FleetPanel).
        private void InitializeCustomControls()
        {
            // Надписи для левой панели (leftGrid)
            lblName = new Label();
            lblName.Text = "Имя:";
            lblName.ForeColor = Color.FromArgb(200, 200, 220);
            lblName.Font = new Font("Segoe UI", 9.5f);
            lblName.Dock = DockStyle.Fill;
            lblName.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblName, 0, 3);

            lblHull = new Label();
            lblHull.Text = "Корпус:";
            lblHull.ForeColor = Color.FromArgb(200, 200, 220);
            lblHull.Font = new Font("Segoe UI", 9.5f);
            lblHull.Dock = DockStyle.Fill;
            lblHull.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblHull, 0, 4);

            lblShield = new Label();
            lblShield.Text = "Щит:";
            lblShield.ForeColor = Color.FromArgb(200, 200, 220);
            lblShield.Font = new Font("Segoe UI", 9.5f);
            lblShield.Dock = DockStyle.Fill;
            lblShield.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblShield, 0, 5);

            lblSpeed = new Label();
            lblSpeed.Text = "Скорость:";
            lblSpeed.ForeColor = Color.FromArgb(200, 200, 220);
            lblSpeed.Font = new Font("Segoe UI", 9.5f);
            lblSpeed.Dock = DockStyle.Fill;
            lblSpeed.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblSpeed, 0, 6);

            lblColor = new Label();
            lblColor.Text = "Цвет:";
            lblColor.ForeColor = Color.FromArgb(200, 200, 220);
            lblColor.Font = new Font("Segoe UI", 9.5f);
            lblColor.Dock = DockStyle.Fill;
            lblColor.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblColor, 0, 7);

            lblWeapon = new Label();
            lblWeapon.Text = "Оружие:";
            lblWeapon.ForeColor = Color.FromArgb(200, 200, 220);
            lblWeapon.Font = new Font("Segoe UI", 9.5f);
            lblWeapon.Dock = DockStyle.Fill;
            lblWeapon.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblWeapon, 0, 9);

            lblDamage = new Label();
            lblDamage.Text = "Урон:";
            lblDamage.ForeColor = Color.FromArgb(200, 200, 220);
            lblDamage.Font = new Font("Segoe UI", 9.5f);
            lblDamage.Dock = DockStyle.Fill;
            lblDamage.TextAlign = ContentAlignment.MiddleLeft;
            leftGrid.Controls.Add(lblDamage, 0, 10);

            // Числовые поля
            nudHull = new NumericUpDown();
            nudHull.Dock = DockStyle.Fill;
            nudHull.Minimum = 10;
            nudHull.Maximum = 200;
            nudHull.Value = 60;
            nudHull.BackColor = Color.FromArgb(50, 50, 75);
            nudHull.ForeColor = Color.White;
            nudHull.Font = new Font("Segoe UI", 9.5f);
            nudHull.BorderStyle = BorderStyle.FixedSingle;
            nudHull.ValueChanged += OnPropertyChanged;
            leftGrid.Controls.Add(nudHull, 1, 4);

            nudShield = new NumericUpDown();
            nudShield.Dock = DockStyle.Fill;
            nudShield.Minimum = 0;
            nudShield.Maximum = 150;
            nudShield.Value = 30;
            nudShield.BackColor = Color.FromArgb(50, 50, 75);
            nudShield.ForeColor = Color.White;
            nudShield.Font = new Font("Segoe UI", 9.5f);
            nudShield.BorderStyle = BorderStyle.FixedSingle;
            nudShield.ValueChanged += OnPropertyChanged;
            leftGrid.Controls.Add(nudShield, 1, 5);

            nudSpeed = new NumericUpDown();
            nudSpeed.Dock = DockStyle.Fill;
            nudSpeed.Minimum = 10;
            nudSpeed.Maximum = 200;
            nudSpeed.Value = 180;
            nudSpeed.BackColor = Color.FromArgb(50, 50, 75);
            nudSpeed.ForeColor = Color.White;
            nudSpeed.Font = new Font("Segoe UI", 9.5f);
            nudSpeed.BorderStyle = BorderStyle.FixedSingle;
            nudSpeed.ValueChanged += OnPropertyChanged;
            leftGrid.Controls.Add(nudSpeed, 1, 6);

            nudDamage = new NumericUpDown();
            nudDamage.Dock = DockStyle.Fill;
            nudDamage.Minimum = 5;
            nudDamage.Maximum = 100;
            nudDamage.Value = 25;
            nudDamage.BackColor = Color.FromArgb(50, 50, 75);
            nudDamage.ForeColor = Color.White;
            nudDamage.Font = new Font("Segoe UI", 9.5f);
            nudDamage.BorderStyle = BorderStyle.FixedSingle;
            nudDamage.ValueChanged += OnPropertyChanged;
            leftGrid.Controls.Add(nudDamage, 1, 10);

            // Панель предпросмотра корабля (кастомный элемент с двойной буферизацией)
            panelPreview = new DoubleBufferedPanel();
            panelPreview.Dock = DockStyle.Fill;
            panelPreview.BackColor = Color.FromArgb(10, 10, 25);
            panelPreview.BorderStyle = BorderStyle.FixedSingle;
            panelPreview.Paint += PanelPreview_Paint;
            previewContainer.Controls.Add(panelPreview);
            // Fill должен быть впереди (index 0), а Top-лейбл — позади (index 1),
            // чтобы WinForms корректно разместил: сначала Label (Top), потом Panel (Fill)
            panelPreview.BringToFront();

            // Панель информации о корабле
            panelInfo = new DoubleBufferedPanel();
            panelInfo.Dock = DockStyle.Fill;
            panelInfo.BackColor = Color.FromArgb(10, 10, 25);
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.Paint += PanelInfo_Paint;
            infoContainer.Controls.Add(panelInfo);
            panelInfo.BringToFront();

            // Панель флота игрока (кастомный элемент)
            playerFleetPanel = new FleetPanel();
            playerFleetPanel.Dock = DockStyle.Fill;
            playerFleetContainer.Controls.Add(playerFleetPanel);
            playerFleetPanel.BringToFront();

            // Панель вражеского флота
            enemyFleetPanel = new FleetPanel();
            enemyFleetPanel.Dock = DockStyle.Fill;
            enemyFleetContainer.Controls.Add(enemyFleetPanel);
            enemyFleetPanel.BringToFront();
        }

        // ЗАГРУЗКА ПРОТОТИПА — создаёт корабль с настройками по умолчанию

        /// Создаёт новый корабль-прототип в зависимости от выбранного RadioButton.
        /// Затем синхронизирует все элементы интерфейса с его свойствами.
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

            // Синхронизируем элементы интерфейса с данными корабля.
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
        private void OnPrototypeChanged(object sender, EventArgs e)
        {
            // Проверяем, что это RadioButton и он выбран
            // (событие CheckedChanged срабатывает и при снятии, и при установке)
            if (sender is RadioButton rb && rb.Checked)
                LoadPrototype();
        }

        /// Обработчик изменения любого свойства корабля
        /// (имя, корпус, щит, скорость, урон, тип оружия, цвет).
        /// Считывает значения из элементов и обновляет объект _currentShip.
        /// Затем перерисовывает панели.
        ///
        /// Флаг _updatingUI не даёт этому методу сработать, когда мы
        /// программно меняем значения элементов в LoadPrototype().
        private void OnPropertyChanged(object sender, EventArgs e)
        {
            if (_updatingUI || _currentShip == null) return;

            // Считываем значения из элементов интерфейса
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
        // КНОПКИ

        /// Кнопка «Clone to Fleet» — КЛОНИРОВАНИЕ (паттерн Прототип).
        /// Верфь клонирует текущий прототип, создавая глубокую копию.
        /// Добавляет клон в список флота. Клон полностью независим от прототипа.
        private void BtnClone_Click(object sender, EventArgs e)
        {
            string type = _currentShip.ShipType;
            Starship clone = _shipyard.BuildShip(type);  // Верфь клонирует прототип!
            _fleet.Add(clone);
            RefreshFleetDisplay();  // Обновляем отображение
        }

        /// Кнопка «Очистить флот» — очищает список флота и вражеского флота.
        private void BtnClearFleet_Click(object sender, EventArgs e)
        {
            _fleet.Clear();
            RefreshFleetDisplay();

            // : Также очищаем вражескую панель
            enemyFleetPanel.Clear();

            // Очищаем лог боя
            txtBattleLog.Text = "Нажмите «В бой!» чтобы начать сражение.\r\n\r\nВаш флот сразится со случайно сгенерированным флотом врага.\r\n\r\nСкорость определяет порядок ходов.\r\nУрон сначала снижает щит, затем корпус.";
        }

        /// Кнопка «Battle!» — запускает бой флота против случайных врагов.
        ///
        /// Алгоритм:
        /// 1. Проверяем, есть ли корабли во флоте игрока
        /// 2. Генерируем случайный вражеский флот того же размера
        /// 3. Переключаемся на вкладку Fleet Command для визуализации боя
        /// 4. Отображаем вражеский флот визуально (карточки с HP/Shield)
        /// 5. Запускаем автоматический пошаговый бой (BattleEngine)
        /// 6. : Отображаем бой раунд за раундом с задержкой между раундами
        /// 7. Обновляем визуальные панели флотов после каждого раунда
        /// 8. ОБНОВЛЯЕМ флот — урон сохраняется, уничтоженные корабли удаляются
        ///
        /// Механика боя:
        /// - Скорость определяет порядок ходов (быстрые корабли ходят первыми)
        /// - Каждый корабль атакует случайного врага
        /// - Урон сначала снижает щит, затем корпус
        /// - Уничтоженные корабли удаляются ИЗ ФЛОТА (реальные последствия!)
        /// - Бой продолжается до полного уничтожения одного из флотов
        private async void BtnBattle_Click(object sender, EventArgs e)
        {
            // Проверяем, есть ли корабли во флоте
            if (_fleet.Count == 0)
            {
                MessageBox.Show("Ваш флот пуст! Сначала клонируйте корабли.",
                    "Нет флота", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            txtBattleLog.Text = "=== ОБНАРУЖЕН ВРАЖЕСКИЙ ФЛОТ ===\r\n" +
                                $"Вражеских кораблей: {enemyFleet.Count}\r\n" +
                                "Подготовка к бою...\r\n\r\n";

            // Отключаем элементы управления на время боя
            btnBattle.Enabled = false;
            btnClone.Enabled = false;
            btnClearFleet.Enabled = false;
            btnRepair.Enabled = false;
            leftGrid.Enabled = false;

            // Переменная для отслеживания текущего раунда
            int lastRound = 0;
            int lastPlayerCount = _fleet.Count;
            int lastEnemyCount = enemyFleet.Count;

            // Запускаем бой с callback для обновления UI в реальном времени
            var battleLog = await battleEngine.RunBattleAsync(_fleet, enemyFleet, async (evt) =>
            {
                // Пропускаем начальные сообщения (уже показаны выше)
                if (evt.Message.Contains("НАЧАЛО БОЯ") || evt.Message.Contains("кораблей"))
                    return;

                // Добавляем сообщение в лог
                txtBattleLog.AppendText(evt.Message + "\r\n");
                txtBattleLog.SelectionStart = txtBattleLog.Text.Length;
                txtBattleLog.ScrollToCaret();

                // Пересоздаём карточки только если количество кораблей изменилось (корабль уничтожен)
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

                // Подсветка цели: красная = попадание, синяя = промах
                if (evt.Target != null)
                {
                    if (evt.IsMiss)
                    {
                        if (_fleet.Contains(evt.Target))
                            _ = playerFleetPanel.FlashMissShip(evt.Target);
                        else if (enemyFleet.Contains(evt.Target))
                            _ = enemyFleetPanel.FlashMissShip(evt.Target);
                    }
                    else
                    {
                        if (_fleet.Contains(evt.Target))
                            _ = playerFleetPanel.FlashShip(evt.Target);
                        else if (enemyFleet.Contains(evt.Target))
                            _ = enemyFleetPanel.FlashShip(evt.Target);
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
            },
            // Назначение целей перед каждым раундом
            async (playerShips, enemyShips) =>
            {
                txtBattleLog.AppendText("=== НАЗНАЧЕНИЕ ЦЕЛЕЙ ===\r\n");
                txtBattleLog.SelectionStart = txtBattleLog.Text.Length;
                txtBattleLog.ScrollToCaret();

                var assignments = new Dictionary<Starship, Starship>();
                foreach (var ship in playerShips)
                {
                    txtBattleLog.AppendText($">> {ship.Name} ({ship.ShipType}) — выберите цель!\r\n");
                    txtBattleLog.SelectionStart = txtBattleLog.Text.Length;
                    txtBattleLog.ScrollToCaret();

                    playerFleetPanel.HighlightAttacker(ship);
                    var selected = await enemyFleetPanel.WaitForSelection();
                    playerFleetPanel.ClearAttacker(ship);

                    assignments[ship] = selected;
                    txtBattleLog.AppendText($"   {ship.Name} -> {selected.Name}\r\n");
                }
                txtBattleLog.AppendText("\r\n");
                return assignments;
            });

            // Добавляем статистику потерь
            int shipsLost = initialFleetSize - _fleet.Count;
            if (shipsLost > 0)
            {
                txtBattleLog.AppendText($"\r\nВаш флот потерял {shipsLost} кораб.(ей) в бою.\r\n");
            }
            if (_fleet.Count > 0)
            {
                txtBattleLog.AppendText($"Выжившие корабли могут быть повреждены.\r\n");
            }

            // Финальное обновление флотов
            playerFleetPanel.SetFleet(_fleet);
            enemyFleetPanel.SetFleet(enemyFleet);

            // Автоматически прокручиваем вниз
            txtBattleLog.SelectionStart = txtBattleLog.Text.Length;
            txtBattleLog.ScrollToCaret();

            // Включаем элементы управления обратно
            btnBattle.Enabled = true;
            btnClone.Enabled = true;
            btnClearFleet.Enabled = true;
            btnRepair.Enabled = true;
            leftGrid.Enabled = true;
        }

        /// Кнопка «Repair Fleet» — ремонтирует все корабли во флоте.
        /// Восстанавливает Hull и Shield всех кораблей до их максимальных значений.
        /// Полезно после боя для подготовки к следующему сражению.
        private void BtnRepair_Click(object sender, EventArgs e)
        {
            if (_fleet.Count == 0)
            {
                MessageBox.Show("Флот пуст! Нечего ремонтировать.",
                    "Нет флота", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show($"Отремонтировано {repairedCount} повреждённых кораблей.\nВсе корабли восстановлены!",
                    "Ремонт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Все корабли уже полностью исправны!",
                    "Ремонт завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// Обновляет отображение флота в визуальной панели.
        private void RefreshFleetDisplay()
        {
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
        private void PanelPreview_Paint(object sender, PaintEventArgs e)
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
        private void PanelInfo_Paint(object sender, PaintEventArgs e)
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

            // Вычисляем размер строки для адаптивных отступов (учитывает DPI)
            int lineH = (int)g.MeasureString("X", font).Height;
            int headerH = (int)g.MeasureString("X", headerFont).Height;
            int barH = Math.Max(14, lineH / 2);  // высота прогресс-бара
            int gap = Math.Max(6, lineH / 3);    // отступ между элементами

            int x = 12, y = headerH;             // начальная позиция — отступ = высота заголовка
            int barX = 12;                        // отступ прогресс-баров
            int barMaxW = Math.Max(50, panelInfo.ClientSize.Width - 30); // ширина баров (адаптивная)

            // --- Заголовок: тип корабля ---
            g.DrawString($"Тип: {_currentShip.ShipType}", headerFont, brush, x, y);
            y += headerH + gap;

            // --- Имя корабля (зелёным цветом) ---
            g.DrawString($"Имя: {_currentShip.Name}", font, valueBrush, x, y);
            y += lineH + gap + 4;

            // --- Корпус + зелёный прогресс-бар ---
            g.DrawString($"Корпус: {_currentShip.HullStrength}", font, brush, x, y);
            y += lineH + 2;
            DrawBar(g, barX, y, barMaxW, barH, _currentShip.HullStrength, 200, Color.Green);
            y += barH + gap + 2;

            // --- Щит + голубой прогресс-бар ---
            g.DrawString($"Щит: {_currentShip.ShieldLevel}", font, brush, x, y);
            y += lineH + 2;
            DrawBar(g, barX, y, barMaxW, barH, _currentShip.ShieldLevel, 150, Color.DodgerBlue);
            y += barH + gap + 2;

            // --- Скорость + жёлтый прогресс-бар ---
            g.DrawString($"Скорость: {_currentShip.Speed}", font, brush, x, y);
            y += lineH + 2;
            DrawBar(g, barX, y, barMaxW, barH, _currentShip.Speed, 200, Color.Yellow);
            y += barH + gap + 6;

            // --- Тип оружия (серым, приглушённым цветом) ---
            g.DrawString($"Оружие: {_currentShip.Weapon.Name}", font, dimBrush, x, y);
            y += lineH + gap;

            // --- Урон + оранжево-красный прогресс-бар ---
            g.DrawString($"Урон: {_currentShip.Weapon.Damage}", font, brush, x, y);
            y += lineH + 2;
            DrawBar(g, barX, y, barMaxW, barH, _currentShip.Weapon.Damage, 100, Color.OrangeRed);
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
