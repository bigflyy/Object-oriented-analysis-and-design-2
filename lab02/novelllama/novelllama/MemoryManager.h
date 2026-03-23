#pragma once
#include <string>
#include <vector>
#include <map>
#include "Entity.h"

// Менеджер памяти: централизованное хранилище всего игрового состояния.
// Хранит сюжет, все сущности (NPC и локации), текущее положение игрока,
// а также глобальные события. Предоставляет нечёткий поиск и формирует
// полный контекст для LLM.
class MemoryManager {
public:
    std::string plot;
    std::string currentLocation;
    std::string currentCharacterName;
    std::vector<std::string> globalEvents;
    std::map<std::string, Entity> entities;

    void addEntity(const Entity& entity) { entities[entity.name] = entity; }

    void addHistory(const std::string& entityName, const std::string& event) {
        if (entities.count(entityName))
            entities[entityName].history.push_back(event);
    }

    void addGlobalEvent(const std::string& event) { globalEvents.push_back(event); }

    // Нечёткий поиск сущности по имени.
    // Стратегия поиска идёт от точного к приблизительному:
    // 1) точное совпадение ключа в map,
    // 2) без учёта регистра,
    // 3) подстрока в любом направлении,
    // 4) поиск по значимым словам (длиннее 3 символов).
    // Это нужно потому что LLM может вернуть имя в другом регистре или формулировке.
    Entity* findByName(const std::string& name) {
        // Шаг 1: точное совпадение — самый быстрый путь
        auto it = entities.find(name);
        if (it != entities.end()) return &it->second;

        // Шаг 2-3: приведение к нижнему регистру для нечёткого сравнения
        // Шаг 2: без учёта регистра + подстрока в любом направлении
        std::string nameLower = toLower(name);
        for (auto& [entityName, entity] : entities) {
            std::string entityNameLower = toLower(entityName);
            if (entityNameLower == nameLower
                // is the query inside the entity name. "cave" found inside "dark cave" -> match.
                || entityNameLower.find(nameLower) != std::string::npos
                // is the entity name inside the query. "dark cave" found inside "the dark cave near the river" -> match.  
                || nameLower.find(entityNameLower) != std::string::npos)
                return &entity;
        }
        return nullptr;
    }

    // Формирование полного текстового контекста для LLM.
    // Включает сюжет, текущее состояние, глобальные события и все сущности.
    // Этот промпт подаётся в каждый запрос к языковой модели,
    // чтобы она помнила всю историю игры.
    std::string getGlobalMemoryPrompt() const {
        std::string prompt;
        prompt += "== PLOT ==\n" + plot + "\n\n";
        prompt += "== CURRENT STATE ==\n";
        if (!currentLocation.empty()) prompt += "Location: " + currentLocation + "\n";
        if (!currentCharacterName.empty()) prompt += "Talking to: " + currentCharacterName + "\n";
        prompt += "\n";
        prompt += "== ENTITIES ==\n";
        for (const auto& [entityName, entity] : entities) prompt += entity.getMemoryPrompt() + "\n";
        if (!globalEvents.empty()) {
            prompt += "== EVENTS ==\n";
            for (const auto& event : globalEvents) prompt += "- " + event + "\n";
            prompt += "\n";
        }
        return prompt;
    }

    // Подсчёт общего размера памяти в байтах.
    // Вызывается перед каждым ходом для проверки необходимости компактификации.
    size_t totalMemorySize() const {
        size_t total = plot.size() + currentLocation.size() + currentCharacterName.size();
        for (const auto& event : globalEvents) total += event.size();
        for (const auto& [entityName, entity] : entities) total += entity.totalSize();
        return total;
    }
};
