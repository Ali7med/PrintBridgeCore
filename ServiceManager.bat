@echo off
setlocal enabledelayedexpansion
set "APP_NAME=PrintBridge Core"
set "SERVICE_NAME=PrintBridgeCore"
set "EXE_NAME=PrinterServer.Api.exe"

:: Check for Administrator privileges
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"
if '%errorlevel%' neq '0' (
    echo.
    echo [!] Requesting Admin privileges...
    goto UACPrompt
) else ( goto gotAdmin )

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
    "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    if exist "%temp%\getadmin.vbs" ( del "%temp%\getadmin.vbs" )
    pushd "%~dp0"

:Menu
cls
color 0B
echo.
echo  ==============================================================
echo            %APP_NAME% - Service Manager
echo  ==============================================================
echo.
echo     [1] Run Program (Normal Mode - No Service)
echo     [2] Install as Windows Service (Auto-Start)
echo     [3] Uninstall Service
echo     [4] Start Service
echo     [5] Stop Service
echo     [6] Add to Startup (User Login)
echo     [7] Remove from Startup
echo.
echo     [0] Exit
echo.
echo  ==============================================================
set /p opt=" Choose an option [0-7]: "

if "%opt%"=="1" goto RunNormal
if "%opt%"=="2" goto InstallSvc
if "%opt%"=="3" goto UninstallSvc
if "%opt%"=="4" goto StartSvc
if "%opt%"=="5" goto StopSvc
if "%opt%"=="6" goto AddStartup
if "%opt%"=="7" goto RemoveStartup
if "%opt%"=="0" exit
goto Menu

:RunNormal
cls
echo.
echo [*] Starting %APP_NAME% in normal mode...
echo [!] Close this window to stop the server.
echo.
start "" "%~dp0%EXE_NAME%" --urls "http://0.0.0.0:5166"
pause
goto Menu

:InstallSvc
cls
echo.
echo [*] Installing Windows Service: %SERVICE_NAME%...
sc.exe create %SERVICE_NAME% binPath= "%~dp0%EXE_NAME%" start= auto
sc.exe description %SERVICE_NAME% "Background service for PrintBridge Core Server."
sc.exe start %SERVICE_NAME%
echo.
echo [OK] Service installed and started.
pause
goto Menu

:UninstallSvc
cls
echo.
echo [*] stopping and removing Service: %SERVICE_NAME%...
sc.exe stop %SERVICE_NAME% >nul 2>&1
sc.exe delete %SERVICE_NAME%
echo.
echo [OK] Service removed.
pause
goto Menu

:StartSvc
cls
echo.
echo [*] Starting Service...
sc.exe start %SERVICE_NAME%
pause
goto Menu

:StopSvc
cls
echo.
echo [*] Stopping Service...
sc.exe stop %SERVICE_NAME%
pause
goto Menu

:AddStartup
cls
echo.
echo [*] Adding shortcut to User Startup folder...
set "SCRIPT_PATH=%temp%\CreateShortcut.vbs"
echo Set oWS = WScript.CreateObject("WScript.Shell") > "%SCRIPT_PATH%"
echo sLinkFile = oWS.ExpandEnvironmentStrings("%%APPDATA%%\Microsoft\Windows\Start Menu\Programs\Startup\%APP_NAME%.lnk") >> "%SCRIPT_PATH%"
echo Set oLink = oWS.CreateShortcut(sLinkFile) >> "%SCRIPT_PATH%"
echo oLink.TargetPath = "%~dp0%EXE_NAME%" >> "%SCRIPT_PATH%"
echo oLink.Arguments = "--urls ""http://0.0.0.0:5166""" >> "%SCRIPT_PATH%"
echo oLink.WorkingDirectory = "%~dp0" >> "%SCRIPT_PATH%"
echo oLink.Save >> "%SCRIPT_PATH%"
cscript /nologo "%SCRIPT_PATH%"
del "%SCRIPT_PATH%"
echo.
echo [OK] Shortcut added to Startup.
pause
goto Menu

:RemoveStartup
cls
echo.
echo [*] Removing shortcut from Startup...
del "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\%APP_NAME%.lnk" >nul 2>&1
echo [OK] Shortcut removed.
pause
goto Menu
