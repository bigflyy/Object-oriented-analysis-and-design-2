#pragma once
#include <string>
#include <vector>
#include <fstream>
#include <sstream>
#include <iostream>
#include <algorithm>
#include <windows.h>
#include <SFML/Network.hpp>
#include "json.hpp"
#include "Entity.h"
#include "MemoryManager.h"

using json = nlohmann::json;

// Обёртка над Ollama API — генерация текста, создание мира, классификация.
// Все обращения к языковой модели проходят через этот класс,
// что позволяет централизовать настройки (модель, температура, контекст).
class LLMStoryCreator {
private:
    std::string modelName;
    float temperature;

    // Удаление тегов <think>...</think> из ответа.
    // Некоторые модели (DeepSeek, Qwen) оборачивают вну    тренние рассуждения
    // в эти теги — нам нужен только финальный ответ без них.
    std::string stripThinkingTags(const std::string& text) {
        std::string result = text;
        while (true) {
            size_t startPos = result.find("<think>");
            if (startPos == std::string::npos) break;
            size_t endPos = result.find("</think>", startPos);
            // Если закрывающий тег не найден — обрезаем всё от открывающего до конца
            if (endPos == std::string::npos) { result.erase(startPos); break; }
            result.erase(startPos, endPos - startPos + 8);
        }
        // Убираем ведущие пробельные символы после очистки
        size_t firstNonSpace = result.find_first_not_of(" \t\n\r");
        return (firstNonSpace != std::string::npos) ? result.substr(firstNonSpace) : result;
    }

    // Проверка доступности Ollama через HTTP-запрос к API тегов
    bool checkOllamaAvailable() {
        sf::Http http("127.0.0.1", 11434);
        sf::Http::Request request("/api/tags");
        return http.sendRequest(request, sf::seconds(3)).getStatus() == sf::Http::Response::Ok;
    }

    // Попытка запустить Ollama как фоновый процесс если она ещё не запущена.
    // Ждём до 15 секунд пока сервер станет доступен.
    bool tryStartOllama() {
        STARTUPINFOA startupInfo = { sizeof(startupInfo) };
        startupInfo.dwFlags = STARTF_USESHOWWINDOW;
        startupInfo.wShowWindow = SW_HIDE;
        PROCESS_INFORMATION processInfo;
        char cmd[] = "ollama serve";
        if (CreateProcessA(NULL, cmd, NULL, NULL, FALSE,
                           CREATE_NO_WINDOW | DETACHED_PROCESS, NULL, NULL, &startupInfo, &processInfo)) {
            CloseHandle(processInfo.hThread);
            CloseHandle(processInfo.hProcess);
            for (int i = 0; i < 15; ++i) {
                Sleep(1000);
                if (checkOllamaAvailable()) return true;
            }
        }
        return false;
    }

