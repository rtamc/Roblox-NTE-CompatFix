# Roblox-NTE-CompatFix (EN) 
## 한국어는 밑에

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A simple registry toggle utility designed to resolve the **anti-cheat driver (`HtAntiCheatDriver`) conflict** between Roblox and NTE.

Instead of manually opening Command Prompt or PowerShell to modify the registry every time, this lightweight executable lets you switch configurations with a single click.

---

## 🚀 Key Features

* **Roblox Mode:** Changes the `HtAntiCheatDriver` start configuration to `4 (Disabled)` so Roblox can run smoothly without conflicts.
* **NTE Mode:** Restores the `HtAntiCheatDriver` start configuration to `2 (Automatic)` to ensure the NTE anti-cheat functions correctly.

---

## 🛠️ How to Use

> ⚠️ **Prerequisite:** Since this program modifies system registries (`HKLM`), it **MUST be run with Administrator privileges**.

1. Download the latest executable from the [Releases](https://github.com/YOUR_GITHUB_USERNAME/YOUR_REPO_NAME/releases) page.
2. Right-click the downloaded `.exe` file and select **[Run as administrator]**.
3. Select the game you want to play (**Roblox** or **NTE**) to apply the registry fix.
4. **You MUST restart your computer** after applying the change for the driver status to take effect.

---

## 🔍 Under the Hood

This tool simply automates the following standard Windows PowerShell commands safely:

* ** Roblox:**
    ```powershell
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver" -Name "Start" -Value 4
    ```
* ** NTE:**
    ```powershell
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver" -Name "Start" -Value 2
    ```

---

## 🛡️ Safety & False Positives Notice

* This utility only toggles the standard `Start` configuration value of the specified service. It does not install any malicious software or modify other system files.
* Because it accesses a sensitive system registry path (`HKLM`), Windows Defender or other antivirus software may trigger a **False Positive warning**. 
* To ensure full transparency and trust, **the complete source code is fully open and visible** in this repository. You are always welcome to review and compile it yourself.

---

## 📄 License

This project is licensed under the [MIT License](LICENSE) - feel free to modify and distribute.

# Roblox-NTE-CompatFix

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

로블록스(Roblox)와 NTE를 번갈아 플레이할 때 발생하는 **안티치트 드라이버(`HtAntiCheatDriver`) 충돌 문제를 해결**하기 위한 레지스트리 토글 프로그램입니다. 

매번 명령프롬프트(CMD)나 파워쉘을 열어 레지스트리를 수정할 필요 없이, `.exe` 파일 실행 한 번으로 간편하게 설정을 전환할 수 있습니다.

---

## 🚀 주요 기능

*   **Roblox 모드:** `HtAntiCheatDriver` 설정을 `4(중지)`로 변경하여 로블록스가 정상 실행되도록 합니다.
*   **NTE 모드:** `HtAntiCheatDriver` 설정을 `2(자동)`로 변경하여 NTE 안티치트가 정상 작동하도록 합니다.

---

## 🛠️ 사용 방법

> ⚠️ **필수 요구사항:** 레지스트리(`HKLM`)를 수정하는 프로그램이므로 반드시 **관리자 권한**으로 실행해야 합니다.

1. [Releases](https://github.com/YOUR_GITHUB_USERNAME/YOUR_REPO_NAME/releases) 페이지에서 최신 버전의 `.exe` 파일을 다운로드합니다.
2. 다운로드한 프로그램을 마우스 우클릭한 뒤 **[관리자 권한으로 실행]**을 선택합니다.
3. 플레이하려는 게임(**Roblox** 또는 **NTE**)을 선택하여 설정을 변경합니다.
4. **반드시 컴퓨터를 재부팅(다시 시작)**한 뒤 게임을 실행해 주세요. (드라이버 상태를 적용하기 위해 필수적입니다.)

---

## 🔍 실제 작동 원리

이 프로그램은 시스템 내부적으로 아래의 Windows PowerShell 명령어를 안전하게 실행해 주는 역할만 수행합니다.

*   **Roblox 플레이 시:**
```powershell
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver" -Name "Start" -Value 4
    ```
*   **NTE 플레이 시:**
```powershell
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver" -Name "Start" -Value 2
    ```

---

## 🛡️ 안전성 및 오탐지 안내 (False Positive)

*   이 프로그램은 시스템 레지스트리의 `Start` 값만 변경하는 안전한 도구입니다.
*   하지만 Windows의 민감한 레지스트리 영역(`HKLM`)을 수정하기 때문에, Windows Defender나 타 안티바이러스 프로그램에서 **경고(오탐지)**를 띄울 수 있습니다. 
*   불안하신 분들을 위해 본 저장소에 **모든 소스 코드를 투명하게 공개**하고 있으니 직접 검증 후 빌드하여 사용하셔도 좋습니다.

---

## 📄 라이선스

이 프로젝트는 [MIT License](LICENSE)에 따라 자유롭게 수정 및 배포가 가능합니다.
