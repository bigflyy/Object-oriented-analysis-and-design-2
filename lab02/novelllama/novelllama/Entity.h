#pragma once
#include <string>
#include <vector>
#include <algorithm>

// Приведение строки к нижнему регистру
inline std::string toLower(const std::string& text) {
    std::string result = text;
    std::transform(result.begin(), result.end(), result.begin(),
                   [](unsigned char letter) { return static_cast<char>(tolower(letter)); });
    return result;
}

// Сущность игрового мира: локация или NPC.
// Каждая сущность хранит свою историю событий и краткое резюме прошлого,
// чтобы LLM мог учитывать контекст при генерации текста.
class Entity {
public:
    std::vector<std::string> history;   // недавние события, связанные с этой сущностью
    std::string summary;                // сжатая старая история (результат компактификации)
    std::string name;
    std::string description;
    std::string type;                   // "NPC" или "location"
    std::string imagePath;

    Entity() = default;
    Entity(const std::string& name, const std::string& description, const std::string& type)
        : name(name), description(description), type(type) {}

    // Формирование текстового блока для контекста LLM.
    // Объединяет имя, тип, описание, прошлое резюме и недавние события,
    // чтобы языковая модель имела полную картину о данной сущности.
    std::string getMemoryPrompt() const {
        std::string prompt = name + " (" + type + "): " + description + "\n";
        if (!summary.empty())
            prompt += "Past: " + summary + "\n";
        if (!history.empty()) {
            prompt += "Recent:\n";
            for (const auto& event : history)
                prompt += "- " + event + "\n";
        }
        return prompt;
    }

    // Подсчёт общего размера текстовых данных сущности в байтах.
    // Используется MemoryManager для определения момента компактификации —
    // когда суммарный объём памяти превышает лимит, старые события сжимаются.
    size_t totalSize() const {
        size_t total = name.size() + description.size() + summary.size();
        for (const auto& event : history) total += event.size();
        return total;
    }
};
