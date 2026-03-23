#pragma once
#include <string>
#include <vector>
#include <fstream>
#include <thread>
#include <atomic>
#include <iostream>
#include <windows.h>
#include <SFML/Network.hpp>
#include <SFML/Graphics.hpp>
#include "json.hpp"

using json = nlohmann::json;

// Клиент генерации изображений через sd-webui-forge-neo API.
// Запускает sd-webui в фоне при инициализации и предоставляет
// метод generateImage для создания фонов (768x768) и персонажей (512x768).
class ImageClient {
private:
    std::string apiHost;
    int apiPort;
    std::atomic<bool> available;
    std::thread startupThread;
    static int imgCounter;

    // Декодирование base64-строки в массив байтов.
    // Используется для сохранения сгенерированных изображений из ответа API.
    std::vector<unsigned char> base64Decode(const std::string& encoded) {
        static const std::string base64Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        std::vector<unsigned char> decoded;
        int accumulator = 0, bitsRemaining = -8;
        for (char character : encoded) {
            if (character == '=' || character == '\n' || character == '\r' || character == ' ') continue;
            size_t pos = base64Chars.find(character);
            if (pos == std::string::npos) continue;
            accumulator = (accumulator << 6) + static_cast<int>(pos);
            bitsRemaining += 6;
            if (bitsRemaining >= 0) {
                decoded.push_back(static_cast<unsigned char>((accumulator >> bitsRemaining) & 0xFF));
                bitsRemaining -= 8;
            }
        }
        return decoded;
    }

    // Проверка доступности sd-webui через запрос списка моделей
    bool checkAvailability() {
        sf::Http http(apiHost, static_cast<unsigned short>(apiPort));
        sf::Http::Request request("/sdapi/v1/sd-models");
        return http.sendRequest(request, sf::seconds(2)).getStatus() == sf::Http::Response::Ok;
    }

    // Фоновый запуск sd-webui если не запущен.
    // Ждём до 60 секунд — загрузка модели Stable Diffusion занимает время.
    void startSdWebuiBackground() {
        if (checkAvailability()) { available = true; return; }
        STARTUPINFOA startupInfo = { sizeof(startupInfo) };
        startupInfo.dwFlags = STARTF_USESHOWWINDOW;
        startupInfo.wShowWindow = SW_MINIMIZE;
        PROCESS_INFORMATION processInfo;
        char cmd[] = "C:\\MyProjects\\sd-webui-forge-neo\\venv\\Scripts\\python.exe "
                     "launch.py --api --nowebui --port 7861 --skip-prepare-environment --skip-install "
                     "--cuda-malloc --pin-shared-memory";
        if (CreateProcessA(NULL, cmd, NULL, NULL, FALSE, CREATE_NEW_CONSOLE, NULL,
                           "C:\\MyProjects\\sd-webui-forge-neo", &startupInfo, &processInfo)) {
            CloseHandle(processInfo.hThread);
            CloseHandle(processInfo.hProcess);
            for (int i = 0; i < 60; ++i) {
                Sleep(1000);
                if (checkAvailability()) { available = true; return; }
            }
        }
    }

