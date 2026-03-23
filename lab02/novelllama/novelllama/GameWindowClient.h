#pragma once
#include <SFML/Graphics.hpp>
#include <string>
#include <thread>
#include <atomic>
#include <mutex>
#include <cmath>
#include "AdventureFacade.h"

// Главное окно игры: отрисовка SFML, обработка ввода, управление потоками.
// Связывает пользовательский интерфейс с игровой логикой через AdventureFacade.
class GameWindowClient {
private:
    sf::RenderWindow window;
    sf::Font font;
    AdventureFacade facade;

    sf::Texture bgTexture;
    sf::Sprite backgroundImg;
    sf::Texture charTexture;
    sf::Sprite characterImg;
    bool characterLoaded = false;
    bool characterVisible = true;

    sf::RectangleShape dialogueBox;
    sf::Text speakerLabel;
    sf::Text dialogueText;
    sf::Text inputText;
    sf::Text promptLabel;
    sf::Text statusText;
    sf::RectangleShape inputBox;
    sf::Text titleText;
    sf::Text subtitleText;
    sf::Text guideText;
    sf::Text modeText; // показывает текущий режим (EXPLORE/TALK/ACT)

    // Адаптивные размеры — масштабируются от высоты окна (база: 768px).
    // Все элементы UI рассчитываются относительно этого масштабного коэффициента,
    // чтобы интерфейс выглядел пропорционально при любом размере окна.
    float scale() const { return static_cast<float>(window.getSize().y) / 768.f; }
    int dialogueHeight() const { return std::max(150, static_cast<int>(window.getSize().y * 0.35f)); }
    int inputHeight() const { return static_cast<int>(36 * scale()); }
    int inputMargin() const { return static_cast<int>(8 * scale()); }
    unsigned int fontSize() const { return static_cast<unsigned int>(std::max(14.f, 20.f * scale())); }
    unsigned int speakerFontSize() const { return static_cast<unsigned int>(std::max(12.f, 18.f * scale())); }
    unsigned int modeFontSize() const { return static_cast<unsigned int>(std::max(12.f, 16.f * scale())); }
    float lineHeight() const { return fontSize() * 1.3f; }

    enum class State { INTRO, LOADING, PLAYING };
    State state;
    GameMode currentMode = GameMode::ACT;
    std::string userInput;
    std::string currentDialogue;
    std::string currentSpeaker;
    int scrollOffset = 0;        // строка с которой начинаем показ (прокрутка)
    int totalLines = 0;          // общее количество строк в тексте
    std::string currentBgPath;
    std::string currentCharPath;

    std::atomic<bool> loading;
    std::mutex resultMutex;
    FacadeResult pendingResult;
    std::atomic<bool> resultReady;
    std::thread workerThread;

    // Кодирование Unicode code point в UTF-8 строку.
    // SFML отдаёт символы как Uint32 code points, а наш текст хранится в UTF-8.
    static std::string utf8Encode(sf::Uint32 codepoint) {
        std::string encoded;
        if (codepoint < 0x80) {
            encoded += static_cast<char>(codepoint);
        }
        else if (codepoint < 0x800) {
            encoded += static_cast<char>(0xC0 | (codepoint >> 6));
            encoded += static_cast<char>(0x80 | (codepoint & 0x3F));
        }
        else if (codepoint < 0x10000) {
            encoded += static_cast<char>(0xE0 | (codepoint >> 12));
            encoded += static_cast<char>(0x80 | ((codepoint >> 6) & 0x3F));
            encoded += static_cast<char>(0x80 | (codepoint & 0x3F));
        }
        else {
            encoded += static_cast<char>(0xF0 | (codepoint >> 18));
            encoded += static_cast<char>(0x80 | ((codepoint >> 12) & 0x3F));
            encoded += static_cast<char>(0x80 | ((codepoint >> 6) & 0x3F));
            encoded += static_cast<char>(0x80 | (codepoint & 0x3F));
        }
        return encoded;
    }

