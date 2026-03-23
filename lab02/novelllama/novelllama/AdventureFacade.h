#pragma once
#include <string>
#include <vector>
#include <iostream>
#include <windows.h>
#include "LLMStoryCreator.h"
#include "ImageClient.h"
#include "MemoryManager.h"

// Режимы игры: EXPLORE (перемещение), TALK (диалог), ACT (действие)
enum class GameMode { EXPLORE, TALK, ACT };

// Результат хода — всё что нужно клиенту для отображения
struct FacadeResult {
    std::string text;
    std::string characterImagePath;  // пустая строка = нет персонажа
    std::string backgroundImagePath;
    std::string speaker;
};

// Фасад: единый интерфейс для GameWindowClient.
// Координирует вызовы между LLMStoryCreator, ImageClient, MemoryManager.
// Не содержит игровой логики — только порядок вызовов и передача данных.
class AdventureFacade {
private:
    LLMStoryCreator llm;
    ImageClient imgGenerator;
    MemoryManager memoryManager;

    std::string currentBackground = "background.png";
    std::string currentCharacter = "character.png";

    // Путь к текущему персонажу или пустая строка если персонажа нет
    std::string characterPath() {
        return memoryManager.currentCharacterName.empty() ? "" : currentCharacter;
    }

    // Текущие изображения сцены для передачи в LLM (vision)
    std::vector<std::string> currentImages() {
        std::vector<std::string> images;
        if (currentBackground != "background.png") images.push_back(currentBackground);
        if (currentCharacter != "character.png" && !memoryManager.currentCharacterName.empty())
            images.push_back(currentCharacter);
        return images;
    }

    // Получить фон локации: из кэша или сгенерировать
    std::string resolveBackground(const std::string& name, const std::string& visual) {
        Entity* cached = memoryManager.findByName(name);
        if (cached && !cached->imagePath.empty() && cached->type == "location")
            return cached->imagePath;
        // Генерация нового фона: выгрузка LLM -> sd-webui
        llm.unloadFromVRAM();
        if (!imgGenerator.isAvailable()) return currentBackground;
        std::string image = imgGenerator.generateImage(visual, "", 768, 768, false);
        if (image.empty()) return currentBackground;
        Entity location(name, visual, "location");
        location.imagePath = image;
        memoryManager.addEntity(location);
        return image;
    }