    // Кодирование файла в base64 для передачи изображений в LLM (vision).
    // Ollama API принимает изображения только в base64-формате.
    std::string fileToBase64(const std::string& path) {
        std::ifstream file(path, std::ios::binary);
        if (!file.is_open()) return "";
        std::vector<unsigned char> data((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());
        if (data.empty()) return "";
        static const char base64Chars[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        std::string result;
        int accumulator = 0, bitsRemaining = -6;
        // Обработка каждого байта: накапливаем биты и извлекаем по 6 для base64-символов
        for (unsigned char byte : data) {
            accumulator = (accumulator << 8) + byte;
            bitsRemaining += 8;
            while (bitsRemaining >= 0) {
                result += base64Chars[(accumulator >> bitsRemaining) & 0x3F];
                bitsRemaining -= 6;
            }
        }
        if (bitsRemaining > -6) result += base64Chars[((accumulator << 8) >> (bitsRemaining + 8)) & 0x3F];
        // Дополнение до кратности 4 символов согласно стандарту base64
        while (result.size() % 4) result += '=';
        return result;
    }

    // HTTP-запрос к Ollama /api/chat (с опциональной передачей изображений).
    // Динамически подбирает num_ctx (размер контекстного окна) по длине промпта,
    // чтобы не тратить VRAM на маленькие запросы.
    std::string ollamaRequest(const std::string& prompt, const std::string& systemPrompt = "",
                              const std::vector<std::string>& imagePaths = {}) {
        sf::Http http("127.0.0.1", 11434);
        json body;
        body["model"] = modelName;
        body["stream"] = false;
        body["think"] = false;
        body["options"]["temperature"] = temperature;
        body["options"]["num_gpu"] = 99;

        // Динамический num_ctx: оценка токенов (~4 символа на токен) + запас, диапазон 4096-32768
        size_t estimatedTokens = (prompt.size() + systemPrompt.size()) / 4 + 512;
        size_t contextSize = std::max((size_t)4096, std::min((size_t)32768, estimatedTokens));
        body["options"]["num_ctx"] = contextSize;

        json messages = json::array();
        if (!systemPrompt.empty())
            messages.push_back({{"role", "system"}, {"content", systemPrompt}});

        // Формирование пользовательского сообщения с прикреплёнными изображениями
        json userMsg = {{"role", "user"}, {"content", prompt}};
        if (!imagePaths.empty()) {
            json images = json::array();
            for (const auto& imagePath : imagePaths) {
                std::string base64Data = fileToBase64(imagePath);
                if (!base64Data.empty()) images.push_back(base64Data);
            }
            if (!images.empty()) userMsg["images"] = images;
        }
        messages.push_back(userMsg);
        body["messages"] = messages;

        sf::Http::Request request("/api/chat", sf::Http::Request::Post);
        request.setField("Content-Type", "application/json");
        request.setBody(body.dump());

        sf::Http::Response response = http.sendRequest(request, sf::seconds(120));
        if (response.getStatus() != sf::Http::Response::Ok)
            return "[LLM Error: " + std::to_string(response.getStatus()) + "]";
        try {
            return stripThinkingTags(json::parse(response.getBody())["message"]["content"].get<std::string>());
        } catch (...) {
            return "[Parse Error]";
        }
    }

public:
    LLMStoryCreator(const std::string& model = "qwen3.5:4b", float temp = 0.8f)
        : modelName(model), temperature(temp) {
        if (!checkOllamaAvailable()) {
            std::cout << "[LLM] Ollama не запущена, запускаю..." << std::endl;
            tryStartOllama();
        }
        std::cout << "[LLM] Готово (модель: " << modelName << ")" << std::endl;
    }

    // Произвольный запрос к LLM
    std::string request(const std::string& prompt, const std::string& sys = "") {
        return ollamaRequest(prompt, sys);
    }

    // Создание начального мира: 2 локации + 2 NPC.
    // LLM генерирует JSON-массив сущностей, который мы парсим.
    // Если парсинг не удался — возвращаем пустой вектор.
    std::vector<Entity> createInitialWorld(const std::string& plot) {
        std::string sys =
            "Create exactly 4 entities for a visual novel: 2 locations and 2 NPCs.\n"
            "Respond with ONLY a JSON array. Each element: {\"name\", \"description\", \"type\": \"location\" or \"NPC\"}.\n"
            "Descriptions are for an IMAGE GENERATOR, not for narration.\n"
            "DO NOT start with 'You stand' or 'You see' — describe the scene itself, not the player.\n"
            "Location descriptions: what the place looks like from inside. Objects, walls, lighting, colors.\n"
            "NPC descriptions: appearance, clothing, expression, pose.\n"
            "Output ONLY the JSON array, nothing else.";
        std::string raw = ollamaRequest("Plot: " + plot, sys);
        std::cout << "[LLM] createInitialWorld: " << raw.substr(0, 200) << std::endl;

        // Ищем JSON-массив в ответе — LLM может добавить текст до/после массива
        size_t jsonStart = raw.find('['), jsonEnd = raw.rfind(']');
        std::vector<Entity> result;
        if (jsonStart == std::string::npos || jsonEnd == std::string::npos) return result;
        try {
            json arr = json::parse(raw.substr(jsonStart, jsonEnd - jsonStart + 1));
            for (auto& element : arr)
                result.emplace_back(element.value("name","Unknown"), element.value("description",""), element.value("type","NPC"));
        } catch (const std::exception& ex) {
            std::cout << "[LLM] Ошибка парсинга JSON: " << ex.what() << std::endl;
        }
        return result;
    }

    // EXPLORE: визуальное описание места для генерации изображения (с контекстом сюжета)
    std::string describePlace(const std::string& memory, const std::string& placeName) {
        return ollamaRequest(memory + "\n\nDescribe visually for an image generator the place: " + placeName,
            "Describe this place for an image generator. 1-2 sentences. English only. "
            "Use the story context to make the description match the setting and atmosphere. "
            "DO NOT start with 'You stand' or 'You see' — describe the scene itself. "
            "Interior view from inside. Walls, floor, objects, lighting, colors, atmosphere. "
            "NO people, NO characters, NO pronouns like 'you' or 'your'.");
    }

    // EXPLORE: определяет кто находится в данной локации.
    // Возвращает имя персонажа или пустую строку если никого нет.
    // Ответ LLM нормализуется: проверяются варианты "no", "none", "nobody" и т.д.
    std::string whoIsHere(const std::string& memory, const std::string& locationName) {
        std::string raw = ollamaRequest(
            memory + "\n\nThe player just arrived at: " + locationName +
            "\nIs there a character (NPC) present at this location? "
            "Reply with ONLY the character's name, or NO if nobody is there.",
            "Reply with ONLY a character name or NO. Nothing else. "
            "Pick from existing characters in context if appropriate.");

        // Приведение ответа к нижнему регистру для нечёткой проверки на "нет"
        std::string lower = toLower(raw);
        size_t firstNonSpace = lower.find_first_not_of(" \t\n\r\"");
        size_t lastNonSpace = lower.find_last_not_of(" \t\n\r.\"");
        if (firstNonSpace == std::string::npos) return "";
        lower = lower.substr(firstNonSpace, lastNonSpace - firstNonSpace + 1);

        // Проверяем все варианты отрицательного ответа
        if (lower == "no" || lower == "none" || lower == "no one" || lower == "nobody" || lower.find("no ") == 0)
            return "";

        // Извлекаем чистое имя из оригинального (не нижнего) регистра
        firstNonSpace = raw.find_first_not_of(" \t\n\r\"");
        lastNonSpace = raw.find_last_not_of(" \t\n\r.\"");
        return (firstNonSpace != std::string::npos) ? raw.substr(firstNonSpace, lastNonSpace - firstNonSpace + 1) : "";
    }

    // EXPLORE: нарратив прибытия (с учётом ухода/появления/следования персонажей)
    std::string narrateArrival(const std::string& memory, const std::string& locationName,
                                const std::string& npcName = "", const std::string& previousNpc = "",
                                bool npcFollowed = false, const std::vector<std::string>& images = {}) {
        std::string context = "\n\nThe player arrives at " + locationName + ".";
        if (npcFollowed && !npcName.empty())
            context += " " + npcName + " came with you — they followed you here. Describe them walking alongside you into this new place.";
        else {
            if (!previousNpc.empty())
                context += " " + previousNpc + " is no longer with you — describe how they parted or stayed behind.";
            if (!npcName.empty())
                context += " You encounter " + npcName + " here — describe how they appear and what they are doing. This is someone you find at this location.";
        }
        return ollamaRequest(memory + context,
            "You are a visual novel narrator. Write 5-8 vivid sentences in second person. "
            "Describe the new location and what the player sees. "
            "If images are provided, use them to describe the scene accurately. "
            "If a companion followed you, mention them being by your side. "
            "If a previous companion left, briefly explain how they departed. "
            "If a new character is present, describe how the player notices and encounters them. "
            "Respond with ONLY the text.", images);
    }

    // TALK: прямая речь NPC (только диалог, без описания действий)
    std::string dialogue(const std::string& memory, const std::string& npcName,
                         const std::string& playerInput, const std::vector<std::string>& images = {}) {
        return ollamaRequest(memory + "\n\nThe player says to you: " + playerInput,
            "You are " + npcName + " in a visual novel. Write ONLY direct speech — the words " + npcName + " says out loud. "
            "5-8 sentences of pure dialogue. "
            "DO NOT describe actions, gestures, movements, or body language. "
            "DO NOT write narration like 'I <do something>' or 'she <does something>'. "
            "Respond with ONLY the spoken words from " + npcName, images);
    }

    // ACT: нарратив действия игрока
    std::string narrateAction(const std::string& memory, const std::string& playerInput,
                               const std::vector<std::string>& images = {}) {
        return ollamaRequest(memory + "\n\nThe player does: " + playerInput,
            "You are a visual novel narrator. Write 5-8 vivid sentences in second person. "
            "If images are provided, use them to describe the scene accurately. "
            "React DIRECTLY to what the player did. Respond with ONLY the text.", images);
    }

    // ACT: дополнение нарратива при появлении/уходе персонажа.
    // Вызывается после классификации, чтобы текст объяснял визуальное изменение.
    std::string narrateTransition(const std::string& memory, const std::string& narration,
                                   const std::string& appeared, const std::string& left) {
        std::string context = memory + "\n\nWhat just happened:\n" + narration + "\n\n";
        if (!left.empty()) context += left + " leaves the scene. ";
        if (!appeared.empty()) context += appeared + " appears. ";
        context += "Write 2-3 sentences describing this transition.";
        return ollamaRequest(context,
            "You are a visual novel narrator. Write 2-3 sentences in second person. "
            "Describe HOW the character leaves or appears — their movement, expression, entrance. "
            "Respond with ONLY the text.");
    }

    // ACT: классификация — что изменилось после действия игрока?
    // Возвращает одно из трёх значений: "APPEAR", "LEAVE", "NOTHING".
    // Результат управляет логикой появления/ухода персонажей в AdventureFacade.
    std::string classifyChange(const std::string& memory, const std::string& narration) {
        std::string raw = ollamaRequest(
            memory + "\n\nWhat just happened:\n" + narration +
            "\n\nDid a character APPEAR, LEAVE, or NOTHING changed? Reply with ONE word only: APPEAR, LEAVE, or NOTHING.",
            "Reply with exactly ONE word: APPEAR, LEAVE, or NOTHING. No explanation.");
        std::string lower = toLower(raw);
        if (lower.find("appear") != std::string::npos) return "APPEAR";
        if (lower.find("leave") != std::string::npos) return "LEAVE";
        return "NOTHING";
    }

    // ACT: извлечение имени нового персонажа из нарратива.
    // Обрезает кавычки и пробельные символы из ответа LLM.
    std::string nameNewCharacter(const std::string& memory, const std::string& narration) {
        std::string raw = ollamaRequest(
            memory + "\n\n" + narration +
            "\n\nA new character appeared. What is their name? Reply with ONLY the name, 1-3 words.",
            "Reply with ONLY the character name. Nothing else.");
        size_t firstNonSpace = raw.find_first_not_of(" \t\n\r\"");
        size_t lastNonSpace = raw.find_last_not_of(" \t\n\r.\"");
        return (firstNonSpace != std::string::npos) ? raw.substr(firstNonSpace, lastNonSpace - firstNonSpace + 1) : "Stranger";
    }

    // Визуальное описание персонажа для генерации изображения (с контекстом сюжета)
    std::string describeCharacter(const std::string& memory, const std::string& name) {
        return ollamaRequest(memory + "\n\nDescribe visually for an image generator the character: " + name,
            "Describe this character for an image generator. 1-2 sentences. English only. "
            "Use the story context to match their role, status, and setting. "
            "Include: hair, clothing, expression, pose. No background.");
    }

    // Сжатие истории: LLM суммирует список событий в 2-3 предложения
    std::string compactHistory(const std::string& events) {
        return ollamaRequest(events, "Summarize these story events into 2-3 sentences. Reply with ONLY the summary.");
    }

    // Компактификация памяти: сжатие старых событий сущностей когда память превышает лимит.
    // LLM суммирует старые записи, оставляя только 2 последних в history.
    void compactMemory(MemoryManager& memory, size_t maxSize = 240000) {
        if (memory.totalMemorySize() <= maxSize) return;
        for (auto& [entityName, entity] : memory.entities) {
            if (entity.history.size() <= 2) continue;
            std::string context = entity.summary.empty() ? "" : "Previous: " + entity.summary + "\n";
            for (size_t i = 0; i < entity.history.size() - 2; ++i)
                context += "- " + entity.history[i] + "\n";
            entity.summary = compactHistory(context);
            entity.history = std::vector<std::string>(entity.history.end() - 2, entity.history.end());
        }
    }

    // Выгрузка модели из VRAM для освобождения памяти перед генерацией изображений.
    // Отправляет keep_alive=0 на оба эндпоинта (/api/chat и /api/generate),
    // потому что Ollama может держать модель загруженной через любой из них.
    // 3 секунды ожидания нужны чтобы VRAM реально освободилась.
    void unloadFromVRAM() {
        std::cout << "[LLM] Выгрузка из VRAM..." << std::endl;
        sf::Http http("127.0.0.1", 11434);
        json chatBody;
        chatBody["model"] = modelName; chatBody["messages"] = json::array(); chatBody["keep_alive"] = "0";
        sf::Http::Request chatRequest("/api/chat", sf::Http::Request::Post);
        chatRequest.setField("Content-Type", "application/json"); chatRequest.setBody(chatBody.dump());
        http.sendRequest(chatRequest, sf::seconds(15));
        json generateBody;
        generateBody["model"] = modelName; generateBody["keep_alive"] = "0";
        sf::Http::Request generateRequest("/api/generate", sf::Http::Request::Post);
        generateRequest.setField("Content-Type", "application/json"); generateRequest.setBody(generateBody.dump());
        http.sendRequest(generateRequest, sf::seconds(10));
        Sleep(3000); // нужно 3 секунды чтобы VRAM реально освободилась
        std::cout << "[LLM] Выгружено." << std::endl;
    }
};