    // Удаление последнего UTF-8 символа из строки.
    // UTF-8 continuation bytes начинаются с 10xxxxxx (0x80-0xBF),
    // поэтому пропускаем их назад до начала символа.
    static void utf8PopBack(std::string& str) {
        if (str.empty()) return;
        size_t pos = str.size() - 1;
        while (pos > 0 && (static_cast<unsigned char>(str[pos]) & 0xC0) == 0x80) --pos;
        str.erase(pos);
    }

    // Перенос текста по словам с учётом максимальной ширины.
    // Измеряет реальную ширину текста через SFML для точного переноса.
    std::string wrapText(const std::string& text, const sf::Font& textFont, unsigned int textSize, float maxWidth) {
        std::string result, currentLine, currentWord;
        for (size_t i = 0; i <= text.size(); ++i) {
            char character = (i < text.size()) ? text[i] : ' ';
            if (character == '\n') {
                // Принудительный перенос строки: добавляем накопленное слово и начинаем новую строку
                if (!currentWord.empty()) {
                    std::string testLine = currentLine.empty() ? currentWord : currentLine + " " + currentWord;
                    sf::Text measurement(sf::String::fromUtf8(testLine.begin(), testLine.end()), textFont, textSize);
                    if (measurement.getLocalBounds().width > maxWidth && !currentLine.empty()) {
                        result += currentLine + "\n";
                        currentLine = currentWord;
                    }
                    else currentLine = testLine;
                    currentWord.clear();
                }
                result += currentLine + "\n"; currentLine.clear();
            } else if (character == ' ' || i == text.size()) {
                // Пробел или конец текста: проверяем помещается ли слово в текущую строку
                if (!currentWord.empty()) {
                    std::string testLine = currentLine.empty() ? currentWord : currentLine + " " + currentWord;
                    sf::Text measurement(sf::String::fromUtf8(testLine.begin(), testLine.end()), textFont, textSize);
                    if (measurement.getLocalBounds().width > maxWidth && !currentLine.empty()) {
                        result += currentLine + "\n";
                        currentLine = currentWord;
                    }
                    else currentLine = testLine;
                    currentWord.clear();
                }
            } else { currentWord += character; }
        }
        if (!currentLine.empty()) result += currentLine;
        return result;
    }

    // Генерация процедурного фона по умолчанию: градиент неба с "звёздами".
    // Используется пока sd-webui не сгенерирует настоящий фон.
    void generateDefaultBackground(unsigned int width, unsigned int height) {
        sf::Image image;
        image.create(width, height);
        for (unsigned int y = 0; y < height; ++y) {
            // Градиент от тёмно-синего сверху к тёмно-фиолетовому снизу
            float gradientFactor = static_cast<float>(y) / height;
            sf::Uint8 red = static_cast<sf::Uint8>(15 + 25 * gradientFactor);
            sf::Uint8 green = static_cast<sf::Uint8>(10 + 15 * gradientFactor);
            sf::Uint8 blue = static_cast<sf::Uint8>(40 + 50 * (1.f - gradientFactor));
            for (unsigned int x = 0; x < width; ++x) image.setPixel(x, y, sf::Color(red, green, blue));
        }
        // Псевдослучайные "звёзды" с фиксированным seed для воспроизводимости
        unsigned int seed = 42;
        for (int i = 0; i < 200; ++i) {
            seed = seed * 1103515245 + 12345; unsigned int starX = (seed >> 4) % width;
            seed = seed * 1103515245 + 12345; unsigned int starY = (seed >> 4) % (height * 2 / 3);
            seed = seed * 1103515245 + 12345; sf::Uint8 brightness = 100 + (seed >> 4) % 156;
            image.setPixel(starX, starY, sf::Color(brightness, brightness, brightness + 30));
        }
        bgTexture.loadFromImage(image);
        backgroundImg.setTexture(bgTexture, true);
    }

