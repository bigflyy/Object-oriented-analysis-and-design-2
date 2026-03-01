using Prototype.Models;

namespace Prototype.UI
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

        /// Инициализирует элементы панели флота.
        private void InitializeControls()
        {
            // Настройка самой панели
            this.BackColor = Color.FromArgb(20, 20, 35);
            this.Padding = new Padding(5, 8, 5, 5);

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
            _emptyLabel.Text = "Флот пуст";
            _emptyLabel.ForeColor = Color.FromArgb(120, 120, 140);
            _emptyLabel.Font = new Font("Segoe UI", 11f, FontStyle.Italic);
            _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
            _emptyLabel.Dock = DockStyle.Fill;
            _emptyLabel.Visible = true;  // По умолчанию видна

            // Добавляем элементы
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
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card && card.Tag == ship)
                {
                    await card.FlashDamage();
                    return true;
                }
            }
            return false;
        }

        /// Подсвечивает карточку указанного корабля синим (промах/уклонение).
        public async Task<bool> FlashMissShip(Starship ship)
        {
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card && card.Tag == ship)
                {
                    await card.FlashMiss();
                    return true;
                }
            }
            return false;
        }

        /// Включает режим выбора цели: подсвечивает все карточки зелёным,
        /// ждёт клика игрока и возвращает выбранный корабль.
        public Task<Starship> WaitForSelection()
        {
            var tcs = new TaskCompletionSource<Starship>();

            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card)
                {
                    card.SetSelectable(true);
                    card.OnSelected += OnCardSelected;
                }
            }

            void OnCardSelected(Starship selected)
            {
                DisableSelection(OnCardSelected);
                tcs.TrySetResult(selected);
            }

            return tcs.Task;
        }

        /// Выключает режим выбора и отписывает обработчик.
        private void DisableSelection(Action<Starship> handler)
        {
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card)
                {
                    card.SetSelectable(false);
                    card.OnSelected -= handler;
                }
            }
        }

        /// Подсвечивает карточку атакующего корабля жёлтым.
        public void HighlightAttacker(Starship ship)
        {
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card && card.Tag == ship)
                    card.SetAttacker(true);
            }
        }

        /// Снимает подсветку атакующего.
        public void ClearAttacker(Starship ship)
        {
            foreach (Control control in _cardContainer.Controls)
            {
                if (control is ShipCard card && card.Tag == ship)
                    card.SetAttacker(false);
            }
        }
    }
}