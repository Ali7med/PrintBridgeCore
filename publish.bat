@echo off
setlocal enabledelayedexpansion

echo.
echo ========================================
echo   PrintBridge Core - Build ^& Publish
echo ========================================
echo.

:: Define paths
set "PROJECT_PATH=PrinterServer.Api\PrinterServer.Api.csproj"
set "PUBLISH_DIR=.\Publish"
set "SERVICE_NAME=PrintBridgeCore"

:: 1. Stop the service if it's running
echo [*] Checking and stopping service: %SERVICE_NAME%...
sc query %SERVICE_NAME% | find "RUNNING" >nul
if %ERRORLEVEL% equ 0 (
    echo [!] Service is running. Stopping it...
    sc stop %SERVICE_NAME% >nul
    :: Wait a bit for the service to actually stop
    timeout /t 3 /nobreak >nul
)

:: 2. Kill any hanging processes just in case
echo [*] Cleaning up processes...
taskkill /F /IM PrinterServer.Api.exe /T 2>nul

:: 3. Run the publish command
echo [*] Starting Publish (Self-Contained win-x64)...
echo.
dotnet publish "%PROJECT_PATH%" -c Release -r win-x64 --self-contained true -o "%PUBLISH_DIR%"

if %ERRORLEVEL% neq 0 (
    echo.
    echo [X] ERROR: Publish failed! Please check the errors above.
    pause
    exit /b %ERRORLEVEL%
)

:: 4. Ensure required tools are present in the publish folder
echo [*] Verifying extra tools...
if exist "SumatraPDF.exe" (
    copy /Y "SumatraPDF.exe" "%PUBLISH_DIR%\SumatraPDF.exe" >nul
    echo [OK] SumatraPDF.exe copied to Publish folder.
)
if exist "ServiceManager.bat" (
    copy /Y "ServiceManager.bat" "%PUBLISH_DIR%\ServiceManager.bat" >nul
    echo [OK] ServiceManager.bat copied to Publish folder.
)

:: 5. Restart the service if it exists
echo [*] Checking service status for restart...
sc query %SERVICE_NAME% >nul
if %ERRORLEVEL% equ 0 (
    echo [!] Starting service: %SERVICE_NAME%...
    sc start %SERVICE_NAME% >nul
)

echo.
echo ========================================
echo   BUILD COMPLETED SUCCESSFULLY!
echo   Location: %PUBLISH_DIR%
echo ========================================
echo.
pause
