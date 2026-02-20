// =============================================================================
// ShipRenderer.cs — Отрисовка кораблей с помощью GDI+ (Graphics Device Interface Plus)
// =============================================================================
// Этот файл содержит:
//   1. Структуру ShipMounts — точки крепления оружия на корпусе корабля
//   2. Статический класс ShipRenderer — методы отрисовки кораблей и оружия
//
// Каждый тип корабля имеет:
//   - Свою уникальную геометрическую форму (полигон из точек)
//   - Свои точки крепления оружия (нос, края корпуса, крылья)
//
// Оружие рисуется ПОВЕРХ корпуса корабля, используя точки крепления,
// чтобы визуальные элементы оружия всегда находились на корпусе,
// независимо от формы конкретного типа корабля.
//
// Используется GDI+ (System.Drawing) — встроенная графическая система Windows,
// работающая через полигоны, эллипсы, линии и заливки.
// Никакие внешние ресурсы (спрайты, текстуры) не требуются.
// =============================================================================

using Prototype.Models;

namespace Prototype
{
    // =========================================================================
    // ShipMounts — точки крепления оружия на корпусе корабля
    // =========================================================================
    // Каждый тип корабля (Fighter, Cruiser, Bomber) имеет уникальную форму,
    // поэтому оружие должно крепиться в разных местах.
    // Эта структура описывает ключевые точки корпуса:
    //
    //                     NoseX, NoseY (нос)
    //                        ↓
    //                        ◆
    //         BodyTop → ────/  \────
    //                  │            │
    //   WingTopY → ──◇──            ── ← WingX
    //                  │            │
    //         BodyBot → ────\  /────
    //                        ◆
    //                     TailX (хвост)
    //   WingBotY → ──◇──
    //
    // =========================================================================
    public struct ShipMounts
    {
        public int NoseX, NoseY;       // Координаты носа корабля (точка стрельбы лазеров/ионного луча)
        public int BodyTop, BodyBot;   // Верхний/нижний край корпуса в центре по X (для турелей и торпед)
        public int WingTopY, WingBotY; // Крайние Y-позиции крыльев/плавников (для ракет)
        public int WingX;              // X-координата области крепления крыльев
        public int TailX;              // Задняя точка корабля (хвост)
    }

    /// Статический класс для отрисовки кораблей с помощью GDI+.
    /// Рисует корпус корабля, элементы оружия и название.
    public static class ShipRenderer
    {
        /// Главный метод отрисовки — рисует корабль целиком.
        /// Вызывается из обработчика Paint панели предпросмотра.
        public static void DrawShip(Graphics g, Starship ship, Rectangle bounds)
        {
            // Включаем сглаживание для красивых линий (без «лесенки»)
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Вычисляем центр и размеры области рисования
            int cx = bounds.X + bounds.Width / 2;   // центр по Xя
            int cy = bounds.Y + bounds.Height / 2;  // центр по Y
            int w = bounds.Width - 20;               // ширина с отступами
            int h = bounds.Height - 20;              // высота с отступами

            // Создаём кисть и перо в цвете корабля
            using var brush = new SolidBrush(ship.ShipColor);  // заливка
            using var pen = new Pen(ship.ShipColor, 2);        // контур

            ShipMounts mounts;

            // Определяем тип корабля и рисуем соответствующую форму
            switch (ship.ShipType)
            {
                case "Fighter":
                    DrawFighter(g, brush, pen, cx, cy, w, h);
                    mounts = GetFighterMounts(cx, cy, w, h);
                    break;
                case "Cruiser":
                    DrawCruiser(g, brush, pen, cx, cy, w, h);
                    mounts = GetCruiserMounts(cx, cy, w, h);
                    break;
                case "Bomber":
                    DrawBomber(g, brush, pen, cx, cy, w, h);
                    mounts = GetBomberMounts(cx, cy, w, h);
                    break;
                default:
                    return; // Неизвестный тип — ничего не рисуем
            }

            // Рисуем оружие поверх корпуса, используя точки крепления
            DrawWeapon(g, ship.Weapon.Type, cx, cy, mounts);

            // Рисуем название корабля внизу по центру
            using var font = new Font("Segoe UI", 9, FontStyle.Bold);
            var nameSize = g.MeasureString(ship.Name, font);
            g.DrawString(ship.Name, font, Brushes.White,
                cx - nameSize.Width / 2, bounds.Bottom - 25);
        }

        // =====================================================================
        // ТОЧКИ КРЕПЛЕНИЯ ОРУЖИЯ — определяются для каждого типа корабля
        // =====================================================================