    // Формирование безопасного имени файла из произвольной строки
    std::string makeFilename(const std::string& base) {
        std::string sanitized;
        for (char character : base) {
            if ((character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z') ||
                (character >= '0' && character <= '9') || character == '-' || character == '_') sanitized += character;
            else if (character == ' ') sanitized += '_';
        }
        return (sanitized.empty() ? "img" : sanitized) + "_" + std::to_string(++imgCounter) + ".png";
    }

public:
    ImageClient(const std::string& host = "127.0.0.1", int port = 7861)
        : apiHost(host), apiPort(port), available(false) {
        // Запуск sd-webui в отдельном потоке чтобы не блокировать инициализацию игры
        startupThread = std::thread([this]() { startSdWebuiBackground(); });
    }

    ~ImageClient() {
        if (startupThread.joinable()) startupThread.join();
    }

    // Генерация изображения: фон (768x768) или персонаж (512x768).
    // Добавляет стилевые промпты в зависимости от типа (фон/персонаж).
    // Сохраняет результат в файл и нормализует размер через SFML.
    std::string generateImage(const std::string& description, const std::string& saveName = "",
                              int width = 768, int height = 768, bool isCharacter = false) {
        if (!available) return "";
        std::string filename = saveName.empty() ? makeFilename("gen") : saveName;

        // Формирование промптов: стилевые добавки зависят от типа изображения
        std::string prompt, negPrompt;
        if (isCharacter) {
            prompt = description +
                ", single character, full body portrait, standing pose, "
                "anime style, visual novel character, simple dark background, clean lineart, high quality";
            negPrompt = "bad quality, blurry, text, watermark, multiple characters";
            width = 512; height = 768;
        } else {
            prompt = description +
                ", first person point of view, interior view, standing inside looking around, "
                "visual novel background, detailed scenery, atmospheric, high quality";
            negPrompt = "people, characters, players bad quality, blurry, text, watermark, " 
                        "aerial view, birds eye view, from above, legs, person, player, foot, "
                        "feet, parts of body, boots, hands";
        }

        sf::Http http(apiHost, static_cast<unsigned short>(apiPort));
        json body;
        body["prompt"] = prompt;
        body["negative_prompt"] = negPrompt;
        body["steps"] = 4;
        body["width"] = width;
        body["height"] = height;
        body["cfg_scale"] = 1.0;
        body["distilled_cfg_scale"] = 3.5;
        body["sampler_name"] = "Euler";
        body["scheduler"] = "simple";

        sf::Http::Request request("/sdapi/v1/txt2img", sf::Http::Request::Post);
        request.setField("Content-Type", "application/json");
        request.setBody(body.dump());

        std::cout << "[IMG] Генерация " << (isCharacter ? "персонажа" : "фона")
                  << ": " << description.substr(0, 60) << "..." << std::endl;

        sf::Http::Response response = http.sendRequest(request, sf::seconds(120));
        if (response.getStatus() != sf::Http::Response::Ok) {
            std::cout << "[IMG] HTTP ошибка: " << response.getStatus() << std::endl;
            std::cout << "[IMG] Тело ответа: " << response.getBody().substr(0, 300) << std::endl;
            return "";
        }

        try {
            json jsonResponse = json::parse(response.getBody());
            std::string base64Data = jsonResponse["images"][0].get<std::string>();
            auto decodedBytes = base64Decode(base64Data);

            // Сохраняем во временный файл, затем нормализуем размер через SFML
            std::string tempName = "temp_" + filename;
            {
                std::ofstream tempFile(tempName, std::ios::binary);
                if (!tempFile.is_open()) return "";
                tempFile.write(reinterpret_cast<const char*>(decodedBytes.data()), decodedBytes.size());
            }

            // Нормализация размера через SFML — sd-webui может вернуть изображение другого размера
            sf::Image image;
            if (!image.loadFromFile(tempName)) { std::remove(tempName.c_str()); return ""; }
            sf::RenderTexture renderTarget;
            if (renderTarget.create(width, height)) {
                sf::Texture texture; texture.loadFromImage(image);
                sf::Sprite sprite(texture);
                sprite.setScale(static_cast<float>(width) / texture.getSize().x,
                                static_cast<float>(height) / texture.getSize().y);
                renderTarget.clear(sf::Color::Transparent); renderTarget.draw(sprite); renderTarget.display();
                renderTarget.getTexture().copyToImage().saveToFile(filename);
            } else { image.saveToFile(filename); }

            std::remove(tempName.c_str());
            std::cout << "[IMG] Сохранено: " << filename << std::endl;
            return filename;
        } catch (const std::exception& ex) {
            std::cout << "[IMG] Ошибка: " << ex.what() << std::endl;
            return "";
        } catch (...) {
            std::cout << "[IMG] Неизвестная ошибка" << std::endl;
            return "";
        }
    }

    bool isAvailable() const { return available; }
};

int ImageClient::imgCounter = 0;
