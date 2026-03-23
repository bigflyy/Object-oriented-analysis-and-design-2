#pragma once
#include <SFML/Graphics.hpp>
#include <string>
#include <thread>
#include <atomic>
#include <mutex>
#include <cmath>
#include <windows.h>
#include "LLMStoryCreator.h"
#include "ImageClient.h"
#include "MemoryManager.h"

// БЕЗ паттерна Фасад: GameWindowClient напрямую вызывает все подсистемы.
// Тот же GUI, тот же результат — но вся координация лежит на клиенте.
// Сравните с GameWindowClient.h где клиент использует только AdventureFacade.

// Режимы игры (дублируем, т.к. не включаем AdventureFacade.h)
enum class GameModeNF { EXPLORE, TALK, ACT };

// Результат хода (дублируем структуру)
struct TurnResult {
    std::string text;
    std::string characterImagePath;
    std::string backgroundImagePath;
    std::string speaker;
};

class GameWindowClientNoFacade {
private:
    sf::RenderWindow window;
    sf::Font font;

    // Три подсистемы напрямую — без фасада клиент знает о каждой
    LLMStoryCreator llm;
    ImageClient imgGenerator;
    MemoryManager memoryManager;
    std::string currentBackground = "background.png";
    std::string currentCharacter = "character.png";

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
    sf::Text modeText;

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
    GameModeNF currentMode = GameModeNF::ACT;
    std::string userInput;
    std::string currentDialogue;
    std::string currentSpeaker;
    int scrollOffset = 0;
    int totalLines = 0;
    std::string currentBgPath;
    std::string currentCharPath;

    std::atomic<bool> loading;
    std::mutex resultMutex;
    TurnResult pendingResult;
    std::atomic<bool> resultReady;
    std::thread workerThread;

    // === Вспомогательные методы (в фасадной версии это AdventureFacade private) ===

    std::string characterPath() {
        return memoryManager.currentCharacterName.empty() ? "" : currentCharacter;
    }

    std::vector<std::string> currentImages() {
        std::vector<std::string> images;
        if (currentBackground != "background.png") images.push_back(currentBackground);
        if (currentCharacter != "character.png" && !memoryManager.currentCharacterName.empty())
            images.push_back(currentCharacter);
        return images;
    }

    std::string resolveBackground(const std::string& name, const std::string& visual) {
        Entity* cached = memoryManager.findByName(name);
        if (cached && !cached->imagePath.empty() && cached->type == "location")
            return cached->imagePath;
        llm.unloadFromVRAM();
        if (!imgGenerator.isAvailable()) return currentBackground;
        std::string image = imgGenerator.generateImage(visual, "", 768, 768, false);
        if (image.empty()) return currentBackground;
        Entity location(name, visual, "location");
        location.imagePath = image;
        memoryManager.addEntity(location);
        return image;
    }

    std::string resolveCharacter(const std::string& name) {
        Entity* cached = memoryManager.findByName(name);
        if (cached && !cached->imagePath.empty() && cached->type == "NPC") {
            memoryManager.currentCharacterName = cached->name;
            return cached->imagePath;
        }
        std::string visual = llm.describeCharacter(memoryManager.getGlobalMemoryPrompt(), name);
        llm.unloadFromVRAM();
        if (!imgGenerator.isAvailable()) { memoryManager.currentCharacterName = name; return currentCharacter; }
        std::string image = imgGenerator.generateImage(visual, "", 512, 768, true);
        if (image.empty()) { memoryManager.currentCharacterName = name; return currentCharacter; }
        Entity npc(name, visual, "NPC");
        npc.imagePath = image;
        memoryManager.addEntity(npc);
        memoryManager.currentCharacterName = name;
        return image;
    }

    // === Три режима — вся координация напрямую в клиенте ===