        /// Точки крепления для истребителя.
        /// Истребитель имеет треугольную форму — корпус сужается к носу,
        /// поэтому BodyTop/BodyBot в центре уже, чем в хвосте.
        /// bodyAtCenter = halfH * 2 / 3 учитывает сужение треугольника.
        private static ShipMounts GetFighterMounts(int cx, int cy, int w, int h)
        {
            int halfW = w / 2, halfH = h / 3;

            // Корпус истребителя — треугольник, сужающийся к носу.
            // В центре по X (cx) корпус уже, чем сзади.
            // Интерполяция по наклону треугольника: примерно 2/3 от максимальной высоты
            int bodyAtCenter = halfH * 2 / 3;

            return new ShipMounts
            {
                NoseX = cx + halfW,                    // Нос — правый край
                NoseY = cy,                            // Нос — по центру вертикали
                BodyTop = cy - bodyAtCenter,           // Верх корпуса (с учётом сужения)
                BodyBot = cy + bodyAtCenter,           // Низ корпуса (с учётом сужения)
                WingTopY = cy - halfH - halfH / 2,    // Верхнее крыло
                WingBotY = cy + halfH + halfH / 2,    // Нижнее крыло
                WingX = cx - halfW / 2,                // X-позиция крыльев (ближе к хвосту)
                TailX = cx - halfW / 2,                // Хвост
            };
        }

        /// Точки крепления для крейсера.
        /// Крейсер симметричный — шестиугольная форма, корпус одинаковой ширины.
        private static ShipMounts GetCruiserMounts(int cx, int cy, int w, int h)
        {
            int halfW = w / 2, halfH = h / 3;
            return new ShipMounts
            {
                NoseX = cx + halfW,          // Нос — правый край
                NoseY = cy,
                BodyTop = cy - halfH,        // Верх корпуса
                BodyBot = cy + halfH,        // Низ корпуса
                WingTopY = cy - halfH,       // У крейсера крылья совпадают с краями корпуса
                WingBotY = cy + halfH,
                WingX = cx - halfW / 4,      // Ближе к центру
                TailX = cx - halfW / 2,      // Хвост
            };
        }

        /// Точки крепления для бомбардировщика.
        /// Бомбардировщик — широкий и приземистый, с плавниками сверху/снизу.
        private static ShipMounts GetBomberMounts(int cx, int cy, int w, int h)
        {
            int halfW = w / 2, halfH = h / 3;
            return new ShipMounts
            {
                NoseX = cx + halfW / 2,                 // Нос короче, чем у других типов
                NoseY = cy,
                BodyTop = cy - halfH / 2,               // Корпус невысокий, но широкий
                BodyBot = cy + halfH / 2,
                WingTopY = cy - halfH - halfH / 3,      // Плавники выступают за корпус
                WingBotY = cy + halfH + halfH / 3,
                WingX = cx - halfW / 3,                  // Плавники ближе к хвосту
                TailX = cx - halfW / 2,                  // Хвост
            };
        }

        // =====================================================================
        // ОТРИСОВКА ОРУЖИЯ — каждый тип оружия имеет уникальный визуальный стиль
        // =====================================================================

        /// Диспетчер отрисовки оружия — вызывает метод для конкретного типа.
        private static void DrawWeapon(Graphics g, WeaponType type, int cx, int cy,
            ShipMounts m)
        {
            switch (type)
            {
                case WeaponType.LaserCannon:
                    DrawLaserCannon(g, m);
                    break;
                case WeaponType.PlasmaTurret:
                    DrawPlasmaTurret(g, cx, cy, m);
                    break;
                case WeaponType.MissileRack:
                    DrawMissileRack(g, m);
                    break;
                case WeaponType.TorpedoBay:
                    DrawTorpedoBay(g, cx, m);
                    break;
                case WeaponType.IonBeam:
                    DrawIonBeam(g, m);
                    break;
            }
        }

        /// Лазерная пушка — двойные красные лучи из носа корабля с огненными точками на концах.
        /// Визуал: две параллельные линии + светящиеся эллипсы (glow tips).
        private static void DrawLaserCannon(Graphics g, ShipMounts m)
        {
            using var laserPen = new Pen(Color.FromArgb(200, Color.Red), 2);
            int nx = m.NoseX;  // Начало лучей — нос корабля
            int ny = m.NoseY;

            // Два параллельных луча: верхний (ny - 4) и нижний (ny + 4)
            g.DrawLine(laserPen, nx - 2, ny - 4, nx + 20, ny - 4);
            g.DrawLine(laserPen, nx - 2, ny + 4, nx + 20, ny + 4);

            // Светящиеся точки на концах лучей (полупрозрачные эллипсы)
            using var glowBrush = new SolidBrush(Color.FromArgb(120, Color.OrangeRed));
            g.FillEllipse(glowBrush, nx + 17, ny - 7, 8, 6);
            g.FillEllipse(glowBrush, nx + 17, ny + 1, 8, 6);
        }

