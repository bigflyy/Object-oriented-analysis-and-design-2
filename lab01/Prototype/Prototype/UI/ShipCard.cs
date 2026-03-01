// ShipCard.cs — Визуальная карточка корабля для отображения во флоте
// Пользовательский элемент (UserControl) для отображения одного корабля
// с миниатюрным превью, названием и прогресс-барами HP/Shield.
// Используется в FleetPanel для визуального представления флота.

using Prototype.Models;

namespace Prototype.UI
{
    /// Карточка корабля — компактное визуальное представление одного корабля.
    /// Показывает миниатюрный предпросмотр, имя, тип и прогресс-бары здоровья/щита.
    public class ShipCard : UserControl
    {
        private Starship _ship;           // Корабль для отображения
        private Panel _previewPanel = null!;        // Панель для GDI+ рисования мини-корабля
        private Label _nameLabel = null!;           // Метка с именем и типом корабля
        private Panel _hullBarBackground = null!;   // Фон для бара корпуса
        private Panel _hullBarFill = null!;         // Заполнение бара корпуса
        private Panel _shieldBarBackground = null!; // Фон для бара щита
        private Panel _shieldBarFill = null!;       // Заполнение бара щита
        private Label _statsLabel = null!;          // Дополнительная информация (скорость, урон)
        private bool _isHighlighted = false;        // Флаг подсветки при получении урона
        private Color _highlightColor = Color.FromArgb(255, 50, 50); // Цвет рамки подсветки
        private bool _isSelectable = false;         // Флаг: можно выбрать как цель

        /// Событие выбора карточки как цели (вызывается при клике в режиме выбора).
        public event Action<Starship> OnSelected;

        public ShipCard()
        {
            InitializeControls();
        }

        /// Устанавливает корабль для отображения на карточке.
        /// Обновляет все визуальные элементы согласно данным корабля.
        public void SetShip(Starship ship)
        {
            _ship = ship;
            UpdateDisplay();
        }

        /// Публичный метод для обновления отображения (вызывается из FleetPanel).
        public void UpdateCardDisplay()
        {
            UpdateDisplay();
        }

