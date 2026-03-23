// БЕЗ паттерна Фасад: GameWindowClientNoFacade напрямую вызывает все подсистемы.
// Тот же GUI, тот же результат — но вся координация лежит на клиенте.
// Сравните с Source.cpp где клиент использует только AdventureFacade.
#include "GameWindowClientNoFacade.h"
#include <iostream>
#include <windows.h>
#include <tlhelp32.h>

void killProcessesByName(const wchar_t* name) {
    HANDLE snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (snapshot == INVALID_HANDLE_VALUE) return;
    PROCESSENTRY32W processEntry; processEntry.dwSize = sizeof(processEntry);
    if (Process32FirstW(snapshot, &processEntry)) {
        do {
            if (_wcsicmp(processEntry.szExeFile, name) == 0) {
                HANDLE proc = OpenProcess(PROCESS_TERMINATE, FALSE, processEntry.th32ProcessID);
                if (proc) { TerminateProcess(proc, 0); CloseHandle(proc); }
            }
        } while (Process32NextW(snapshot, &processEntry));
    }
    CloseHandle(snapshot);
}

void freeAllVRAM() {
    sf::Http http("127.0.0.1", 11434);
    nlohmann::json body;
    body["model"] = "qwen3.5:4b"; body["keep_alive"] = 0;
    sf::Http::Request request("/api/generate", sf::Http::Request::Post);
    request.setField("Content-Type", "application/json"); request.setBody(body.dump());
    http.sendRequest(request, sf::seconds(5));
    killProcessesByName(L"python.exe");
    Sleep(2000);
}

int main() {
    SetConsoleOutputCP(65001);
    SetConsoleCP(65001);
    freeAllVRAM();
    std::cout << "Запуск NovelLlama (БЕЗ паттерна Фасад)..." << std::endl;
    GameWindowClientNoFacade game;
    game.run();
    freeAllVRAM();
    return 0;
}