        /// Плазменная турель — фиолетовые энергетические сферы на верхнем и нижнем краях корпуса.
        /// Каждая сфера: полупрозрачный фиолетовый круг + белое ядро + контур.
        private static void DrawPlasmaTurret(Graphics g, int cx, int cy, ShipMounts m)
        {
            using var orbBrush1 = new SolidBrush(Color.FromArgb(160, Color.MediumPurple));  // основной цвет сферы
            using var orbBrush2 = new SolidBrush(Color.FromArgb(100, Color.White));          // яркое ядро
            using var orbPen = new Pen(Color.FromArgb(200, Color.MediumPurple), 1);          // контур

            int orbSize = 14;  // размер каждой сферы

            // Верхняя турель — крепится к верхнему краю корпуса (BodyTop)
            int topX = cx - orbSize / 2;
            int topY = m.BodyTop - orbSize + 2;
            g.FillEllipse(orbBrush1, topX, topY, orbSize, orbSize);              // тело сферы
            g.FillEllipse(orbBrush2, topX + 3, topY + 3, orbSize - 6, orbSize - 6);  // ядро
            g.DrawEllipse(orbPen, topX, topY, orbSize, orbSize);                  // контур

            // Нижняя турель — крепится к нижнему краю корпуса (BodyBot)
            int botY = m.BodyBot - 2;
            g.FillEllipse(orbBrush1, topX, botY, orbSize, orbSize);
            g.FillEllipse(orbBrush2, topX + 3, botY + 3, orbSize - 6, orbSize - 6);
            g.DrawEllipse(orbPen, topX, botY, orbSize, orbSize);
        }

        /// Ракетная установка — четыре оранжевых ракеты (треугольники) на позициях крыльев.
        /// Ракеты распределены между краем корпуса и кончиком крыла.
        private static void DrawMissileRack(Graphics g, ShipMounts m)
        {
            using var missileBrush = new SolidBrush(Color.FromArgb(220, Color.OrangeRed));
            using var missilePen = new Pen(Color.FromArgb(180, Color.DarkRed), 1);

            int mw = 10, mh = 5;  // размеры каждой ракеты (ширина × высота)
            int mx = m.WingX;     // X-позиция — область крыльев

            // Вычисляем промежуточные Y-позиции между крылом и краем корпуса
            int topMid = (m.BodyTop + m.WingTopY) / 2;  // между верхом корпуса и верхним крылом
            int botMid = (m.BodyBot + m.WingBotY) / 2;  // между низом корпуса и нижним крылом

            // 4 позиции: верхнее крыло, между верхом, между низом, нижнее крыло
            int[] yPositions = { m.WingTopY + 2, topMid, botMid, m.WingBotY - 2 };

            foreach (int yPos in yPositions)
            {
                int my = yPos - mh / 2;
                // Каждая ракета — маленький треугольник, «летящий» вправо
                Point[] missile = {
                    new(mx + mw, my + mh / 2),  // остриё (правая точка)
                    new(mx, my),                  // верхний угол хвоста
                    new(mx, my + mh),             // нижний угол хвоста
                };
                g.FillPolygon(missileBrush, missile);
                g.DrawPolygon(missilePen, missile);
            }
        }

        /// Торпедный отсек — бирюзовые овальные торпеды над и под корпусом.
        /// С тонкими линиями-коннекторами от торпед к корпусу (подвеска).
        private static void DrawTorpedoBay(Graphics g, int cx, ShipMounts m)
        {
            using var torpBrush = new SolidBrush(Color.FromArgb(200, Color.DarkCyan));
            using var torpPen = new Pen(Color.FromArgb(150, Color.Cyan), 1);

            int tw = 18, th = 6;          // размеры торпеды (овал)
            int baseX = cx - tw / 2;      // центрируем по X

            // Верхняя торпеда — чуть выше верхнего края корпуса
            g.FillEllipse(torpBrush, baseX, m.BodyTop - th - 2, tw, th);
            g.DrawEllipse(torpPen, baseX, m.BodyTop - th - 2, tw, th);

            // Нижняя торпеда — чуть ниже нижнего края корпуса
            g.FillEllipse(torpBrush, baseX, m.BodyBot + 2, tw, th);
            g.DrawEllipse(torpPen, baseX, m.BodyBot + 2, tw, th);

            // Тонкие линии-коннекторы (подвеска торпед к корпусу)
            using var bayPen = new Pen(Color.FromArgb(100, Color.Cyan), 1);
            g.DrawLine(bayPen, baseX + tw / 2, m.BodyTop - 2, baseX + tw / 2, m.BodyTop + 4);
            g.DrawLine(bayPen, baseX + tw / 2, m.BodyBot - 4, baseX + tw / 2, m.BodyBot + 2);
        }