    TurnResult doStart(const std::string& userIdea) {
        // Клиент сам координирует: LLM сюжет -> LLM сущности -> ImageClient изображения -> LLM нарратив
        memoryManager.plot = llm.request(
            "Create a 2-3 sentence plot for a second-person visual novel. Use 'you'. Based on: " + userIdea,
            "Respond with just the plot. No extra text. "
            "Be creative and specific to the user's idea. DO NOT use themes about fading memories, "
            "forgotten pasts, or lost identities unless the user specifically asked for that. "
            "Focus on concrete goals: find something, save someone, defeat an enemy, escape a place, solve a mystery.");

        auto entities = llm.createInitialWorld(memoryManager.plot);
        llm.unloadFromVRAM();
        for (int i = 0; i < 60 && !imgGenerator.isAvailable(); ++i) Sleep(1000);

        for (auto& entity : entities) {
            bool isNpc = (toLower(entity.type) == "npc");
            std::string image = imgGenerator.generateImage(entity.description, "", isNpc ? 512 : 768, 768, isNpc);
            if (!image.empty()) entity.imagePath = image;
            memoryManager.addEntity(entity);
        }

        for (auto& [entityName, entity] : memoryManager.entities) {
            std::string type = toLower(entity.type);
            if (type == "location" && memoryManager.currentLocation.empty()) {
                memoryManager.currentLocation = entity.name;
                if (!entity.imagePath.empty()) currentBackground = entity.imagePath;
            }
            if (type == "npc" && memoryManager.currentCharacterName.empty()) {
                memoryManager.currentCharacterName = entity.name;
                if (!entity.imagePath.empty()) currentCharacter = entity.imagePath;
            }
        }

        std::string intro = llm.narrateArrival(memoryManager.getGlobalMemoryPrompt(),
            memoryManager.currentLocation, memoryManager.currentCharacterName);
        memoryManager.addGlobalEvent("Story began: " + userIdea);
        return { intro, characterPath(), currentBackground, "Narrator" };
    }

    TurnResult doExplore(const std::string& destination) {
        llm.compactMemory(memoryManager);
        std::string previousNpc = memoryManager.currentCharacterName;

        Entity* cached = memoryManager.findByName(destination);
        std::string locationName;
        if (cached && cached->type == "location") {
            locationName = cached->name;
            if (!cached->imagePath.empty()) currentBackground = cached->imagePath;
        } else {
            std::string visual = llm.describePlace(memoryManager.getGlobalMemoryPrompt(), destination);
            locationName = destination;
            currentBackground = resolveBackground(destination, visual);
        }

        memoryManager.currentLocation = locationName;
        memoryManager.currentCharacterName = "";
        memoryManager.addGlobalEvent("Traveled to: " + locationName);

        std::string whoHere = llm.whoIsHere(memoryManager.getGlobalMemoryPrompt(), locationName);
        if (!whoHere.empty())
            currentCharacter = resolveCharacter(whoHere);

        bool followed = (!previousNpc.empty() && !memoryManager.currentCharacterName.empty()
            && memoryManager.findByName(previousNpc) == memoryManager.findByName(memoryManager.currentCharacterName));
        if (!previousNpc.empty())
            memoryManager.addGlobalEvent(previousNpc + (followed ? " followed to " + locationName : " stayed behind"));
        if (!memoryManager.currentCharacterName.empty() && !followed)
            memoryManager.addGlobalEvent("Met: " + memoryManager.currentCharacterName);

        std::string narration = llm.narrateArrival(memoryManager.getGlobalMemoryPrompt(),
            locationName, memoryManager.currentCharacterName, followed ? "" : previousNpc, followed, currentImages());
        return { narration, characterPath(), currentBackground, "Narrator" };
    }

    TurnResult doTalk(const std::string& playerInput) {
        llm.compactMemory(memoryManager);
        if (memoryManager.currentCharacterName.empty())
            return { "There is no one here to talk to. Use EXPLORE to find someone.", "", currentBackground, "Narrator" };

        std::string npcName = memoryManager.currentCharacterName;
        std::string dialogueText = llm.dialogue(memoryManager.getGlobalMemoryPrompt(), npcName, playerInput, currentImages());
        memoryManager.addHistory(npcName, "Player: " + playerInput);
        memoryManager.addHistory(npcName, npcName + " responded");
        return { dialogueText, characterPath(), currentBackground, npcName };
    }

