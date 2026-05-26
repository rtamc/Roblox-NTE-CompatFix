@echo off
chcp 65001 > nul
set "csc=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist "%csc%" (
    echo [ERROR] C# 컴파일러(csc.exe)를 찾을 수 없습니다.
    echo C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe 경로를 확인해 주세요.
    pause
    exit /b 1
)

echo.
echo ==============================================
echo [NTE ^& ROBLOX MULTI PATCHER 컴파일 시작]
echo ==============================================
echo.

"%csc%" /nologo /target:winexe /out:NteRobloxPatch.exe /win32manifest:app.manifest Program.cs

if %errorlevel% equ 0 (
    echo.
    echo ==============================================
    echo [성공] 컴파일이 완료되었습니다!
    echo 생성된 파일: NteRobloxPatch.exe
    echo ==============================================
) else (
    echo.
    echo ==============================================
    echo [실패] 컴파일 도중 오류가 발생했습니다.
    echo ==============================================
)
pause
