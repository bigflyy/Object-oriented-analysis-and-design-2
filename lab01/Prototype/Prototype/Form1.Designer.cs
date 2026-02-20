// =============================================================================
// Form1.Designer.cs — Разметка и создание визуальных элементов формы
// =============================================================================
// Этот файл создаёт и настраивает ВСЕ визуальные контролы:
//
// ТАБЛИЧНАЯ СТРУКТУРА:
//   Tab 1 "Shipyard" — Строительство кораблей:
//     - Левая панель: выбор прототипа, редактирование свойств
//     - Правая область: предпросмотр корабля + панель характеристик
//
//   Tab 2 "Fleet Command" — Управление флотом и бой:
//     - Левая колонка: флот игрока (визуальные карточки кораблей)
//     - Центральная колонка: лог боя + кнопки управления
//     - Правая колонка: вражеский флот (визуальные карточки врагов)
//
// Тёмная тема: фон RGB(20,20,35), панели RGB(30,30,50), контролы RGB(50,50,75)
// =============================================================================

namespace Prototype
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        /// Освобождение ресурсов формы.
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// Создание и настройка всех визуальных элементов формы.
        /// Вызывается из конструктора Form1().
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 720);   // увеличенный размер для табов
            this.MinimumSize = new System.Drawing.Size(1000, 650);  // минимальный размер
            this.Text = "Starship Fleet Builder — Prototype Pattern";
            this.BackColor = Color.FromArgb(20, 20, 35);            // тёмный фон формы
            this.DoubleBuffered = true;                              // двойная буферизация формы

            // =================================================================
            // TAB CONTROL — главный контейнер для табов
            // =================================================================
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.BackColor = Color.FromArgb(30, 30, 50);
            tabControl.Font = new Font("Segoe UI", 10f, FontStyle.Bold);

            // =================================================================
            // TAB 1: SHIPYARD — Строительство кораблей
            // =================================================================
            tabShipyard = new TabPage("Shipyard");
            tabShipyard.BackColor = Color.FromArgb(20, 20, 35);
            tabShipyard.Padding = new Padding(0);

            // --- ЛЕВАЯ ПАНЕЛЬ (переиспользуется из старой версии) ---
            panelLeft = new Panel();
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Width = 290;
            panelLeft.BackColor = Color.FromArgb(30, 30, 50);

            // Координаты для размещения контролов внутри левой панели
            int x1 = 12;    // колонка надписей
            int x2 = 100;   // колонка контролов
            int ctrlW = 170; // ширина контролов
            int y = 12;      // текущая вертикальная позиция
            int rowH = 32;   // высота одного ряда

            // =================================================================
            // СЕКЦИЯ: Выбор прототипа
            // =================================================================
            lblSelectPrototype = new Label();
            lblSelectPrototype.Text = "Select Prototype:";
            lblSelectPrototype.ForeColor = Color.White;
            lblSelectPrototype.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblSelectPrototype.Location = new Point(x1, y);
            lblSelectPrototype.AutoSize = true;
            y += 30;

            rbFighter = new RadioButton();
            rbFighter.Text = "Fighter";
            rbFighter.ForeColor = Color.LightSkyBlue;
            rbFighter.Font = new Font("Segoe UI", 9);
            rbFighter.Location = new Point(x1, y);
            rbFighter.Size = new Size(85, 24);
            rbFighter.Checked = true;
            rbFighter.CheckedChanged += OnPrototypeChanged;

            rbCruiser = new RadioButton();
            rbCruiser.Text = "Cruiser";
            rbCruiser.ForeColor = Color.Gold;
            rbCruiser.Font = new Font("Segoe UI", 9);
            rbCruiser.Location = new Point(x1 + 88, y);
            rbCruiser.Size = new Size(85, 24);
            rbCruiser.CheckedChanged += OnPrototypeChanged;

            rbBomber = new RadioButton();
            rbBomber.Text = "Bomber";
            rbBomber.ForeColor = Color.Salmon;
            rbBomber.Font = new Font("Segoe UI", 9);
            rbBomber.Location = new Point(x1 + 176, y);
            rbBomber.Size = new Size(90, 24);
            rbBomber.CheckedChanged += OnPrototypeChanged;
            y += 32;

            // =================================================================
            // СЕКЦИЯ: Свойства корабля
            // =================================================================
            lblProperties = new Label();
            lblProperties.Text = "Properties";
            lblProperties.ForeColor = Color.FromArgb(140, 140, 170);
            lblProperties.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblProperties.Location = new Point(x1, y);
            lblProperties.AutoSize = true;

            var sep1 = new Label();
            sep1.BackColor = Color.FromArgb(60, 60, 85);
            sep1.Location = new Point(x1 + 80, y + 9);
            sep1.Size = new Size(190, 1);
            panelLeft.Controls.Add(sep1);
            y += 28;

            lblName = CreateLabel("Name:", x1, y);
            txtName = new TextBox();
            txtName.Location = new Point(x2, y);
            txtName.Width = ctrlW;
            txtName.BackColor = Color.FromArgb(50, 50, 75);
            txtName.ForeColor = Color.White;
            txtName.Font = new Font("Segoe UI", 9.5f);
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtName.MaxLength = 20;  // Ограничение: максимум 20 символов
            txtName.TextChanged += OnPropertyChanged;
            y += rowH;

            lblHull = CreateLabel("Hull:", x1, y);
            nudHull = CreateNumeric(x2, y, 10, 200, 60);
            nudHull.ValueChanged += OnPropertyChanged;
            y += rowH;

            lblShield = CreateLabel("Shield:", x1, y);
            nudShield = CreateNumeric(x2, y, 0, 150, 30);
            nudShield.ValueChanged += OnPropertyChanged;
            y += rowH;

            lblSpeed = CreateLabel("Speed:", x1, y);
            nudSpeed = CreateNumeric(x2, y, 10, 200, 180);
            nudSpeed.ValueChanged += OnPropertyChanged;
            y += rowH;

            lblColor = CreateLabel("Color:", x1, y);
            cmbColor = new ComboBox();
            cmbColor.Location = new Point(x2, y);
            cmbColor.Width = ctrlW;
            cmbColor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbColor.BackColor = Color.FromArgb(50, 50, 75);
            cmbColor.ForeColor = Color.White;
            cmbColor.Font = new Font("Segoe UI", 9.5f);
            cmbColor.FlatStyle = FlatStyle.Flat;
            cmbColor.Items.AddRange(new object[] {
                "LightSkyBlue", "Red", "Green", "Gold", "Orange",
                "Magenta", "Cyan", "Lime", "Salmon", "White"
            });
            cmbColor.SelectedIndexChanged += OnPropertyChanged;
            y += rowH + 8;

            // =================================================================
            // СЕКЦИЯ: Система вооружения
            // =================================================================
            lblWeaponHeader = new Label();
            lblWeaponHeader.Text = "Weapon System";
            lblWeaponHeader.ForeColor = Color.FromArgb(140, 140, 170);
            lblWeaponHeader.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lblWeaponHeader.Location = new Point(x1, y);
            lblWeaponHeader.AutoSize = true;

            var sep2 = new Label();
            sep2.BackColor = Color.FromArgb(60, 60, 85);
            sep2.Location = new Point(x1 + 110, y + 9);
            sep2.Size = new Size(160, 1);
            panelLeft.Controls.Add(sep2);
            y += 28;

            lblWeapon = CreateLabel("Weapon:", x1, y);
            cmbWeapon = new ComboBox();
            cmbWeapon.Location = new Point(x2, y);
            cmbWeapon.Width = ctrlW;
            cmbWeapon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWeapon.BackColor = Color.FromArgb(50, 50, 75);
            cmbWeapon.ForeColor = Color.White;
            cmbWeapon.Font = new Font("Segoe UI", 9.5f);
            cmbWeapon.FlatStyle = FlatStyle.Flat;
            cmbWeapon.Items.AddRange(new object[] {
                "Laser Cannon", "Plasma Turret", "Missile Rack", "Torpedo Bay", "Ion Beam"
            });
            cmbWeapon.SelectedIndexChanged += OnPropertyChanged;
            y += rowH;

            lblDamage = CreateLabel("Damage:", x1, y);
            nudDamage = CreateNumeric(x2, y, 5, 100, 25);
            nudDamage.ValueChanged += OnPropertyChanged;
            y += rowH + 16;

            // =================================================================
            // КНОПКИ — только кнопки для строительства кораблей
            // (Battle/Repair/Clear переехали в Tab 2)
            // =================================================================
            btnClone = new Button();
            btnClone.Text = "Clone to Fleet";
            btnClone.Location = new Point(x1, y);
            btnClone.Size = new Size(260, 38);
            btnClone.BackColor = Color.FromArgb(50, 120, 50);
            btnClone.ForeColor = Color.White;
            btnClone.FlatStyle = FlatStyle.Flat;
            btnClone.FlatAppearance.BorderColor = Color.FromArgb(70, 160, 70);
            btnClone.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            btnClone.Click += BtnClone_Click;
            y += 46;

            btnDeepCopyDemo = new Button();
            btnDeepCopyDemo.Text = "Deep Copy Demo";
            btnDeepCopyDemo.Location = new Point(x1, y);
            btnDeepCopyDemo.Size = new Size(260, 32);
            btnDeepCopyDemo.BackColor = Color.FromArgb(90, 55, 130);
            btnDeepCopyDemo.ForeColor = Color.White;
            btnDeepCopyDemo.FlatStyle = FlatStyle.Flat;
            btnDeepCopyDemo.FlatAppearance.BorderColor = Color.FromArgb(120, 80, 170);
            btnDeepCopyDemo.Font = new Font("Segoe UI", 9.5f);
            btnDeepCopyDemo.Click += BtnDeepCopyDemo_Click;

            // Добавляем все контролы в левую панель
            panelLeft.Controls.AddRange(new Control[] {
                lblSelectPrototype, rbFighter, rbCruiser, rbBomber,
                lblProperties, lblName, txtName,
                lblHull, nudHull,
                lblShield, nudShield,
                lblSpeed, nudSpeed,
                lblColor, cmbColor,
                lblWeaponHeader, lblWeapon, cmbWeapon,
                lblDamage, nudDamage,
                btnClone, btnDeepCopyDemo
            });

            // --- ПРАВАЯ ОБЛАСТЬ TAB 1 — предпросмотр и информация ---
            panelShipyardRight = new Panel();
            panelShipyardRight.Dock = DockStyle.Fill;
            panelShipyardRight.BackColor = Color.FromArgb(20, 20, 35);
            panelShipyardRight.Padding = new Padding(10);

            // TableLayoutPanel для размещения предпросмотра (45%) и информации (55%)
            panelTopRow = new TableLayoutPanel();
            panelTopRow.Dock = DockStyle.Fill;
            panelTopRow.ColumnCount = 2;
            panelTopRow.RowCount = 1;
            panelTopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            panelTopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            panelTopRow.BackColor = Color.FromArgb(20, 20, 35);

            // Панель предпросмотра корабля
            lblPreview = new Label();
            lblPreview.Text = "Ship Preview:";
            lblPreview.ForeColor = Color.White;
            lblPreview.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblPreview.Dock = DockStyle.Top;
            lblPreview.Height = 25;

            panelPreview = new DoubleBufferedPanel();
            panelPreview.Dock = DockStyle.Fill;
            panelPreview.BackColor = Color.FromArgb(10, 10, 25);
            panelPreview.BorderStyle = BorderStyle.FixedSingle;
            panelPreview.Paint += PanelPreview_Paint;

            var previewContainer = new Panel();
            previewContainer.Dock = DockStyle.Fill;
            previewContainer.Padding = new Padding(5, 5, 3, 5);
            previewContainer.Controls.Add(panelPreview);
            previewContainer.Controls.Add(lblPreview);

            // Панель информации о корабле
            lblInfo = new Label();
            lblInfo.Text = "Current Ship Info:";
            lblInfo.ForeColor = Color.White;
            lblInfo.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Height = 25;

            panelInfo = new DoubleBufferedPanel();
            panelInfo.Dock = DockStyle.Fill;
            panelInfo.BackColor = Color.FromArgb(10, 10, 25);
            panelInfo.BorderStyle = BorderStyle.FixedSingle;
            panelInfo.Paint += PanelInfo_Paint;

            var infoContainer = new Panel();
            infoContainer.Dock = DockStyle.Fill;
            infoContainer.Padding = new Padding(3, 5, 5, 5);
            infoContainer.Controls.Add(panelInfo);
            infoContainer.Controls.Add(lblInfo);

            panelTopRow.Controls.Add(previewContainer, 0, 0);
            panelTopRow.Controls.Add(infoContainer, 1, 0);

            panelShipyardRight.Controls.Add(panelTopRow);

            // Собираем Tab 1
            tabShipyard.Controls.Add(panelShipyardRight);
            tabShipyard.Controls.Add(panelLeft);

            // =================================================================
            // TAB 2: FLEET COMMAND — Управление флотом и бой
            // =================================================================
            tabFleetCommand = new TabPage("Fleet Command");
            tabFleetCommand.BackColor = Color.FromArgb(20, 20, 35);
            tabFleetCommand.Padding = new Padding(10);

            // TableLayoutPanel с 3 колонками: флот игрока (33%) | зона боя (34%) | флот врага (33%)
            panelFleetLayout = new TableLayoutPanel();
            panelFleetLayout.Dock = DockStyle.Fill;
            panelFleetLayout.ColumnCount = 3;
            panelFleetLayout.RowCount = 1;
            panelFleetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            panelFleetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            panelFleetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            panelFleetLayout.BackColor = Color.FromArgb(20, 20, 35);

            // --- ЛЕВАЯ КОЛОНКА: Флот игрока ---
            lblPlayerFleet = new Label();
            lblPlayerFleet.Text = "Player Fleet:";
            lblPlayerFleet.ForeColor = Color.White;
            lblPlayerFleet.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblPlayerFleet.Dock = DockStyle.Top;
            lblPlayerFleet.Height = 30;
            lblPlayerFleet.TextAlign = ContentAlignment.MiddleCenter;

            playerFleetPanel = new FleetPanel();
            playerFleetPanel.Dock = DockStyle.Fill;

            var playerFleetContainer = new Panel();
            playerFleetContainer.Dock = DockStyle.Fill;
            playerFleetContainer.Padding = new Padding(0, 0, 5, 0);
            playerFleetContainer.Controls.Add(playerFleetPanel);
            playerFleetContainer.Controls.Add(lblPlayerFleet);

            // --- ЦЕНТРАЛЬНАЯ КОЛОНКА: Зона боя (лог + кнопки) ---
            lblBattleLog = new Label();
            lblBattleLog.Text = "Battle Log:";
            lblBattleLog.ForeColor = Color.White;
            lblBattleLog.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblBattleLog.Dock = DockStyle.Top;
            lblBattleLog.Height = 30;
            lblBattleLog.TextAlign = ContentAlignment.MiddleCenter;

            txtBattleLog = new TextBox();
            txtBattleLog.Dock = DockStyle.Fill;
            txtBattleLog.BackColor = Color.FromArgb(15, 15, 30);
            txtBattleLog.ForeColor = Color.LightGreen;
            txtBattleLog.Font = new Font("Consolas", 9f);
            txtBattleLog.BorderStyle = BorderStyle.FixedSingle;
            txtBattleLog.Multiline = true;
            txtBattleLog.ScrollBars = ScrollBars.Vertical;
            txtBattleLog.ReadOnly = true;
            txtBattleLog.Text = "Click 'Battle!' to start a fleet battle.\r\n\r\nYour fleet will fight against a randomly generated enemy fleet.\r\n\r\nSpeed determines turn order.\r\nDamage reduces shields first, then hull.";

            // Панель кнопок (внизу зоны боя)
            panelBattleButtons = new Panel();
            panelBattleButtons.Dock = DockStyle.Bottom;
            panelBattleButtons.Height = 140;
            panelBattleButtons.BackColor = Color.FromArgb(20, 20, 35);
            panelBattleButtons.Padding = new Padding(10, 5, 10, 5);

            btnBattle = new Button();
            btnBattle.Text = "⚔ Battle!";
            btnBattle.Dock = DockStyle.Top;
            btnBattle.Height = 40;
            btnBattle.BackColor = Color.FromArgb(180, 50, 50);
            btnBattle.ForeColor = Color.White;
            btnBattle.FlatStyle = FlatStyle.Flat;
            btnBattle.FlatAppearance.BorderColor = Color.FromArgb(220, 80, 80);
            btnBattle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnBattle.Click += BtnBattle_Click;

            btnRepair = new Button();
            btnRepair.Text = "🔧 Repair Fleet";
            btnRepair.Location = new Point(10, 50);
            btnRepair.Size = new Size(0, 32);  // Width will be set by Dock
            btnRepair.Dock = DockStyle.Top;
            btnRepair.Height = 32;
            btnRepair.BackColor = Color.FromArgb(40, 120, 140);
            btnRepair.ForeColor = Color.White;
            btnRepair.FlatStyle = FlatStyle.Flat;
            btnRepair.FlatAppearance.BorderColor = Color.FromArgb(60, 160, 180);
            btnRepair.Font = new Font("Segoe UI", 9.5f);
            btnRepair.Click += BtnRepair_Click;

            btnClearFleet = new Button();
            btnClearFleet.Text = "Clear Fleet";
            btnClearFleet.Location = new Point(10, 90);
            btnClearFleet.Size = new Size(0, 32);  // Width will be set by Dock
            btnClearFleet.Dock = DockStyle.Top;
            btnClearFleet.Height = 32;
            btnClearFleet.BackColor = Color.FromArgb(130, 45, 45);
            btnClearFleet.ForeColor = Color.White;
            btnClearFleet.FlatStyle = FlatStyle.Flat;
            btnClearFleet.FlatAppearance.BorderColor = Color.FromArgb(170, 60, 60);
            btnClearFleet.Font = new Font("Segoe UI", 9.5f);
            btnClearFleet.Click += BtnClearFleet_Click;

            panelBattleButtons.Controls.Add(btnClearFleet);
            panelBattleButtons.Controls.Add(btnRepair);
            panelBattleButtons.Controls.Add(btnBattle);

            var battleZoneContainer = new Panel();
            battleZoneContainer.Dock = DockStyle.Fill;
            battleZoneContainer.Padding = new Padding(5, 0, 5, 0);
            battleZoneContainer.Controls.Add(txtBattleLog);
            battleZoneContainer.Controls.Add(panelBattleButtons);
            battleZoneContainer.Controls.Add(lblBattleLog);

            // --- ПРАВАЯ КОЛОНКА: Вражеский флот ---
            lblEnemyFleet = new Label();
            lblEnemyFleet.Text = "Enemy Fleet:";
            lblEnemyFleet.ForeColor = Color.FromArgb(255, 100, 100);
            lblEnemyFleet.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblEnemyFleet.Dock = DockStyle.Top;
            lblEnemyFleet.Height = 30;
            lblEnemyFleet.TextAlign = ContentAlignment.MiddleCenter;

            enemyFleetPanel = new FleetPanel();
            enemyFleetPanel.Dock = DockStyle.Fill;

            var enemyFleetContainer = new Panel();
            enemyFleetContainer.Dock = DockStyle.Fill;
            enemyFleetContainer.Padding = new Padding(5, 0, 0, 0);
            enemyFleetContainer.Controls.Add(enemyFleetPanel);
            enemyFleetContainer.Controls.Add(lblEnemyFleet);

            // Добавляем колонки в TableLayoutPanel
            panelFleetLayout.Controls.Add(playerFleetContainer, 0, 0);
            panelFleetLayout.Controls.Add(battleZoneContainer, 1, 0);
            panelFleetLayout.Controls.Add(enemyFleetContainer, 2, 0);

            tabFleetCommand.Controls.Add(panelFleetLayout);

            // --- СКРЫТЫЙ ListBox для обратной совместимости с Form1.cs ---
            // Form1.cs ещё использует lstFleet.Items.Add(), поэтому оставляем его
            lstFleet = new ListBox();
            lstFleet.Visible = false;  // скрываем, используем только для хранения данных

            // =================================================================
            // ДОБАВЛЕНИЕ ТАБОВ В TAB CONTROL
            // =================================================================
            tabControl.TabPages.Add(tabShipyard);
            tabControl.TabPages.Add(tabFleetCommand);

            // Добавляем TabControl в форму
            this.Controls.Add(tabControl);
        }

        /// Вспомогательный метод — создаёт надпись (Label) с тёмной темой.
        private Label CreateLabel(string text, int x, int y)
        {
            var lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = Color.FromArgb(200, 200, 220);
            lbl.Font = new Font("Segoe UI", 9.5f);
            lbl.Location = new Point(x, y + 2);
            lbl.AutoSize = true;
            return lbl;
        }

        /// Вспомогательный метод — создаёт числовой контрол (NumericUpDown) с тёмной темой.
        private NumericUpDown CreateNumeric(int x, int y, int min, int max, int value)
        {
            var nud = new NumericUpDown();
            nud.Location = new Point(x, y);
            nud.Width = 80;
            nud.Minimum = min;
            nud.Maximum = max;
            nud.Value = value;
            nud.BackColor = Color.FromArgb(50, 50, 75);
            nud.ForeColor = Color.White;
            nud.Font = new Font("Segoe UI", 9.5f);
            nud.BorderStyle = BorderStyle.FixedSingle;
            return nud;
        }

        #endregion

        // =====================================================================
        // ОБЪЯВЛЕНИЯ ПОЛЕЙ — все визуальные контролы формы
        // =====================================================================

        // --- Tab Control ---
        private TabControl tabControl;              // главный контейнер табов
        private TabPage tabShipyard;                // таб 1: строительство кораблей
        private TabPage tabFleetCommand;            // таб 2: управление флотом

        // --- Tab 1: Shipyard (левая панель) ---
        private Panel panelLeft;                    // левая панель (Dock.Left, 290px)
        private Label lblSelectPrototype;           // заголовок «Select Prototype:»
        private RadioButton rbFighter;              // кнопка выбора истребителя
        private RadioButton rbCruiser;              // кнопка выбора крейсера
        private RadioButton rbBomber;               // кнопка выбора бомбардировщика
        private Label lblProperties;                // заголовок «Properties»
        private Label lblName;                      // надпись «Name:»
        private TextBox txtName;                    // поле ввода имени
        private Label lblHull;                      // надпись «Hull:»
        private NumericUpDown nudHull;              // числовое поле корпуса
        private Label lblShield;                    // надпись «Shield:»
        private NumericUpDown nudShield;            // числовое поле щита
        private Label lblSpeed;                     // надпись «Speed:»
        private NumericUpDown nudSpeed;             // числовое поле скорости
        private Label lblColor;                     // надпись «Color:»
        private ComboBox cmbColor;                  // выпадающий список цветов
        private Label lblWeaponHeader;              // заголовок «Weapon System»
        private Label lblWeapon;                    // надпись «Weapon:»
        private ComboBox cmbWeapon;                 // выпадающий список типов оружия
        private Label lblDamage;                    // надпись «Damage:»
        private NumericUpDown nudDamage;            // числовое поле урона
        private Button btnClone;                    // кнопка «Clone to Fleet»
        private Button btnDeepCopyDemo;             // кнопка «Deep Copy Demo»

        // --- Tab 1: Shipyard (правая область) ---
        private Panel panelShipyardRight;           // правая область таба Shipyard
        private TableLayoutPanel panelTopRow;       // верхний ряд (предпросмотр + инфо)
        private Label lblPreview;                   // заголовок «Ship Preview:»
        private Panel panelPreview;                 // панель предпросмотра (GDI+)
        private Panel panelInfo;                    // панель информации
        private Label lblInfo;                      // заголовок «Current Ship Info:»

        // --- Tab 2: Fleet Command ---
        private TableLayoutPanel panelFleetLayout;  // 3-колоночная разметка
        private Label lblPlayerFleet;               // заголовок «Player Fleet:»
        private FleetPanel playerFleetPanel;        // панель с визуальными карточками флота
        private Label lblBattleLog;                 // заголовок «Battle Log:»
        private TextBox txtBattleLog;               // лог боя
        private Panel panelBattleButtons;           // панель кнопок боя
        private Button btnBattle;                   // кнопка «Battle!»
        private Button btnRepair;                   // кнопка «Repair Fleet»
        private Button btnClearFleet;               // кнопка «Clear Fleet»
        private Label lblEnemyFleet;                // заголовок «Enemy Fleet:»
        private FleetPanel enemyFleetPanel;         // панель с вражескими карточками

        // --- Обратная совместимость ---
        private ListBox lstFleet;                   // скрытый ListBox (для старого кода)
    }
}