    TurnResult doAct(const std::string& playerInput) {
        llm.compactMemory(memoryManager);

        std::string narration = llm.narrateAction(memoryManager.getGlobalMemoryPrompt(), playerInput, currentImages());
        memoryManager.addGlobalEvent("Player: " + playerInput);

        std::string changed = llm.classifyChange(memoryManager.getGlobalMemoryPrompt(), narration);

        if (changed == "APPEAR") {
            std::string charName = llm.nameNewCharacter(memoryManager.getGlobalMemoryPrompt(), narration);
            std::string previousName = memoryManager.currentCharacterName;
            if (!previousName.empty())
                memoryManager.addGlobalEvent(previousName + " left");
            currentCharacter = resolveCharacter(charName);
            memoryManager.addGlobalEvent("Met: " + memoryManager.currentCharacterName);
            narration += "\n\n" + llm.narrateTransition(memoryManager.getGlobalMemoryPrompt(), narration, charName, previousName);
        }
        else if (changed == "LEAVE" && !memoryManager.currentCharacterName.empty()) {
            std::string whoLeft = memoryManager.currentCharacterName;
            memoryManager.addGlobalEvent(whoLeft + " left");
            memoryManager.currentCharacterName = "";
            narration += "\n\n" + llm.narrateTransition(memoryManager.getGlobalMemoryPrompt(), narration, "", whoLeft);
        }

        return { narration, characterPath(), currentBackground, "Narrator" };
    }

    // === UI методы (идентичны GameWindowClient) ===