        /// Ионный луч — расширяющийся конус бирюзового цвета из носа корабля.
        /// Визуал: двухслойный конус (яркий + бледный) + кольцо эмиттера + центральный луч.
        private static void DrawIonBeam(Graphics g, ShipMounts m)
        {
            int nx = m.NoseX;  // Начало луча — нос корабля
            int ny = m.NoseY;

            // Внутренний конус (более яркий, узкий)
            using var beamBrush1 = new SolidBrush(Color.FromArgb(50, Color.Cyan));
            Point[] cone = {
                new(nx, ny),                // вершина конуса (нос корабля)
                new(nx + 35, ny - 15),      // верхний край расширения
                new(nx + 35, ny + 15),      // нижний край расширения
            };
            g.FillPolygon(beamBrush1, cone);

            // Внешний конус (бледнее, шире)
            using var beamBrush2 = new SolidBrush(Color.FromArgb(25, Color.Cyan));
            Point[] outerCone = {
                new(nx, ny),
                new(nx + 45, ny - 22),
                new(nx + 45, ny + 22),
            };
            g.FillPolygon(beamBrush2, outerCone);

            // Кольцо эмиттера — маленький овал на носу
            using var emitterPen = new Pen(Color.FromArgb(220, Color.Cyan), 2);
            g.DrawEllipse(emitterPen, nx - 5, ny - 6, 10, 12);

            // Центральный луч — тонкая белая линия по оси конуса
            using var corePen = new Pen(Color.FromArgb(180, Color.White), 1);
            g.DrawLine(corePen, nx + 3, ny, nx + 30, ny);
        }

        // =====================================================================
        // ОТРИСОВКА КОРПУСОВ КОРАБЛЕЙ — каждый тип имеет уникальную форму
        // =====================================================================

        /// Истребитель — треугольная стреловидная форма с двумя крыльями.
        ///
        /// Основной корпус — ромбовидный треугольник (4 точки):
        ///   Нос (правый край) → верхний угол → вогнутость хвоста → нижний угол
        ///
        /// Крылья — два четырёхугольника, отходящие от задней части:
        ///   Верхнее крыло: от середины корпуса вверх и назад
        ///   Нижнее крыло: от середины корпуса вниз и назад
        ///
        /// Двигатель — маленький бирюзовый эллипс в хвосте (glow/свечение).
        private static void DrawFighter(Graphics g, Brush brush, Pen pen,
            int cx, int cy, int w, int h)
        {
            int halfW = w / 2;
            int halfH = h / 3;

            // Основной корпус — заострённый ромб
            Point[] body = {
                new(cx + halfW, cy),               // нос (правый край)
                new(cx - halfW / 2, cy - halfH),   // верхний задний угол
                new(cx - halfW / 3, cy),            // вогнутость хвоста (центр)
                new(cx - halfW / 2, cy + halfH),   // нижний задний угол
            };
            g.FillPolygon(brush, body);   // заливка
            g.DrawPolygon(pen, body);     // контур

            // Верхнее крыло — четырёхугольник, отходящий вверх-назад от корпуса
            Point[] topWing = {
                new(cx - halfW / 4, cy - halfH / 2),              // от середины корпуса
                new(cx - halfW / 2, cy - halfH - halfH / 2),      // кончик крыла (вверх-назад)
                new(cx - halfW / 2 - halfW / 4, cy - halfH),      // задний край крыла
                new(cx - halfW / 3, cy - halfH / 2 - 5),          // обратно к корпусу
            };
            g.FillPolygon(brush, topWing);

            // Нижнее крыло — зеркальное отражение верхнего
            Point[] bottomWing = {
                new(cx - halfW / 4, cy + halfH / 2),
                new(cx - halfW / 2, cy + halfH + halfH / 2),
                new(cx - halfW / 2 - halfW / 4, cy + halfH),
                new(cx - halfW / 3, cy + halfH / 2 + 5),
            };
            g.FillPolygon(brush, bottomWing);

            // Свечение двигателя — бирюзовый эллипс в хвосте
            using var glowBrush = new SolidBrush(Color.FromArgb(150, Color.Cyan));
            g.FillEllipse(glowBrush, cx - halfW / 2 - 8, cy - 4, 10, 8);
        }

