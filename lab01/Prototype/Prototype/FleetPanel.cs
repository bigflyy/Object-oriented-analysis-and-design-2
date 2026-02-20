// =============================================================================
// FleetPanel.cs — Панель для отображения флота кораблей
// =============================================================================
// Пользовательский контрол (Panel) для отображения списка кораблей
// в виде вертикального ряда визуальных карточек (ShipCard).
// Поддерживает прокрутку при большом количестве кораблей.
// Используется для отображения как флота игрока, так и вражеского флота.
// =============================================================================

using Prototype.Models;

namespace Prototype
{
    /// Панель флота — контейнер для визуального отображения списка кораблей.
    /// Автоматически создаёт карточки (ShipCard) для каждого корабля
    /// и располагает их вертикально с возможностью прокрутки.
    public class FleetPanel : Panel
    {
        private FlowLayoutPanel _cardContainer = null!;  // Контейнер для карточек с авто-прокруткой
        private Label _emptyLabel = null!;               // Метка "Флот пуст" когда кораблей нет

        public FleetPanel()
        {
            InitializeControls();
        }

        /// Инициализирует контролы панели флота.
        private void InitializeControls()
        {
            // Настройка самой панели
            this.BackColor = Color.FromArgb(20, 20, 35);
            this.Padding = new Padding(5);

            // Контейнер для карточек кораблей (вертикальный список с прокруткой)
            _cardContainer = new FlowLayoutPanel();
            _cardContainer.Dock = DockStyle.Fill;
            _cardContainer.FlowDirection = FlowDirection.TopDown;  // Сверху вниз
            _cardContainer.WrapContents = false;                   // Не переносить на новую строку
            _cardContainer.AutoScroll = true;                      // Включить прокрутку
            _cardContainer.BackColor = Color.FromArgb(20, 20, 35);
            _cardContainer.Padding = new Padding(5);

            // Метка для пустого флота
            _emptyLabel = new Label();
            _emptyLabel.Text = "Флот пуст\nEmpty Fleet";
            _emptyLabel.ForeColor = Color.FromArgb(120, 120, 140);
            _emptyLabel.Font = new Font("Segoe UI", 11f, FontStyle.Italic);
            _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            _emptyLabel.Dock = DockStyle.Fill;
            _emptyLabel.Visible = true;  // По умолчанию видна

            // Добавляем контролы
            this.Controls.Add(_cardContainer);
            this.Controls.Add(_emptyLabel);

            // Метка должна быть поверх контейнера (показывается когда флот пуст)
            _emptyLabel.BringToFront();
        }

        /// Устанавливает список кораблей для отображения.
        /// Очищает старые карточки и создаёт новые для каждого корабля.
        public void SetFleet(List<Starship> fleet)
        {
            // Очищаем старые карточки
            _cardContainer.Controls.Clear();

            // Если флот пуст, показываем метку
            if (fleet == null || fleet.Count == 0)
            {
                _emptyLabel.Visible = true;
                return;
            }

            // Скрываем метку "пусто"
            _emptyLabel.Visible = false;

            // Создаём карточку для каждого корабля
            foreach (var ship in fleet)
            {
                var shipCard = new ShipCard();
                shipCard.SetShip(ship);
                shipCard.Tag = ship;  // Сохраняем ссылку на корабль для поиска карточки
                shipCard.Margin = new Padding(0, 0, 0, 10);  // Увеличенный отступ между карточками
                _cardContainer.Controls.Add(shipCard);
            }
        }

        /// Обновляет отображение всех карточек БЕЗ пересоздания.
        /// Вызывает UpdateDisplay() на каждой карточке для обновления HP/Shield.
        /// Полезно если характеристики кораблей изменились (например, после боя).
        public void RefreshCards()
        {
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card)
                {
                    card.UpdateCardDisplay();  // Обновить данные карточки
                }
            }
        }

        /// Очищает флот (удаляет все карточки).
        public void Clear()
        {
            _cardContainer.Controls.Clear();
            _emptyLabel.Visible = true;
        }

        /// Подсвечивает карточку указанного корабля красным (эффект получения урона).
        public async Task<bool> FlashShip(Starship ship)
        {
            // Ищем карточку корабля среди дочерних контролов
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card && card.Tag == ship)
                {
                    await card.FlashDamage();
                    return true;  // Нашли и подсветили
                }
            }
            return false;  // Не нашли карточку
        }
    }
}
