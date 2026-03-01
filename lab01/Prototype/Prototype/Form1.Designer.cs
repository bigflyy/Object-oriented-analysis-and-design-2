// Form1.Designer.cs — Разметка и создание визуальных элементов формы
// Этот файл создаёт и настраивает ВСЕ визуальные элементы:
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
// Тёмная тема: фон RGB(20,20,35), панели RGB(30,30,50), элементы RGB(50,50,75)

using Prototype.UI;

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
            tabControl = new TabControl();
            tabShipyard = new TabPage();
            panelShipyardRight = new Panel();
            panelTopRow = new TableLayoutPanel();
            previewContainer = new Panel();
            lblPreview = new Label();
            infoContainer = new Panel();
            lblInfo = new Label();
            panelLeft = new Panel();
            leftGrid = new TableLayoutPanel();
            lblSelectPrototype = new Label();
            radioPanel = new FlowLayoutPanel();
            rbFighter = new RadioButton();
            rbCruiser = new RadioButton();
            rbBomber = new RadioButton();
            lblProperties = new Label();
            txtName = new TextBox();
            cmbColor = new ComboBox();
            lblWeaponHeader = new Label();
            cmbWeapon = new ComboBox();
            btnClone = new Button();
            tabFleetCommand = new TabPage();
            panelFleetLayout = new TableLayoutPanel();
            playerFleetContainer = new Panel();
            lblPlayerFleet = new Label();
            battleZoneContainer = new Panel();
            txtBattleLog = new TextBox();
            panelBattleButtons = new Panel();
            btnClearFleet = new Button();
            btnRepair = new Button();
            btnBattle = new Button();
            lblBattleLog = new Label();
            enemyFleetContainer = new Panel();
            lblEnemyFleet = new Label();
            tabControl.SuspendLayout();
            tabShipyard.SuspendLayout();
            panelShipyardRight.SuspendLayout();
            panelTopRow.SuspendLayout();
            previewContainer.SuspendLayout();
            infoContainer.SuspendLayout();
            panelLeft.SuspendLayout();
            leftGrid.SuspendLayout();
            radioPanel.SuspendLayout();
            tabFleetCommand.SuspendLayout();
            panelFleetLayout.SuspendLayout();
            playerFleetContainer.SuspendLayout();
            battleZoneContainer.SuspendLayout();
            panelBattleButtons.SuspendLayout();
            enemyFleetContainer.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabShipyard);
            tabControl.Controls.Add(tabFleetCommand);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 720);
            tabControl.TabIndex = 0;
            // 
            // tabShipyard
            // 
            tabShipyard.BackColor = Color.FromArgb(20, 20, 35);
            tabShipyard.Controls.Add(panelShipyardRight);
            tabShipyard.Controls.Add(panelLeft);
            tabShipyard.Location = new Point(4, 37);
            tabShipyard.Name = "tabShipyard";
            tabShipyard.Size = new Size(1192, 679);
            tabShipyard.TabIndex = 0;
            tabShipyard.Text = "Верфь";
            // 
            // panelShipyardRight
            // 
            panelShipyardRight.BackColor = Color.FromArgb(20, 20, 35);
            panelShipyardRight.Controls.Add(panelTopRow);
            panelShipyardRight.Dock = DockStyle.Fill;
            panelShipyardRight.Location = new Point(310, 0);
            panelShipyardRight.Name = "panelShipyardRight";
            panelShipyardRight.Padding = new Padding(10);
            panelShipyardRight.Size = new Size(882, 679);
            panelShipyardRight.TabIndex = 0;
            // 
            // panelTopRow
            // 
            panelTopRow.BackColor = Color.FromArgb(20, 20, 35);
            panelTopRow.ColumnCount = 2;
            panelTopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            panelTopRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            panelTopRow.Controls.Add(previewContainer, 0, 0);
            panelTopRow.Controls.Add(infoContainer, 1, 0);
            panelTopRow.Dock = DockStyle.Fill;
            panelTopRow.Location = new Point(10, 10);
            panelTopRow.Name = "panelTopRow";
            panelTopRow.RowCount = 1;
            panelTopRow.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            panelTopRow.Size = new Size(862, 659);
            panelTopRow.TabIndex = 0;
            // 
            // previewContainer
            // 
            previewContainer.Controls.Add(lblPreview);
            previewContainer.Dock = DockStyle.Fill;
            previewContainer.Location = new Point(3, 3);
            previewContainer.Name = "previewContainer";
            previewContainer.Padding = new Padding(5, 5, 10, 5);
            previewContainer.Size = new Size(381, 653);
            previewContainer.TabIndex = 0;
            // 
            // lblPreview
            // 
            lblPreview.Dock = DockStyle.Top;
            lblPreview.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblPreview.ForeColor = Color.White;
            lblPreview.Location = new Point(5, 5);
            lblPreview.Name = "lblPreview";
            lblPreview.Size = new Size(366, 32);
            lblPreview.TabIndex = 0;
            lblPreview.Text = "Предпросмотр:";
            // 
            // infoContainer
            // 
            infoContainer.Controls.Add(lblInfo);
            infoContainer.Dock = DockStyle.Fill;
            infoContainer.Location = new Point(390, 3);
            infoContainer.Name = "infoContainer";
            infoContainer.Padding = new Padding(10, 5, 5, 5);
            infoContainer.Size = new Size(469, 653);
            infoContainer.TabIndex = 1;
            // 
            // lblInfo
            // 
            lblInfo.Dock = DockStyle.Top;
            lblInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblInfo.ForeColor = Color.White;
            lblInfo.Location = new Point(10, 5);
            lblInfo.Name = "lblInfo";
            lblInfo.Size = new Size(454, 32);
            lblInfo.TabIndex = 0;
            lblInfo.Text = "Характеристики:";
            // 
            // panelLeft
            // 
            panelLeft.BackColor = Color.FromArgb(30, 30, 50);
            panelLeft.Controls.Add(leftGrid);
            panelLeft.Dock = DockStyle.Left;
            panelLeft.Location = new Point(0, 0);
            panelLeft.Name = "panelLeft";
            panelLeft.Padding = new Padding(10);
            panelLeft.Size = new Size(310, 679);
            panelLeft.TabIndex = 1;
            // 
            // leftGrid
            // 
            leftGrid.AutoSize = true;
            leftGrid.BackColor = Color.FromArgb(30, 30, 50);
            leftGrid.ColumnCount = 2;
            leftGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88F));
            leftGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftGrid.Controls.Add(lblSelectPrototype, 0, 0);
            leftGrid.Controls.Add(radioPanel, 0, 1);
            leftGrid.Controls.Add(lblProperties, 0, 2);
            leftGrid.Controls.Add(txtName, 1, 3);
            leftGrid.Controls.Add(cmbColor, 1, 7);
            leftGrid.Controls.Add(lblWeaponHeader, 0, 8);
            leftGrid.Controls.Add(cmbWeapon, 1, 9);
            leftGrid.Controls.Add(btnClone, 0, 11);
            leftGrid.Dock = DockStyle.Fill;
            leftGrid.Location = new Point(10, 10);
            leftGrid.Name = "leftGrid";
            leftGrid.RowCount = 15;
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 75F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            leftGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            leftGrid.Size = new Size(290, 659);
            leftGrid.TabIndex = 0;
            // 
            // lblSelectPrototype
            // 
            leftGrid.SetColumnSpan(lblSelectPrototype, 2);
            lblSelectPrototype.Dock = DockStyle.Fill;
            lblSelectPrototype.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblSelectPrototype.ForeColor = Color.White;
            lblSelectPrototype.Location = new Point(3, 0);
            lblSelectPrototype.Name = "lblSelectPrototype";
            lblSelectPrototype.Size = new Size(284, 40);
            lblSelectPrototype.TabIndex = 0;
            lblSelectPrototype.Text = "Выбор прототипа:";
            lblSelectPrototype.TextAlign = ContentAlignment.BottomLeft;
            // 
            // radioPanel
            // 
            radioPanel.BackColor = Color.FromArgb(30, 30, 50);
            leftGrid.SetColumnSpan(radioPanel, 2);
            radioPanel.Controls.Add(rbFighter);
            radioPanel.Controls.Add(rbCruiser);
            radioPanel.Controls.Add(rbBomber);
            radioPanel.Dock = DockStyle.Fill;
            radioPanel.Location = new Point(0, 40);
            radioPanel.Margin = new Padding(0);
            radioPanel.Name = "radioPanel";
            radioPanel.Size = new Size(290, 75);
            radioPanel.TabIndex = 1;
            // 
            // rbFighter
            // 
            rbFighter.AutoSize = true;
            rbFighter.Checked = true;
            rbFighter.Font = new Font("Segoe UI", 9F);
            rbFighter.ForeColor = Color.LightSkyBlue;
            rbFighter.Location = new Point(3, 3);
            rbFighter.Name = "rbFighter";
            rbFighter.Size = new Size(139, 29);
            rbFighter.TabIndex = 0;
            rbFighter.TabStop = true;
            rbFighter.Text = "Истребитель";
            rbFighter.CheckedChanged += OnPrototypeChanged;
            // 
            // rbCruiser
            // 
            rbCruiser.AutoSize = true;
            rbCruiser.Font = new Font("Segoe UI", 9F);
            rbCruiser.ForeColor = Color.Gold;
            rbCruiser.Location = new Point(148, 3);
            rbCruiser.Name = "rbCruiser";
            rbCruiser.Size = new Size(105, 29);
            rbCruiser.TabIndex = 1;
            rbCruiser.Text = "Крейсер";
            rbCruiser.CheckedChanged += OnPrototypeChanged;
            // 
            // rbBomber
            // 
            rbBomber.AutoSize = true;
            rbBomber.Font = new Font("Segoe UI", 9F);
            rbBomber.ForeColor = Color.Salmon;
            rbBomber.Location = new Point(3, 38);
            rbBomber.Name = "rbBomber";
            rbBomber.Size = new Size(187, 29);
            rbBomber.TabIndex = 2;
            rbBomber.Text = "Бомбардировщик";
            rbBomber.CheckedChanged += OnPrototypeChanged;
            // 
            // lblProperties
            // 
            leftGrid.SetColumnSpan(lblProperties, 2);
            lblProperties.Dock = DockStyle.Fill;
            lblProperties.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblProperties.ForeColor = Color.FromArgb(140, 140, 170);
            lblProperties.Location = new Point(3, 115);
            lblProperties.Name = "lblProperties";
            lblProperties.Size = new Size(284, 36);
            lblProperties.TabIndex = 2;
            lblProperties.Text = "── Свойства ──";
            lblProperties.TextAlign = ContentAlignment.BottomLeft;
            // 
            // txtName
            // 
            txtName.BackColor = Color.FromArgb(50, 50, 75);
            txtName.BorderStyle = BorderStyle.FixedSingle;
            txtName.Dock = DockStyle.Fill;
            txtName.Font = new Font("Segoe UI", 9.5F);
            txtName.ForeColor = Color.White;
            txtName.Location = new Point(91, 154);
            txtName.MaxLength = 20;
            txtName.Name = "txtName";
            txtName.Size = new Size(196, 33);
            txtName.TabIndex = 3;
            txtName.TextChanged += OnPropertyChanged;
            // 
            // cmbColor
            // 
            cmbColor.BackColor = Color.FromArgb(50, 50, 75);
            cmbColor.Dock = DockStyle.Fill;
            cmbColor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbColor.FlatStyle = FlatStyle.Flat;
            cmbColor.Font = new Font("Segoe UI", 9.5F);
            cmbColor.ForeColor = Color.White;
            cmbColor.Items.AddRange(new object[] { "LightSkyBlue", "Red", "Green", "Gold", "Orange", "Magenta", "Cyan", "Lime", "Salmon", "White" });
            cmbColor.Location = new Point(91, 322);
            cmbColor.Name = "cmbColor";
            cmbColor.Size = new Size(196, 33);
            cmbColor.TabIndex = 4;
            cmbColor.SelectedIndexChanged += OnPropertyChanged;
            // 
            // lblWeaponHeader
            // 
            leftGrid.SetColumnSpan(lblWeaponHeader, 2);
            lblWeaponHeader.Dock = DockStyle.Fill;
            lblWeaponHeader.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblWeaponHeader.ForeColor = Color.FromArgb(140, 140, 170);
            lblWeaponHeader.Location = new Point(3, 361);
            lblWeaponHeader.Name = "lblWeaponHeader";
            lblWeaponHeader.Size = new Size(284, 38);
            lblWeaponHeader.TabIndex = 5;
            lblWeaponHeader.Text = "── Вооружение ──";
            lblWeaponHeader.TextAlign = ContentAlignment.BottomLeft;
            // 
            // cmbWeapon
            // 
            cmbWeapon.BackColor = Color.FromArgb(50, 50, 75);
            cmbWeapon.Dock = DockStyle.Fill;
            cmbWeapon.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWeapon.FlatStyle = FlatStyle.Flat;
            cmbWeapon.Font = new Font("Segoe UI", 9.5F);
            cmbWeapon.ForeColor = Color.White;
            cmbWeapon.Items.AddRange(new object[] { "Laser Cannon", "Plasma Turret", "Missile Rack", "Torpedo Bay", "Ion Beam" });
            cmbWeapon.Location = new Point(91, 402);
            cmbWeapon.Name = "cmbWeapon";
            cmbWeapon.Size = new Size(196, 33);
            cmbWeapon.TabIndex = 6;
            cmbWeapon.SelectedIndexChanged += OnPropertyChanged;
            // 
            // btnClone
            // 
            btnClone.BackColor = Color.FromArgb(50, 120, 50);
            leftGrid.SetColumnSpan(btnClone, 2);
            btnClone.Dock = DockStyle.Fill;
            btnClone.FlatAppearance.BorderColor = Color.FromArgb(70, 160, 70);
            btnClone.FlatStyle = FlatStyle.Flat;
            btnClone.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnClone.ForeColor = Color.White;
            btnClone.Location = new Point(3, 486);
            btnClone.Name = "btnClone";
            btnClone.Size = new Size(284, 44);
            btnClone.TabIndex = 7;
            btnClone.Text = "Клонировать во флот";
            btnClone.UseVisualStyleBackColor = false;
            btnClone.Click += BtnClone_Click;
            // 
            // tabFleetCommand
            // 
            tabFleetCommand.BackColor = Color.FromArgb(20, 20, 35);
            tabFleetCommand.Controls.Add(panelFleetLayout);
            tabFleetCommand.Location = new Point(4, 37);
            tabFleetCommand.Name = "tabFleetCommand";
            tabFleetCommand.Padding = new Padding(10);
            tabFleetCommand.Size = new Size(1192, 679);
            tabFleetCommand.TabIndex = 1;
            tabFleetCommand.Text = "Командование флотом";
            // 
            // panelFleetLayout
            // 
            panelFleetLayout.BackColor = Color.FromArgb(20, 20, 35);
            panelFleetLayout.ColumnCount = 3;
            panelFleetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            panelFleetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            panelFleetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            panelFleetLayout.Controls.Add(playerFleetContainer, 0, 0);
            panelFleetLayout.Controls.Add(battleZoneContainer, 1, 0);
            panelFleetLayout.Controls.Add(enemyFleetContainer, 2, 0);
            panelFleetLayout.Dock = DockStyle.Fill;
            panelFleetLayout.Location = new Point(10, 10);
            panelFleetLayout.Name = "panelFleetLayout";
            panelFleetLayout.RowCount = 1;
            panelFleetLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            panelFleetLayout.Size = new Size(1172, 659);
            panelFleetLayout.TabIndex = 0;
            // 
            // playerFleetContainer
            // 
            playerFleetContainer.Controls.Add(lblPlayerFleet);
            playerFleetContainer.Dock = DockStyle.Fill;
            playerFleetContainer.Location = new Point(3, 3);
            playerFleetContainer.Name = "playerFleetContainer";
            playerFleetContainer.Padding = new Padding(0, 0, 5, 0);
            playerFleetContainer.Size = new Size(380, 653);
            playerFleetContainer.TabIndex = 0;
            // 
            // lblPlayerFleet
            // 
            lblPlayerFleet.Dock = DockStyle.Top;
            lblPlayerFleet.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblPlayerFleet.ForeColor = Color.White;
            lblPlayerFleet.Location = new Point(0, 0);
            lblPlayerFleet.Name = "lblPlayerFleet";
            lblPlayerFleet.Size = new Size(375, 38);
            lblPlayerFleet.TabIndex = 0;
            lblPlayerFleet.Text = "Флот игрока:";
            lblPlayerFleet.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // battleZoneContainer
            // 
            battleZoneContainer.Controls.Add(txtBattleLog);
            battleZoneContainer.Controls.Add(panelBattleButtons);
            battleZoneContainer.Controls.Add(lblBattleLog);
            battleZoneContainer.Dock = DockStyle.Fill;
            battleZoneContainer.Location = new Point(389, 3);
            battleZoneContainer.Name = "battleZoneContainer";
            battleZoneContainer.Padding = new Padding(5, 0, 5, 0);
            battleZoneContainer.Size = new Size(392, 653);
            battleZoneContainer.TabIndex = 1;
            // 
            // txtBattleLog
            // 
            txtBattleLog.BackColor = Color.FromArgb(15, 15, 30);
            txtBattleLog.BorderStyle = BorderStyle.FixedSingle;
            txtBattleLog.Dock = DockStyle.Fill;
            txtBattleLog.Font = new Font("Consolas", 9F);
            txtBattleLog.ForeColor = Color.LightGreen;
            txtBattleLog.Location = new Point(5, 38);
            txtBattleLog.Multiline = true;
            txtBattleLog.Name = "txtBattleLog";
            txtBattleLog.ReadOnly = true;
            txtBattleLog.ScrollBars = ScrollBars.Vertical;
            txtBattleLog.Size = new Size(382, 475);
            txtBattleLog.TabIndex = 0;
            txtBattleLog.Text = "Нажмите «В бой!» чтобы начать сражение.\r\n\r\nВаш флот сразится со случайно сгенерированным флотом врага.\r\n\r\nСкорость определяет порядок ходов.\r\nУрон сначала снижает щит, затем корпус.";
            // 
            // panelBattleButtons
            // 
            panelBattleButtons.BackColor = Color.FromArgb(20, 20, 35);
            panelBattleButtons.Controls.Add(btnClearFleet);
            panelBattleButtons.Controls.Add(btnRepair);
            panelBattleButtons.Controls.Add(btnBattle);
            panelBattleButtons.Dock = DockStyle.Bottom;
            panelBattleButtons.Location = new Point(5, 513);
            panelBattleButtons.Name = "panelBattleButtons";
            panelBattleButtons.Padding = new Padding(10, 5, 10, 5);
            panelBattleButtons.Size = new Size(382, 140);
            panelBattleButtons.TabIndex = 1;
            // 
            // btnClearFleet
            // 
            btnClearFleet.BackColor = Color.FromArgb(130, 45, 45);
            btnClearFleet.Dock = DockStyle.Top;
            btnClearFleet.FlatAppearance.BorderColor = Color.FromArgb(170, 60, 60);
            btnClearFleet.FlatStyle = FlatStyle.Flat;
            btnClearFleet.Font = new Font("Segoe UI", 9.5F);
            btnClearFleet.ForeColor = Color.White;
            btnClearFleet.Location = new Point(10, 77);
            btnClearFleet.Name = "btnClearFleet";
            btnClearFleet.Size = new Size(362, 32);
            btnClearFleet.TabIndex = 0;
            btnClearFleet.Text = "Очистить флот";
            btnClearFleet.UseVisualStyleBackColor = false;
            btnClearFleet.Click += BtnClearFleet_Click;
            // 
            // btnRepair
            // 
            btnRepair.BackColor = Color.FromArgb(40, 120, 140);
            btnRepair.Dock = DockStyle.Top;
            btnRepair.FlatAppearance.BorderColor = Color.FromArgb(60, 160, 180);
            btnRepair.FlatStyle = FlatStyle.Flat;
            btnRepair.Font = new Font("Segoe UI", 9.5F);
            btnRepair.ForeColor = Color.White;
            btnRepair.Location = new Point(10, 45);
            btnRepair.Name = "btnRepair";
            btnRepair.Size = new Size(362, 32);
            btnRepair.TabIndex = 1;
            btnRepair.Text = "🔧 Ремонт флота";
            btnRepair.UseVisualStyleBackColor = false;
            btnRepair.Click += BtnRepair_Click;
            // 
            // btnBattle
            // 
            btnBattle.BackColor = Color.FromArgb(180, 50, 50);
            btnBattle.Dock = DockStyle.Top;
            btnBattle.FlatAppearance.BorderColor = Color.FromArgb(220, 80, 80);
            btnBattle.FlatStyle = FlatStyle.Flat;
            btnBattle.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnBattle.ForeColor = Color.White;
            btnBattle.Location = new Point(10, 5);
            btnBattle.Name = "btnBattle";
            btnBattle.Size = new Size(362, 40);
            btnBattle.TabIndex = 2;
            btnBattle.Text = "⚔ В бой!";
            btnBattle.UseVisualStyleBackColor = false;
            btnBattle.Click += BtnBattle_Click;
            // 
            // lblBattleLog
            // 
            lblBattleLog.Dock = DockStyle.Top;
            lblBattleLog.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBattleLog.ForeColor = Color.White;
            lblBattleLog.Location = new Point(5, 0);
            lblBattleLog.Name = "lblBattleLog";
            lblBattleLog.Size = new Size(382, 38);
            lblBattleLog.TabIndex = 2;
            lblBattleLog.Text = "Журнал боя:";
            lblBattleLog.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // enemyFleetContainer
            // 
            enemyFleetContainer.Controls.Add(lblEnemyFleet);
            enemyFleetContainer.Dock = DockStyle.Fill;
            enemyFleetContainer.Location = new Point(787, 3);
            enemyFleetContainer.Name = "enemyFleetContainer";
            enemyFleetContainer.Padding = new Padding(5, 0, 0, 0);
            enemyFleetContainer.Size = new Size(382, 653);
            enemyFleetContainer.TabIndex = 2;
            // 
            // lblEnemyFleet
            // 
            lblEnemyFleet.Dock = DockStyle.Top;
            lblEnemyFleet.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblEnemyFleet.ForeColor = Color.FromArgb(255, 100, 100);
            lblEnemyFleet.Location = new Point(5, 0);
            lblEnemyFleet.Name = "lblEnemyFleet";
            lblEnemyFleet.Size = new Size(377, 38);
            lblEnemyFleet.TabIndex = 0;
            lblEnemyFleet.Text = "Флот врага:";
            lblEnemyFleet.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(20, 20, 35);
            ClientSize = new Size(1200, 720);
            Controls.Add(tabControl);
            DoubleBuffered = true;
            MinimumSize = new Size(1000, 650);
            Name = "Form1";
            Text = "Строитель Флота — Паттерн Прототип";
            tabControl.ResumeLayout(false);
            tabShipyard.ResumeLayout(false);
            panelShipyardRight.ResumeLayout(false);
            panelTopRow.ResumeLayout(false);
            previewContainer.ResumeLayout(false);
            infoContainer.ResumeLayout(false);
            panelLeft.ResumeLayout(false);
            panelLeft.PerformLayout();
            leftGrid.ResumeLayout(false);
            leftGrid.PerformLayout();
            radioPanel.ResumeLayout(false);
            radioPanel.PerformLayout();
            tabFleetCommand.ResumeLayout(false);
            panelFleetLayout.ResumeLayout(false);
            playerFleetContainer.ResumeLayout(false);
            battleZoneContainer.ResumeLayout(false);
            battleZoneContainer.PerformLayout();
            panelBattleButtons.ResumeLayout(false);
            enemyFleetContainer.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        // =====================================================================
        // ОБЪЯВЛЕНИЯ ПОЛЕЙ — все визуальные элементы формы
        // =====================================================================

        // --- Tab Control ---
        private TabControl tabControl;              // главный контейнер табов
        private TabPage tabShipyard;                // таб 1: строительство кораблей
        private TabPage tabFleetCommand;            // таб 2: управление флотом

        // --- Tab 1: Shipyard (левая панель) ---
        private Panel panelLeft;                    // левая панель (Dock.Left, 290px)
        private TableLayoutPanel leftGrid;          // сетка для элементов левой панели
        private FlowLayoutPanel radioPanel;         // панель для радиокнопок прототипа
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

        private Panel previewContainer;
        private Panel infoContainer;
        private Panel playerFleetContainer;
        private Panel battleZoneContainer;
        private Panel enemyFleetContainer;
    }
}