        /// Крейсер — удлинённый шестиугольник с секцией капитанского мостика.
        ///
        /// Корпус — шестиугольник (6 точек):
        ///   Нос → верхний правый → верхний левый → хвост → нижний левый → нижний правый
        ///
        /// Мостик — ромбовидная секция в передней части (полупрозрачная белая).
        /// Двигатель — оранжевый эллипс в хвосте.
        private static void DrawCruiser(Graphics g, Brush brush, Pen pen,
            int cx, int cy, int w, int h)
        {
            int halfW = w / 2;
            int halfH = h / 3;

            // Основной корпус — шестиугольник
            Point[] body = {
                new(cx + halfW, cy),                // нос
                new(cx + halfW / 3, cy - halfH),    // верхний правый
                new(cx - halfW / 3, cy - halfH),    // верхний левый
                new(cx - halfW / 2, cy),             // хвост
                new(cx - halfW / 3, cy + halfH),    // нижний левый
                new(cx + halfW / 3, cy + halfH),    // нижний правый
            };
            g.FillPolygon(brush, body);
            g.DrawPolygon(pen, body);

            // Секция мостика — ромб в передней части (полупрозрачный)
            using var darkBrush = new SolidBrush(Color.FromArgb(100, Color.White));
            Point[] bridge = {
                new(cx + halfW / 4, cy - halfH / 3),   // верх мостика
                new(cx + halfW / 2, cy),                 // правый край (ближе к носу)
                new(cx + halfW / 4, cy + halfH / 3),   // низ мостика
                new(cx, cy),                              // левый край
            };
            g.FillPolygon(darkBrush, bridge);

            // Свечение двигателя — оранжевый эллипс в хвосте
            using var glowBrush = new SolidBrush(Color.FromArgb(150, Color.Orange));
            g.FillEllipse(glowBrush, cx - halfW / 2 - 6, cy - 6, 10, 12);
        }

        /// Бомбардировщик — широкий прямоугольный корпус с носовым клином и плавниками.
        ///
        /// Корпус — прямоугольник (широкий и невысокий).
        /// Нос — треугольный клин, выступающий вперёд.
        /// Плавники — два треугольника сверху и снизу хвостовой части.
        /// Двигатели — два красных эллипса (верхний и нижний) в хвосте.
        private static void DrawBomber(Graphics g, Brush brush, Pen pen,
            int cx, int cy, int w, int h)
        {
            int halfW = w / 2;
            int halfH = h / 3;

            // Основной корпус — прямоугольник
            Point[] body = {
                new(cx + halfW / 3, cy - halfH / 3),    // правый верхний угол
                new(cx + halfW / 3, cy + halfH / 3),    // правый нижний угол
                new(cx - halfW / 2, cy + halfH / 2),    // левый нижний (чуть шире)
                new(cx - halfW / 2, cy - halfH / 2),    // левый верхний
            };
            g.FillPolygon(brush, body);
            g.DrawPolygon(pen, body);

            // Носовой клин — треугольник, выступающий вправо из корпуса
            Point[] nose = {
                new(cx + halfW / 3, cy - halfH / 4),    // верхний край
                new(cx + halfW / 2, cy),                  // остриё
                new(cx + halfW / 3, cy + halfH / 4),    // нижний край
            };
            g.FillPolygon(brush, nose);
            g.DrawPolygon(pen, nose);

            // Верхний плавник — треугольник, выступающий вверх из задней части
            Point[] topFin = {
                new(cx - halfW / 4, cy - halfH / 2),
                new(cx - halfW / 3, cy - halfH - halfH / 3),   // кончик плавника
                new(cx - halfW / 2, cy - halfH / 2),
            };
            g.FillPolygon(brush, topFin);

            // Нижний плавник — зеркальное отражение верхнего
            Point[] bottomFin = {
                new(cx - halfW / 4, cy + halfH / 2),
                new(cx - halfW / 3, cy + halfH + halfH / 3),   // кончик плавника
                new(cx - halfW / 2, cy + halfH / 2),
            };
            g.FillPolygon(brush, bottomFin);

            // Свечение двигателей — два красных эллипса (верхний и нижний)
            using var glowBrush = new SolidBrush(Color.FromArgb(150, Color.Red));
            g.FillEllipse(glowBrush, cx - halfW / 2 - 6, cy - halfH / 3, 8, 8);       // верхний
            g.FillEllipse(glowBrush, cx - halfW / 2 - 6, cy + halfH / 3 - 8, 8, 8);  // нижний
        }
    }
}