    static std::string utf8Encode(sf::Uint32 codepoint) {
        std::string encoded;
        if (codepoint < 0x80) { encoded += static_cast<char>(codepoint); }
        else if (codepoint < 0x800) { encoded += static_cast<char>(0xC0 | (codepoint >> 6)); encoded += static_cast<char>(0x80 | (codepoint & 0x3F)); }
        else if (codepoint < 0x10000) { encoded += static_cast<char>(0xE0 | (codepoint >> 12)); encoded += static_cast<char>(0x80 | ((codepoint >> 6) & 0x3F)); encoded += static_cast<char>(0x80 | (codepoint & 0x3F)); }
        else { encoded += static_cast<char>(0xF0 | (codepoint >> 18)); encoded += static_cast<char>(0x80 | ((codepoint >> 12) & 0x3F)); encoded += static_cast<char>(0x80 | ((codepoint >> 6) & 0x3F)); encoded += static_cast<char>(0x80 | (codepoint & 0x3F)); }
        return encoded;
    }
    static void utf8PopBack(std::string& str) {
        if (str.empty()) return;
        size_t pos = str.size() - 1;
        while (pos > 0 && (static_cast<unsigned char>(str[pos]) & 0xC0) == 0x80) --pos;
        str.erase(pos);
    }
    std::string wrapText(const std::string& text, const sf::Font& textFont, unsigned int textSize, float maxWidth) {
        std::string result, currentLine, currentWord;
        for (size_t i = 0; i <= text.size(); ++i) {
            char character = (i < text.size()) ? text[i] : ' ';
            if (character == '\n') {
                if (!currentWord.empty()) { std::string testLine = currentLine.empty() ? currentWord : currentLine + " " + currentWord; sf::Text measurement(sf::String::fromUtf8(testLine.begin(), testLine.end()), textFont, textSize); if (measurement.getLocalBounds().width > maxWidth && !currentLine.empty()) { result += currentLine + "\n"; currentLine = currentWord; } else currentLine = testLine; currentWord.clear(); }
                result += currentLine + "\n"; currentLine.clear();
            } else if (character == ' ' || i == text.size()) {
                if (!currentWord.empty()) { std::string testLine = currentLine.empty() ? currentWord : currentLine + " " + currentWord; sf::Text measurement(sf::String::fromUtf8(testLine.begin(), testLine.end()), textFont, textSize); if (measurement.getLocalBounds().width > maxWidth && !currentLine.empty()) { result += currentLine + "\n"; currentLine = currentWord; } else currentLine = testLine; currentWord.clear(); }
            } else { currentWord += character; }
        }
        if (!currentLine.empty()) result += currentLine;
        return result;
    }
    void generateDefaultBackground(unsigned int width, unsigned int height) {
        sf::Image image; image.create(width, height);
        for (unsigned int y = 0; y < height; ++y) { float g = static_cast<float>(y) / height; for (unsigned int x = 0; x < width; ++x) image.setPixel(x, y, sf::Color(static_cast<sf::Uint8>(15+25*g), static_cast<sf::Uint8>(10+15*g), static_cast<sf::Uint8>(40+50*(1.f-g)))); }
        unsigned int seed = 42; for (int i = 0; i < 200; ++i) { seed = seed*1103515245+12345; unsigned int sx = (seed>>4)%width; seed = seed*1103515245+12345; unsigned int sy = (seed>>4)%(height*2/3); seed = seed*1103515245+12345; sf::Uint8 b = 100+(seed>>4)%156; image.setPixel(sx, sy, sf::Color(b,b,b+30)); }
        bgTexture.loadFromImage(image); backgroundImg.setTexture(bgTexture, true);
    }
    void removeCharacterBackground(const std::string& path) {
        std::string cmd = "C:\\MyProjects\\sd-webui-forge-neo\\venv\\Scripts\\python.exe C:\\MyProjects\\Object-oriented-analysis-and-design-2\\lab02\\novelllama\\novelllama\\rembg_helper.py \"" + path + "\"";
        STARTUPINFOA si = { sizeof(si) }; si.dwFlags = STARTF_USESHOWWINDOW; si.wShowWindow = SW_HIDE; PROCESS_INFORMATION pi;
        std::vector<char> cmdBuf(cmd.begin(), cmd.end()); cmdBuf.push_back('\0');
        if (CreateProcessA(NULL, cmdBuf.data(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) { WaitForSingleObject(pi.hProcess, 30000); CloseHandle(pi.hThread); CloseHandle(pi.hProcess); }
    }
    void loadTexture(sf::Texture& tex, sf::Sprite& sprite, const std::string& path, bool scaleToFill = false, bool isCharacter = false) {
        if (isCharacter) {
            removeCharacterBackground(path); if (!tex.loadFromFile(path)) return; sprite.setTexture(tex, true);
            float ww = static_cast<float>(window.getSize().x), wh = static_cast<float>(window.getSize().y);
            float s = std::min(ww*0.35f/tex.getSize().x, (wh-dialogueHeight()-20.f)/tex.getSize().y);
            sprite.setScale(s,s); sprite.setPosition(ww-tex.getSize().x*s-20.f, wh-tex.getSize().y*s-dialogueHeight()); characterLoaded = true;
        } else if (scaleToFill) { if (!tex.loadFromFile(path)) return; sprite.setTexture(tex, true); sprite.setScale(static_cast<float>(window.getSize().x)/tex.getSize().x, static_cast<float>(window.getSize().y)/tex.getSize().y); sprite.setPosition(0,0);
        } else { if (!tex.loadFromFile(path)) return; sprite.setTexture(tex, true); }
    }
    void refreshDialogueText(bool resetScroll = false) {
        unsigned int fs = fontSize(); float lh = lineHeight();
        float tw = static_cast<float>(window.getSize().x) - 40.f*scale();
        float th = static_cast<float>(dialogueHeight()) - speakerFontSize() - 12.f*scale() - inputHeight() - inputMargin() - 8.f*scale();
        int maxLines = std::max(1, static_cast<int>(th/lh));
        std::string wrapped = wrapText(currentDialogue, font, fs, tw);
        std::vector<std::string> lines; std::string cl;
        for (char c : wrapped) { if (c == '\n') { lines.push_back(cl); cl.clear(); } else cl += c; }
        if (!cl.empty()) lines.push_back(cl); totalLines = static_cast<int>(lines.size());
        if (resetScroll) scrollOffset = 0;
        int maxScroll = std::max(0, totalLines - maxLines);
        scrollOffset = std::max(0, std::min(scrollOffset, maxScroll));
        std::string visible;
        for (int i = scrollOffset; i < std::min(totalLines, scrollOffset + maxLines); ++i) { if (i > scrollOffset) visible += "\n"; visible += lines[i]; }
        dialogueText.setCharacterSize(fs); dialogueText.setString(sf::String::fromUtf8(visible.begin(), visible.end()));
        if (totalLines > maxLines && state == State::PLAYING) { std::string ind; if (scrollOffset > 0) ind += "[Scroll Up] "; if (scrollOffset < maxScroll) ind += "[Scroll Down]"; statusText.setString("Your turn:  " + ind); }
    }
    void updateView(const TurnResult& result) {
        if (result.backgroundImagePath != currentBgPath) { currentBgPath = result.backgroundImagePath; loadTexture(bgTexture, backgroundImg, result.backgroundImagePath, true, false); }
        if (result.characterImagePath != currentCharPath) { currentCharPath = result.characterImagePath; if (!result.characterImagePath.empty()) loadTexture(charTexture, characterImg, result.characterImagePath, false, true); }
        characterVisible = !result.characterImagePath.empty();
        currentSpeaker = result.speaker; currentDialogue = result.text;
        speakerLabel.setString(sf::String::fromUtf8(currentSpeaker.begin(), currentSpeaker.end()));
        refreshDialogueText(true);
    }
    std::string modeString() const { switch (currentMode) { case GameModeNF::EXPLORE: return "EXPLORE"; case GameModeNF::TALK: return "TALK"; case GameModeNF::ACT: return "ACT"; } return "ACT"; }
    void updateModeDisplay() { std::string label = "[Tab] Mode: " + modeString(); if (currentMode == GameModeNF::EXPLORE) label += "  (type where to go)"; else if (currentMode == GameModeNF::TALK) label += "  (type what to say)"; else label += "  (type what to do)"; modeText.setString(label); }
    void cycleMode() { if (currentMode == GameModeNF::ACT) currentMode = GameModeNF::EXPLORE; else if (currentMode == GameModeNF::EXPLORE) currentMode = GameModeNF::TALK; else currentMode = GameModeNF::ACT; updateModeDisplay(); }
    void repositionUI() {
        float ww = static_cast<float>(window.getSize().x), wh = static_cast<float>(window.getSize().y), sf = scale();
        int ih = inputHeight(), im = inputMargin(); float pad = 20.f*sf;
        speakerLabel.setCharacterSize(speakerFontSize()); inputText.setCharacterSize(static_cast<unsigned int>(std::max(12.f,16.f*sf)));
        modeText.setCharacterSize(modeFontSize()); statusText.setCharacterSize(static_cast<unsigned int>(std::max(14.f,18.f*sf)));
        dialogueBox.setSize(sf::Vector2f(ww, static_cast<float>(dialogueHeight()))); dialogueBox.setPosition(0, wh-dialogueHeight());
        speakerLabel.setPosition(pad, wh-dialogueHeight()+6.f*sf); dialogueText.setPosition(pad, wh-dialogueHeight()+speakerFontSize()+12.f*sf);
        inputBox.setSize(sf::Vector2f(ww-pad, static_cast<float>(ih))); inputBox.setPosition(pad/2.f, wh-ih-im);
        inputText.setPosition(pad/2.f+6.f*sf, wh-ih-im+4.f*sf);
        promptLabel.setCharacterSize(static_cast<unsigned int>(std::max(16.f,24.f*sf))); promptLabel.setPosition((ww-promptLabel.getLocalBounds().width)/2.f, wh-dialogueHeight()-60.f*sf);
        titleText.setCharacterSize(static_cast<unsigned int>(52.f*sf)); titleText.setPosition((ww-titleText.getLocalBounds().width)/2.f, 60.f*sf);
        subtitleText.setCharacterSize(static_cast<unsigned int>(std::max(12.f,18.f*sf))); subtitleText.setPosition((ww-subtitleText.getLocalBounds().width)/2.f, 120.f*sf);
        guideText.setCharacterSize(static_cast<unsigned int>(std::max(11.f,14.f*sf))); guideText.setPosition(30.f*sf, 160.f*sf);
        modeText.setPosition(pad, 50.f*sf); statusText.setPosition(pad, 20.f*sf);
        if (bgTexture.getSize().x > 0) backgroundImg.setScale(ww/bgTexture.getSize().x, wh/bgTexture.getSize().y);
        if (characterLoaded && charTexture.getSize().x > 0) { float mh = wh-dialogueHeight()-pad, mw = ww*0.35f; float cs = std::min(mw/charTexture.getSize().x, mh/charTexture.getSize().y); characterImg.setScale(cs,cs); characterImg.setPosition(ww-charTexture.getSize().x*cs-pad, wh-charTexture.getSize().y*cs-dialogueHeight()); }
    }
    void joinWorker() { if (workerThread.joinable()) workerThread.join(); }

    // Обработка ввода — клиент напрямую вызывает подсистемы
    void handleInput() {
        if (userInput.empty()) return;
        if (state == State::INTRO) {
            state = State::LOADING; loading = true; resultReady = false;
            statusText.setString("Creating your story world... (this may take a minute)");
            std::string idea = userInput; userInput.clear(); inputText.setString("_");
            joinWorker();
            workerThread = std::thread([this, idea]() {
                try { TurnResult r = doStart(idea); std::lock_guard<std::mutex> lock(resultMutex); pendingResult = r; }
                catch (...) { std::lock_guard<std::mutex> lock(resultMutex); pendingResult = { "An error occurred.", "", "background.png", "System" }; }
                loading = false; resultReady = true;
            });
        } else if (state == State::PLAYING) {
            state = State::LOADING; loading = true; resultReady = false;
            statusText.setString(modeString() + ": processing...");
            std::string input = userInput; userInput.clear(); inputText.setString("_");
            GameModeNF mode = currentMode;
            joinWorker();
            workerThread = std::thread([this, input, mode]() {
                try {
                    TurnResult r;
                    switch (mode) {
                        case GameModeNF::EXPLORE: r = doExplore(input); break;
                        case GameModeNF::TALK:    r = doTalk(input); break;
                        case GameModeNF::ACT:     r = doAct(input); break;
                    }
                    std::lock_guard<std::mutex> lock(resultMutex); pendingResult = r;
                } catch (...) { std::lock_guard<std::mutex> lock(resultMutex); pendingResult = { "An error occurred.", currentCharPath, currentBgPath, "System" }; }
                loading = false; resultReady = true;
            });
        }
    }

public:
    GameWindowClientNoFacade() : window(), state(State::INTRO), loading(false), resultReady(false) {}
    ~GameWindowClientNoFacade() { joinWorker(); }

    void run() {
        window.create(sf::VideoMode(1024, 768), L"NovelLlama - Visual Novel (No Facade)", sf::Style::Default);
        window.setFramerateLimit(60);
        if (!font.loadFromFile("OpenSans-Regular.ttf")) return;
        generateDefaultBackground(1024, 768);
        currentBgPath = "__intro__"; currentCharPath = "__intro__";
        titleText.setFont(font); titleText.setCharacterSize(52); titleText.setFillColor(sf::Color(255,220,130)); titleText.setOutlineColor(sf::Color(80,40,0)); titleText.setOutlineThickness(3.f); titleText.setString("NovelLlama");
        subtitleText.setFont(font); subtitleText.setCharacterSize(18); subtitleText.setFillColor(sf::Color(180,180,200)); subtitleText.setString("AI-Powered Visual Novel (No Facade)");
        guideText.setFont(font); guideText.setCharacterSize(14); guideText.setFillColor(sf::Color(160,160,180));
        guideText.setString("HOW TO PLAY:\nPress [Tab] to switch between three modes:\n\n  EXPLORE - type where you want to go\n  TALK - type what you want to say to the NPC here\n  ACT - type what you want to do\n\nPress [Enter] to submit your input.\nStart by typing your story idea below!");
        dialogueBox.setFillColor(sf::Color(0,0,0,200));
        speakerLabel.setFont(font); speakerLabel.setCharacterSize(16); speakerLabel.setFillColor(sf::Color(255,220,100)); speakerLabel.setStyle(sf::Text::Bold);
        dialogueText.setFont(font); dialogueText.setCharacterSize(16); dialogueText.setFillColor(sf::Color::White);
        inputBox.setFillColor(sf::Color(40,40,40,230)); inputBox.setOutlineColor(sf::Color(120,120,120)); inputBox.setOutlineThickness(1.f);
        inputText.setFont(font); inputText.setCharacterSize(16); inputText.setFillColor(sf::Color(200,255,200)); inputText.setString("_");
        promptLabel.setFont(font); promptLabel.setCharacterSize(24); promptLabel.setFillColor(sf::Color::White); promptLabel.setOutlineColor(sf::Color::Black); promptLabel.setOutlineThickness(2.f); promptLabel.setString("Enter your story idea and press Enter:");
        statusText.setFont(font); statusText.setCharacterSize(18); statusText.setFillColor(sf::Color::Yellow); statusText.setOutlineColor(sf::Color::Black); statusText.setOutlineThickness(2.f); statusText.setPosition(20.f,20.f); statusText.setString("Image generation: starting sd-webui...");
        modeText.setFont(font); modeText.setCharacterSize(16); modeText.setFillColor(sf::Color(100,255,100)); modeText.setOutlineColor(sf::Color::Black); modeText.setOutlineThickness(1.f);
        updateModeDisplay(); repositionUI();

        while (window.isOpen()) {
            sf::Event event;
            while (window.pollEvent(event)) {
                if (event.type == sf::Event::Closed) window.close();
                if (event.type == sf::Event::Resized) { sf::FloatRect area(0,0,static_cast<float>(event.size.width),static_cast<float>(event.size.height)); window.setView(sf::View(area)); repositionUI(); if (!currentDialogue.empty()) refreshDialogueText(false); }
                if (event.type == sf::Event::MouseWheelScrolled && state == State::PLAYING) { if (event.mouseWheelScroll.delta > 0) scrollOffset = std::max(0, scrollOffset-2); else scrollOffset += 2; refreshDialogueText(false); }
                if (event.type == sf::Event::KeyPressed && !loading) { if (event.key.code == sf::Keyboard::Tab && state == State::PLAYING) cycleMode(); }
                if (event.type == sf::Event::TextEntered && !loading) {
                    if (event.text.unicode == '\r' || event.text.unicode == '\n') { handleInput(); }
                    else if (event.text.unicode == '\b') { if (!userInput.empty()) { utf8PopBack(userInput); std::string d = userInput + "_"; inputText.setString(sf::String::fromUtf8(d.begin(), d.end())); } }
                    else if (event.text.unicode == '\t') {}
                    else if (event.text.unicode >= 32) { userInput += utf8Encode(event.text.unicode); std::string d = userInput + "_"; inputText.setString(sf::String::fromUtf8(d.begin(), d.end())); }
                }
            }
            if (state == State::INTRO) { statusText.setString(imgGenerator.isAvailable() ? "Image generation: ON" : "Image generation: starting sd-webui..."); }
            if (resultReady) { std::lock_guard<std::mutex> lock(resultMutex); updateView(pendingResult); state = State::PLAYING; resultReady = false; updateModeDisplay(); statusText.setString("Your turn:"); }
            window.clear(sf::Color::Black); window.draw(backgroundImg);
            if ((state == State::PLAYING || state == State::LOADING) && characterLoaded && characterVisible) window.draw(characterImg);
            window.draw(dialogueBox);
            if (state == State::INTRO) { window.draw(titleText); window.draw(subtitleText); window.draw(guideText); window.draw(promptLabel); }
            if (state == State::PLAYING) { window.draw(speakerLabel); window.draw(modeText); }
            window.draw(dialogueText); window.draw(inputBox); window.draw(inputText); window.draw(statusText);
            window.display();
        }
        joinWorker();
    }
};
