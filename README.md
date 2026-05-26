# Roblox-NTE-CompatFix (EN) 
## 한국어는 밑에

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

⚠️ Error
Third-party software is interfering with Roblox. If you're using software for exploiting or reverse-engineering, you'll need to uninstall it.

If you have NTE installed on your PC, there is a 98% chance this error is caused by a kernel-level conflict between Roblox's anti-cheat and NTE's anti-cheat driver (HtAntiCheatDriver).

Stop wasting your time typing commands into the prompt every single time. Use this lightweight utility to fix the issue instantly with just a single click.

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

## 🛡️ Malware Scan Results

(virustotal result: https://www.virustotal.com/gui/file/bbf8bcf9cac79d7e3d773c9a9685659107eb1f5016bd177c33e81590c5b6be6d/detection)

(MetaDefender result: (https://metadefender.com/results/file/bzI2MDUyNnZuSVNiZ0FIMV9TQnQ0SmMtTWRs_mdaas))

---

## 📄 References 
https://www.reddit.com/r/RobloxHelp/comments/1t26i9l/nte_roblox_fix/

---

## 📄 License

This project is licensed under the [MIT License](LICENSE) - feel free to modify and distribute.

---

# Roblox-NTE-CompatFix

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

⚠️ Error
Third-party software is interfering with Roblox. If you're using software for exploiting or reverse-engineering, you'll need to uninstall it.
(서드파티 소프트웨어가 Roblox를 방해하고 있습니다. 변조 및 리버스 엔지니어링용 소프트웨어를 사용 중이라면 삭제해야 합니다.)

PC에 이환(NTE)이 설치되어 있다면, 이 오류의 원인은 98% 확률로 NTE의 안티치트 드라이버인 HtAntiCheatDriver와 Roblox 안티치트 간의 커널 레벨 충돌 때문입니다.
귀찮게 매번 명령어를 입력하지 말고 이걸 사용해서 해결 해보세요.


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

* ** 로블럭스:**
    ```powershell
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver" -Name "Start" -Value 4
    ```
* ** 이환:**
    ```powershell
    Set-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Services\HtAntiCheatDriver" -Name "Start" -Value 2
    ```

---

## 🛡️ 안전성 및 오탐지 안내 (False Positive)

*   이 프로그램은 시스템 레지스트리의 `Start` 값만 변경하는 안전한 도구입니다.
*   하지만 Windows의 민감한 레지스트리 영역(`HKLM`)을 수정하기 때문에, Windows Defender나 타 안티바이러스 프로그램에서 **경고(오탐지)**를 띄울 수 있습니다. 
*   불안하신 분들을 위해 본 저장소에 **모든 소스 코드를 투명하게 공개**하고 있으니 직접 검증 후 빌드하여 사용하셔도 좋습니다.

## 🛡️악성 코드 검사 결과
(virustotal 결과: https://www.virustotal.com/gui/file/bbf8bcf9cac79d7e3d773c9a9685659107eb1f5016bd177c33e81590c5b6be6d/detection)
(MetaDefender 결과: (https://metadefender.com/results/file/bzI2MDUyNnZuSVNiZ0FIMV9TQnQ0SmMtTWRs_mdaas))


---

## 📄 참고하였습니다 
https://www.reddit.com/r/RobloxHelp/comments/1t26i9l/nte_roblox_fix/

---


## 📄 라이선스

이 프로젝트는 [MIT License](LICENSE)에 따라 자유롭게 수정 및 배포가 가능합니다.