        /// Инициализирует все элементы карточки.
        private void InitializeControls()
        {
            // Настройка самой карточки (увеличенный размер для лучшей читаемости)
            this.Size = new Size(230, 208);
            this.BackColor = Color.FromArgb(25, 25, 40);
            this.BorderStyle = BorderStyle.None;  // Убираем стандартную границу (рисуем свою)
            this.Padding = new Padding(10);  // Увеличиваем отступ для красной рамки
            this.Paint += Card_Paint;  // Обработчик для рисования рамки

            // Панель предпросмотра корабля (миниатюра)
            _previewPanel = new Panel();
            _previewPanel.Location = new Point(10, 10);
            _previewPanel.Size = new Size(210, 85);
            _previewPanel.BackColor = Color.FromArgb(10, 10, 20);
            _previewPanel.BorderStyle = BorderStyle.FixedSingle;
            _previewPanel.Paint += PreviewPanel_Paint;

            // Метка с именем и типом корабля (увеличена высота для длинных имён)
            _nameLabel = new Label();
            _nameLabel.Location = new Point(10, 99);
            _nameLabel.Size = new Size(210, 24);
            _nameLabel.ForeColor = Color.White;
            _nameLabel.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            _nameLabel.Text = "Корабль";
            _nameLabel.TextAlign = ContentAlignment.MiddleCenter;

            // Бар корпуса (Hull) — фон
            _hullBarBackground = new Panel();
            _hullBarBackground.Location = new Point(10, 127);
            _hullBarBackground.Size = new Size(210, 14);
            _hullBarBackground.BackColor = Color.FromArgb(40, 40, 50);
            _hullBarBackground.BorderStyle = BorderStyle.FixedSingle;

            // Бар корпуса — заполнение (зелёный)
            _hullBarFill = new Panel();
            _hullBarFill.Location = new Point(0, 0);
            _hullBarFill.Size = new Size(210, 14);
            _hullBarFill.BackColor = Color.FromArgb(100, 200, 100);
            _hullBarBackground.Controls.Add(_hullBarFill);

            // Бар щита (Shield) — фон
            _shieldBarBackground = new Panel();
            _shieldBarBackground.Location = new Point(10, 145);
            _shieldBarBackground.Size = new Size(210, 14);
            _shieldBarBackground.BackColor = Color.FromArgb(40, 40, 50);
            _shieldBarBackground.BorderStyle = BorderStyle.FixedSingle;

            // Бар щита — заполнение (голубой)
            _shieldBarFill = new Panel();
            _shieldBarFill.Location = new Point(0, 0);
            _shieldBarFill.Size = new Size(210, 14);
            _shieldBarFill.BackColor = Color.FromArgb(100, 150, 255);
            _shieldBarBackground.Controls.Add(_shieldBarFill);

            // Дополнительная информация (скорость, урон) — увеличена высота
            _statsLabel = new Label();
            _statsLabel.Location = new Point(10, 163);
            _statsLabel.Size = new Size(210, 38);
            _statsLabel.ForeColor = Color.FromArgb(160, 160, 180);
            _statsLabel.Font = new Font("Consolas", 7.5f);
            _statsLabel.Text = "Скр:180 Урн:25";
            _statsLabel.TextAlign = ContentAlignment.TopCenter;
            _statsLabel.AutoSize = false;

            // Добавляем все элементы на карточку
            this.Controls.Add(_previewPanel);
            this.Controls.Add(_nameLabel);
            this.Controls.Add(_hullBarBackground);
            this.Controls.Add(_shieldBarBackground);
            this.Controls.Add(_statsLabel);

            // Клик по карточке — выбор цели (работает только в режиме _isSelectable)
            this.Click += (s, e) =>
            {
                if (_isSelectable && _ship != null)
                    OnSelected?.Invoke(_ship);
            };
        }

        /// Обновляет отображение карточки согласно данным корабля.
        private void UpdateDisplay()
        {
            if (_ship == null) return;

            // Обновляем название
            _nameLabel.Text = $"{_ship.ShipType} \"{_ship.Name}\"";

            // Обновляем бар корпуса
            float hullRatio = _ship.MaxHull > 0 ? (float)_ship.HullStrength / _ship.MaxHull : 0f;
            hullRatio = Math.Max(0f, Math.Min(1f, hullRatio));
            _hullBarFill.Width = (int)(208 * hullRatio);  // 210 - 2 для границ

            // Цвет бара корпуса зависит от процента здоровья
            if (hullRatio > 0.6f)
                _hullBarFill.BackColor = Color.FromArgb(100, 200, 100);  // Зелёный
            else if (hullRatio > 0.3f)
                _hullBarFill.BackColor = Color.FromArgb(230, 180, 50);   // Жёлтый
            else
                _hullBarFill.BackColor = Color.FromArgb(220, 80, 80);    // Красный

            // Обновляем бар щита
            float shieldRatio = _ship.MaxShield > 0 ? (float)_ship.ShieldLevel / _ship.MaxShield : 0f;
            shieldRatio = Math.Max(0f, Math.Min(1f, shieldRatio));
            _shieldBarFill.Width = (int)(208 * shieldRatio);  // 210 - 2 для границ

            // Обновляем дополнительную информацию
            _statsLabel.Text = $"КР:{_ship.HullStrength}/{_ship.MaxHull}  " +
                               $"Щт:{_ship.ShieldLevel}/{_ship.MaxShield}\n" +
                               $"Скр:{_ship.Speed}  Урн:{_ship.Weapon.Damage}";

            // Перерисовываем предпросмотр
            _previewPanel.Invalidate();
        }