    // Получить персонажа: из кэша или сгенерировать
    std::string resolveCharacter(const std::string& name) {
        Entity* cached = memoryManager.findByName(name);
        if (cached && !cached->imagePath.empty() && cached->type == "NPC") {
            memoryManager.currentCharacterName = cached->name;
            return cached->imagePath;
        }
        // Генерация: описание через LLM -> выгрузка -> sd-webui
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

    // EXPLORE: LLM описание -> ImageClient фон -> LLM кто здесь -> ImageClient персонаж -> LLM нарратив
    FacadeResult explore(const std::string& destination) {
        llm.compactMemory(memoryManager);
        std::string previousNpc = memoryManager.currentCharacterName;

        // Фон локации
        Entity* cached = memoryManager.findByName(destination);
        std::string locationName;
        if (cached && cached->type == "location") {
            locationName = cached->name;
            if (!cached->imagePath.empty()) currentBackground = cached->imagePath;
        }
        else {
            std::string visual = llm.describePlace(memoryManager.getGlobalMemoryPrompt(), destination);
            locationName = destination;
            currentBackground = resolveBackground(destination, visual);
        }

        // Обновление состояния
        memoryManager.currentLocation = locationName;
        memoryManager.currentCharacterName = "";
        memoryManager.addGlobalEvent("Traveled to: " + locationName);

        // Кто здесь?
        std::string whoHere = llm.whoIsHere(memoryManager.getGlobalMemoryPrompt(), locationName);
        if (!whoHere.empty())
            currentCharacter = resolveCharacter(whoHere);

        // Событие: предыдущий NPC последовал или остался
        bool followed = (!previousNpc.empty() && !memoryManager.currentCharacterName.empty()
            && memoryManager.findByName(previousNpc) == memoryManager.findByName(memoryManager.currentCharacterName));
        if (!previousNpc.empty())
            memoryManager.addGlobalEvent(previousNpc + (followed ? " followed to " + locationName : " stayed behind"));
        if (!memoryManager.currentCharacterName.empty() && !followed)
            memoryManager.addGlobalEvent("Met: " + memoryManager.currentCharacterName);

        // Нарратив прибытия
        std::string narration = llm.narrateArrival(memoryManager.getGlobalMemoryPrompt(),
            locationName, memoryManager.currentCharacterName, followed ? "" : previousNpc, followed, currentImages());
        return { narration, characterPath(), currentBackground, "Narrator" };
    }

    // TALK: LLM диалог -> MemoryManager запись
    FacadeResult talk(const std::string& playerInput) {
        llm.compactMemory(memoryManager);
        if (memoryManager.currentCharacterName.empty())
            return { "There is no one here to talk to. Use EXPLORE to find someone.",
                     "", currentBackground, "Narrator" };

        std::string npcName = memoryManager.currentCharacterName;
        std::string dialogueText = llm.dialogue(memoryManager.getGlobalMemoryPrompt(), npcName, playerInput, currentImages());
        memoryManager.addHistory(npcName, "Player: " + playerInput);
        memoryManager.addHistory(npcName, npcName + " responded");
        return { dialogueText, characterPath(), currentBackground, npcName };
    }

    // ACT: LLM нарратив -> LLM классификация -> ImageClient (если APPEAR)
    FacadeResult act(const std::string& playerInput) {
        llm.compactMemory(memoryManager);

        // Нарратив действия
        std::string narration = llm.narrateAction(memoryManager.getGlobalMemoryPrompt(), playerInput, currentImages());
        memoryManager.addGlobalEvent("Player: " + playerInput);

        // Классификация последствий
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
 
    const MemoryManager& getMemory() const { return memoryManager; }

public:
    AdventureFacade() = default;

    // Инициализация мира: LLM сюжет -> LLM сущности -> ImageClient изображения -> LLM нарратив
    FacadeResult start(const std::string& userIdea) {
        // LLM: сюжет
        memoryManager.plot = llm.request(
            "Create a 2-3 sentence plot for a second-person visual novel. Use 'you'. Based on: " + userIdea,
            "Respond with just the plot. No extra text. "
            "Be creative and specific to the user's idea. DO NOT use themes about fading memories, "
            "forgotten pasts, or lost identities unless the user specifically asked for that. "
            "Focus on concrete goals: find something, save someone, defeat an enemy, escape a place, solve a mystery.");

        // LLM: начальные сущности
        auto entities = llm.createInitialWorld(memoryManager.plot);

        // ImageClient: изображения для каждой сущности
        llm.unloadFromVRAM();
        for (int i = 0; i < 60 && !imgGenerator.isAvailable(); ++i) Sleep(1000);
        for (auto& entity : entities) {
            bool isNpc = (toLower(entity.type) == "npc");
            std::string image = imgGenerator.generateImage(entity.description, "", isNpc ? 512 : 768, 768, isNpc);
            if (!image.empty()) entity.imagePath = image;
            memoryManager.addEntity(entity);
        }

        // MemoryManager: выбор начальной сцены
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

        // LLM: нарратив прибытия
        std::string intro = llm.narrateArrival(memoryManager.getGlobalMemoryPrompt(),
            memoryManager.currentLocation, memoryManager.currentCharacterName);
        memoryManager.addGlobalEvent("Story began: " + userIdea);
        return { intro, characterPath(), currentBackground, "Narrator" };
    }

    // Диспетчер режимов
    FacadeResult processTurn(const std::string& userInput, GameMode mode) {
        switch (mode) {
            case GameMode::EXPLORE: return explore(userInput);
            case GameMode::TALK:    return talk(userInput);
            case GameMode::ACT:     return act(userInput);
        }
        return act(userInput);
    }

    bool isImageGenAvailable() const { return imgGenerator.isAvailable(); }
};