    // Удаление фона с изображения персонажа через rembg (Python-скрипт).
    // Вызывается перед загрузкой текстуры персонажа, чтобы он отображался
    // поверх фона без белого/цветного прямоугольника.
    void removeCharacterBackground(const std::string& path) {
        std::string cmd =
            "C:\\MyProjects\\sd-webui-forge-neo\\venv\\Scripts\\python.exe "
            "C:\\MyProjects\\Object-oriented-analysis-and-design-2\\lab02\\novelllama\\novelllama\\rembg_helper.py "
            "\"" + path + "\"";
        STARTUPINFOA startupInfo = { sizeof(startupInfo) };
        startupInfo.dwFlags = STARTF_USESHOWWINDOW; startupInfo.wShowWindow = SW_HIDE;
        PROCESS_INFORMATION processInfo;
        std::vector<char> cmdBuf(cmd.begin(), cmd.end()); cmdBuf.push_back('\0');
        if (CreateProcessA(NULL, cmdBuf.data(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &startupInfo, &processInfo)) {
            WaitForSingleObject(processInfo.hProcess, 30000);
            CloseHandle(processInfo.hThread); CloseHandle(processInfo.hProcess);
        }
    }

    // Загрузка текстуры и настройка спрайта с масштабированием.
    // Для персонажа: удаляет фон, масштабирует до максимального размера
    // с сохранением пропорций и позиционирует справа внизу.
    // Для фона: растягивает на всё окно.
    void loadTexture(sf::Texture& tex, sf::Sprite& sprite, const std::string& path,
                     bool scaleToFill = false, bool isCharacter = false) {
        if (isCharacter) {
            removeCharacterBackground(path);
            if (!tex.loadFromFile(path)) return;
            sprite.setTexture(tex, true);
            float windowWidth = static_cast<float>(window.getSize().x);
            float windowHeight = static_cast<float>(window.getSize().y);
            float maxCharHeight = windowHeight - dialogueHeight() - 20.f;
            float maxCharWidth = windowWidth * 0.35f;
            float charScale = std::min(maxCharWidth / tex.getSize().x, maxCharHeight / tex.getSize().y);
            sprite.setScale(charScale, charScale);
            float scaledWidth = tex.getSize().x * charScale;
            float scaledHeight = tex.getSize().y * charScale;
            sprite.setPosition(windowWidth - scaledWidth - 20.f, windowHeight - scaledHeight - dialogueHeight());
            characterLoaded = true;
        } else if (scaleToFill) {
            if (!tex.loadFromFile(path)) return;
            sprite.setTexture(tex, true);
            sprite.setScale(static_cast<float>(window.getSize().x) / tex.getSize().x,
                            static_cast<float>(window.getSize().y) / tex.getSize().y);
            sprite.setPosition(0, 0);
        } else {
            if (!tex.loadFromFile(path)) return;
            sprite.setTexture(tex, true);
        }
    }

    // Перерисовка текста диалога с учётом прокрутки.
    // Разбивает текст на строки, показывает только видимые (scrollOffset..scrollOffset+maxLines),
    // и обновляет индикатор прокрутки в статусной строке.
    void refreshDialogueText(bool resetScroll = false) {
        unsigned int currentFontSize = fontSize();
        float currentLineHeight = lineHeight();
        float textWidth = static_cast<float>(window.getSize().x) - 40.f * scale();
        float textHeight = static_cast<float>(dialogueHeight()) - speakerFontSize() - 12.f * scale()
                   - inputHeight() - inputMargin() - 8.f * scale();
        int maxLines = std::max(1, static_cast<int>(textHeight / currentLineHeight));

        std::string wrapped = wrapText(currentDialogue, font, currentFontSize, textWidth);
        std::vector<std::string> lines;
        std::string currentLine;
        for (char character : wrapped) {
            if (character == '\n') { lines.push_back(currentLine); currentLine.clear(); }
            else currentLine += character;
        }
        if (!currentLine.empty()) lines.push_back(currentLine);
        totalLines = static_cast<int>(lines.size());

        // При новом тексте — прокрутка в начало
        if (resetScroll) scrollOffset = 0;

        // Ограничение прокрутки чтобы не уйти за границы текста
        int maxScroll = std::max(0, totalLines - maxLines);
        scrollOffset = std::max(0, std::min(scrollOffset, maxScroll));

        // Сборка видимых строк для отображения
        std::string visible;
        for (int i = scrollOffset; i < std::min(totalLines, scrollOffset + maxLines); ++i) {
            if (i > scrollOffset) visible += "\n";
            visible += lines[i];
        }
        dialogueText.setCharacterSize(currentFontSize);
        dialogueText.setString(sf::String::fromUtf8(visible.begin(), visible.end()));

        // Индикатор прокрутки (стрелки) в статусной строке
        if (totalLines > maxLines) {
            std::string indicator = "";
            if (scrollOffset > 0) indicator += "[Scroll Up] ";
            if (scrollOffset < maxScroll) indicator += "[Scroll Down]";
            if (state == State::PLAYING)
                statusText.setString("Your turn:  " + indicator);
        }
    }

    // Обновление всех визуальных элементов по результату хода.
    // Загружает новые текстуры если пути изменились, обновляет текст диалога.
    void updateView(const FacadeResult& result) {
        if (result.backgroundImagePath != currentBgPath) {
            currentBgPath = result.backgroundImagePath;
            loadTexture(bgTexture, backgroundImg, result.backgroundImagePath, true, false);
        }
        if (result.characterImagePath != currentCharPath) {
            currentCharPath = result.characterImagePath;
            if (!result.characterImagePath.empty())
                loadTexture(charTexture, characterImg, result.characterImagePath, false, true);
        }
        characterVisible = !result.characterImagePath.empty();
        currentSpeaker = result.speaker;
        currentDialogue = result.text;
        speakerLabel.setString(sf::String::fromUtf8(currentSpeaker.begin(), currentSpeaker.end()));

        // Адаптивный перенос текста с прокруткой (сброс на начало)
        refreshDialogueText(true);
    }

    std::string modeString() const {
        switch (currentMode) {
            case GameMode::EXPLORE: return "EXPLORE";
            case GameMode::TALK:    return "TALK";
            case GameMode::ACT:     return "ACT";
        }
        return "ACT";
    }

    void updateModeDisplay() {
        std::string label = "[Tab] Mode: " + modeString();
        if (currentMode == GameMode::EXPLORE) label += "  (type where to go)";
        else if (currentMode == GameMode::TALK) label += "  (type what to say)";
        else label += "  (type what to do)";
        modeText.setString(label);
    }

    // Циклическое переключение режима: ACT -> EXPLORE -> TALK -> ACT
    void cycleMode() {
        if (currentMode == GameMode::ACT) currentMode = GameMode::EXPLORE;
        else if (currentMode == GameMode::EXPLORE) currentMode = GameMode::TALK;
        else currentMode = GameMode::ACT;
        updateModeDisplay();
    }

    // Пересчёт позиций и размеров всех UI-элементов при изменении размера окна.
    // Все координаты рассчитываются от текущих размеров окна и масштабного коэффициента.
    void repositionUI() {
        float windowWidth = static_cast<float>(window.getSize().x);
        float windowHeight = static_cast<float>(window.getSize().y);
        float scaleFactor = scale();
        int inputH = inputHeight();
        int inputM = inputMargin();
        float padding = 20.f * scaleFactor;

        // Размеры шрифтов пересчитываются при каждом ресайзе
        speakerLabel.setCharacterSize(speakerFontSize());
        inputText.setCharacterSize(static_cast<unsigned int>(std::max(12.f, 16.f * scaleFactor)));
        modeText.setCharacterSize(modeFontSize());
        statusText.setCharacterSize(static_cast<unsigned int>(std::max(14.f, 18.f * scaleFactor)));

        // Панель диалога — полупрозрачная полоса внизу окна
        dialogueBox.setSize(sf::Vector2f(windowWidth, static_cast<float>(dialogueHeight())));
        dialogueBox.setPosition(0, windowHeight - dialogueHeight());
        speakerLabel.setPosition(padding, windowHeight - dialogueHeight() + 6.f * scaleFactor);
        dialogueText.setPosition(padding, windowHeight - dialogueHeight() + speakerFontSize() + 12.f * scaleFactor);

        // Поле ввода — внизу панели диалога
        inputBox.setSize(sf::Vector2f(windowWidth - padding, static_cast<float>(inputH)));
        inputBox.setPosition(padding / 2.f, windowHeight - inputH - inputM);
        inputText.setPosition(padding / 2.f + 6.f * scaleFactor, windowHeight - inputH - inputM + 4.f * scaleFactor);

        // Заголовки и подсказки — центрирование по горизонтали
        promptLabel.setCharacterSize(static_cast<unsigned int>(std::max(16.f, 24.f * scaleFactor)));
        promptLabel.setPosition((windowWidth - promptLabel.getLocalBounds().width) / 2.f, windowHeight - dialogueHeight() - 60.f * scaleFactor);
        titleText.setCharacterSize(static_cast<unsigned int>(52.f * scaleFactor));
        titleText.setPosition((windowWidth - titleText.getLocalBounds().width) / 2.f, 60.f * scaleFactor);
        subtitleText.setCharacterSize(static_cast<unsigned int>(std::max(12.f, 18.f * scaleFactor)));
        subtitleText.setPosition((windowWidth - subtitleText.getLocalBounds().width) / 2.f, 120.f * scaleFactor);
        guideText.setCharacterSize(static_cast<unsigned int>(std::max(11.f, 14.f * scaleFactor)));
        guideText.setPosition(30.f * scaleFactor, 160.f * scaleFactor);
        modeText.setPosition(padding, 50.f * scaleFactor);
        statusText.setPosition(padding, 20.f * scaleFactor);

        // Масштабирование фона на всё окно
        if (bgTexture.getSize().x > 0)
            backgroundImg.setScale(windowWidth / bgTexture.getSize().x, windowHeight / bgTexture.getSize().y);

        // Персонаж: масштабирование с сохранением пропорций, позиция справа внизу
        if (characterLoaded && charTexture.getSize().x > 0) {
            float maxCharHeight = windowHeight - dialogueHeight() - padding;
            float maxCharWidth = windowWidth * 0.35f;
            float charScale = std::min(maxCharWidth / charTexture.getSize().x, maxCharHeight / charTexture.getSize().y);
            characterImg.setScale(charScale, charScale);
            float scaledCharWidth = charTexture.getSize().x * charScale;
            float scaledCharHeight = charTexture.getSize().y * charScale;
            characterImg.setPosition(windowWidth - scaledCharWidth - padding, windowHeight - scaledCharHeight - dialogueHeight());
        }
    }

    void joinWorker() { if (workerThread.joinable()) workerThread.join(); }

    // Обработка ввода пользователя: запуск фоновой задачи для обращения к фасаду.
    // В состоянии INTRO — создание мира, в PLAYING — обработка хода.
    // Фоновый поток нужен чтобы UI не замерзал во время генерации LLM/изображений.
    void handleInput() {
        if (userInput.empty()) return;
        if (state == State::INTRO) {
            state = State::LOADING; loading = true; resultReady = false;
            statusText.setString("Creating your story world... (this may take a minute)");
            std::string idea = userInput; userInput.clear(); inputText.setString("_");
            joinWorker();
            workerThread = std::thread([this, idea]() {
                try {
                    FacadeResult facadeResult = facade.start(idea);
                    std::lock_guard<std::mutex> lock(resultMutex); pendingResult = facadeResult;
                } catch (...) {
                    std::lock_guard<std::mutex> lock(resultMutex);
                    pendingResult = { "An error occurred.", "character.png", "background.png", "System" };
                }
                loading = false; resultReady = true;
            });
        } else if (state == State::PLAYING) {
            state = State::LOADING; loading = true; resultReady = false;
            statusText.setString(modeString() + ": processing...");
            std::string input = userInput; userInput.clear(); inputText.setString("_");
            GameMode mode = currentMode;
            joinWorker();
            workerThread = std::thread([this, input, mode]() {
                try {
                    FacadeResult facadeResult = facade.processTurn(input, mode);
                    std::lock_guard<std::mutex> lock(resultMutex); pendingResult = facadeResult;
                } catch (...) {
                    std::lock_guard<std::mutex> lock(resultMutex);
                    pendingResult = { "An error occurred.", currentCharPath, currentBgPath, "System" };
                }
                loading = false; resultReady = true;
            });
        }
    }

public:
    GameWindowClient() : window(), state(State::INTRO), loading(false), resultReady(false) {}
    ~GameWindowClient() { joinWorker(); }

    // Главный цикл: инициализация окна и UI, затем бесконечный цикл отрисовки.
    // Обрабатывает события SFML (ввод текста, ресайз, прокрутка),
    // проверяет завершение фоновых задач и перерисовывает кадр.
    void run() {
        window.create(sf::VideoMode(1024, 768), L"NovelLlama - Visual Novel", sf::Style::Default);
        window.setFramerateLimit(60);
        if (!font.loadFromFile("OpenSans-Regular.ttf")) return;

        generateDefaultBackground(1024, 768);
        currentBgPath = "__intro__"; currentCharPath = "__intro__";

        // Настройка текстовых элементов титульного экрана
        titleText.setFont(font); titleText.setCharacterSize(52);
        titleText.setFillColor(sf::Color(255, 220, 130));
        titleText.setOutlineColor(sf::Color(80, 40, 0)); titleText.setOutlineThickness(3.f);
        titleText.setString("NovelLlama");

        subtitleText.setFont(font); subtitleText.setCharacterSize(18);
        subtitleText.setFillColor(sf::Color(180, 180, 200));
        subtitleText.setString("AI-Powered Visual Novel");

        guideText.setFont(font); guideText.setCharacterSize(14);
        guideText.setFillColor(sf::Color(160, 160, 180));
        guideText.setString(
            "HOW TO PLAY:\n"
            "Press [Tab] to switch between three modes:\n\n"
            "  EXPLORE - type where you want to go\n"
            "    e.g. 'the dark forest', 'a nearby cave'\n\n"
            "  TALK - type what you want to say to the NPC here\n"
            "    e.g. 'who are you?', 'tell me about the curse'\n\n"
            "  ACT - type what you want to do\n"
            "    e.g. 'I search the room', 'I attack the beast'\n\n"
            "Press [Enter] to submit your input.\n"
            "Start by typing your story idea below!");

        // Настройка элементов панели диалога
        dialogueBox.setFillColor(sf::Color(0, 0, 0, 200));
        speakerLabel.setFont(font); speakerLabel.setCharacterSize(16);
        speakerLabel.setFillColor(sf::Color(255, 220, 100)); speakerLabel.setStyle(sf::Text::Bold);
        dialogueText.setFont(font); dialogueText.setCharacterSize(16);
        dialogueText.setFillColor(sf::Color::White);
        inputBox.setFillColor(sf::Color(40, 40, 40, 230));
        inputBox.setOutlineColor(sf::Color(120, 120, 120)); inputBox.setOutlineThickness(1.f);
        inputText.setFont(font); inputText.setCharacterSize(16);
        inputText.setFillColor(sf::Color(200, 255, 200)); inputText.setString("_");
        promptLabel.setFont(font); promptLabel.setCharacterSize(24);
        promptLabel.setFillColor(sf::Color::White);
        promptLabel.setOutlineColor(sf::Color::Black); promptLabel.setOutlineThickness(2.f);
        promptLabel.setString("Enter your story idea and press Enter:");
        statusText.setFont(font); statusText.setCharacterSize(18);
        statusText.setFillColor(sf::Color::Yellow);
        statusText.setOutlineColor(sf::Color::Black); statusText.setOutlineThickness(2.f);
        statusText.setPosition(20.f, 20.f);
        statusText.setString("Image generation: starting sd-webui...");

        modeText.setFont(font); modeText.setCharacterSize(16);
        modeText.setFillColor(sf::Color(100, 255, 100));
        modeText.setOutlineColor(sf::Color::Black); modeText.setOutlineThickness(1.f);
        updateModeDisplay();

        repositionUI();

        // Главный цикл отрисовки
        while (window.isOpen()) {
            sf::Event event;
            while (window.pollEvent(event)) {
                if (event.type == sf::Event::Closed) window.close();
                if (event.type == sf::Event::Resized) {
                    sf::FloatRect area(0, 0, static_cast<float>(event.size.width), static_cast<float>(event.size.height));
                    window.setView(sf::View(area));
                    repositionUI();
                    if (!currentDialogue.empty()) refreshDialogueText(false);
                }
                // Прокрутка текста колесом мыши (по 2 строки за шаг)
                if (event.type == sf::Event::MouseWheelScrolled && state == State::PLAYING) {
                    if (event.mouseWheelScroll.delta > 0) scrollOffset = std::max(0, scrollOffset - 2);
                    else scrollOffset += 2;
                    refreshDialogueText(false);
                }
                if (event.type == sf::Event::KeyPressed && !loading) {
                    if (event.key.code == sf::Keyboard::Tab && state == State::PLAYING)
                        cycleMode();
                }
                // Обработка текстового ввода: Enter, Backspace, обычные символы
                if (event.type == sf::Event::TextEntered && !loading) {
                    if (event.text.unicode == '\r' || event.text.unicode == '\n') {
                        handleInput();
                    } else if (event.text.unicode == '\b') {
                        if (!userInput.empty()) {
                            utf8PopBack(userInput);
                            std::string displayText = userInput + "_";
                            inputText.setString(sf::String::fromUtf8(displayText.begin(), displayText.end()));
                        }
                    } else if (event.text.unicode == '\t') {
                        // Tab обрабатывается через KeyPressed выше
                    } else if (event.text.unicode >= 32) {
                        userInput += utf8Encode(event.text.unicode);
                        std::string displayText = userInput + "_";
                        inputText.setString(sf::String::fromUtf8(displayText.begin(), displayText.end()));
                    }
                }
            }

            // Обновление статуса sd-webui на титульном экране
            if (state == State::INTRO) {
                statusText.setString(facade.isImageGenAvailable() ? "Image generation: ON" : "Image generation: starting sd-webui...");
            }
            // Проверка завершения фоновой задачи и применение результата
            if (resultReady) {
                std::lock_guard<std::mutex> lock(resultMutex);
                updateView(pendingResult);
                state = State::PLAYING;
                resultReady = false;
                updateModeDisplay();
                statusText.setString("Your turn:");
            }

            // Отрисовка кадра
            window.clear(sf::Color::Black);
            window.draw(backgroundImg);
            if ((state == State::PLAYING || state == State::LOADING) && characterLoaded && characterVisible)
                window.draw(characterImg);
            window.draw(dialogueBox);
            if (state == State::INTRO) { window.draw(titleText); window.draw(subtitleText); window.draw(guideText); window.draw(promptLabel); }
            if (state == State::PLAYING) { window.draw(speakerLabel); window.draw(modeText); }
            window.draw(dialogueText);
            window.draw(inputBox);
            window.draw(inputText);
            window.draw(statusText);
            window.display();
        }
        joinWorker();
    }
};