        /// Обработчик отрисовки панели предпросмотра корабля.
        /// Рисует миниатюрную версию корабля с помощью ShipRenderer.
        private void PreviewPanel_Paint(object sender, PaintEventArgs e)
        {
            if (_ship == null) return;

            // Рисуем миниатюрный корабль
            var bounds = new Rectangle(0, 0, _previewPanel.Width, _previewPanel.Height);
            ShipRenderer.DrawShip(e.Graphics, _ship, bounds);
        }

        /// Обработчик отрисовки карточки — рисует красную рамку при получении урона.
        private void Card_Paint(object sender, PaintEventArgs e)
        {
            if (_isHighlighted)
            {
                // Рисуем толстую рамку вокруг карточки (красная = урон, синяя = промах)
                using var pen = new Pen(_highlightColor, 4);
                e.Graphics.DrawRectangle(pen, 2, 2, this.Width - 4, this.Height - 4);
            }
            else if (_isSelectable)
            {
                // Зелёная рамка — можно выбрать как цель
                using var pen = new Pen(Color.FromArgb(80, 220, 80), 2);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                e.Graphics.DrawRectangle(pen, 1, 1, this.Width - 2, this.Height - 2);
            }
            else
            {
                // Рисуем тонкую серую рамку (обычное состояние)
                using var pen = new Pen(Color.FromArgb(60, 60, 80), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            }
        }

        /// Подсвечивает карточку красным на короткое время (эффект получения урона).
        public async Task FlashDamage()
        {
            _isHighlighted = true;
            _highlightColor = Color.FromArgb(255, 50, 50);  // Красная рамка
            this.BackColor = Color.FromArgb(80, 20, 20);    // Тёмно-красный фон
            this.Invalidate();
            await Task.Delay(600);
            _isHighlighted = false;
            this.BackColor = Color.FromArgb(25, 25, 40);
            this.Invalidate();
        }

        /// Подсвечивает карточку синим на короткое время (промах/уклонение).
        public async Task FlashMiss()
        {
            _isHighlighted = true;
            _highlightColor = Color.FromArgb(100, 150, 255); // Синяя рамка
            this.BackColor = Color.FromArgb(20, 30, 70);     // Тёмно-синий фон
            this.Invalidate();
            await Task.Delay(600);
            _isHighlighted = false;
            this.BackColor = Color.FromArgb(25, 25, 40);
            this.Invalidate();
        }

        /// Подсвечивает карточку жёлтым (текущий атакующий корабль).
        public void SetAttacker(bool active)
        {
            _isHighlighted = active;
            if (active)
            {
                _highlightColor = Color.FromArgb(255, 220, 50); // Жёлтая рамка
                this.BackColor = Color.FromArgb(50, 45, 15);    // Тёмно-жёлтый фон
            }
            else
            {
                this.BackColor = Color.FromArgb(25, 25, 40);
            }
            this.Invalidate();
        }

        /// Включает/выключает режим выбора цели (зелёная рамка + курсор-рука).
        public void SetSelectable(bool selectable)
        {
            _isSelectable = selectable;
            this.Cursor = selectable ? Cursors.Hand : Cursors.Default;

            // Дочерние контролы перехватывают клики — нужно пробросить их
            // Рекурсивно обходим ВСЕ вложенные контролы (включая bar fills внутри bar backgrounds)
            SetClickRecursive(this, selectable);

            this.Invalidate();
        }

        private void SetClickRecursive(Control parent, bool selectable)
        {
            foreach (Control child in parent.Controls)
            {
                child.Cursor = selectable ? Cursors.Hand : Cursors.Default;
                if (selectable)
                    child.Click += ChildClick;
                else
                    child.Click -= ChildClick;
                SetClickRecursive(child, selectable);
            }
        }

        private void ChildClick(object sender, EventArgs e)
        {
            if (_isSelectable && _ship != null)
                OnSelected?.Invoke(_ship);
        }
    }
}
