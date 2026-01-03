@echo off
setlocal EnableDelayedExpansion

echo ========================================
echo Publishing LevelRank Solution
echo ========================================
echo.

set "ROOT_DIR=%~dp0"
set "BUILD_CONFIG=Release"
set "FAILED=0"

:: Set output directories
set "MODULES_DIR=%ROOT_DIR%sharp\modules"
set "SHARED_DIR=%ROOT_DIR%sharp\shared"

:: Clean output directories
if exist "%MODULES_DIR%\LevelRank" rd /s /q "%MODULES_DIR%\LevelRank"
if exist "%MODULES_DIR%\LevelRank.Request.Sql" rd /s /q "%MODULES_DIR%\LevelRank.Request.Sql"
if exist "%SHARED_DIR%\LevelRank.Shared" rd /s /q "%SHARED_DIR%\LevelRank.Shared"

:: Publish LevelRank.Shared
echo [PUBLISH] LevelRank.Shared
dotnet publish "%ROOT_DIR%LevelRank.Shared\LevelRank.Shared.csproj" -c %BUILD_CONFIG% -o "%SHARED_DIR%\LevelRank.Shared" --nologo -v q
if !ERRORLEVEL! neq 0 (
    echo [FAILED] LevelRank.Shared
    set "FAILED=1"
    goto :end
) else (
    echo [OK] LevelRank.Shared
)
echo.

:: Publish LevelRank.Request.Sql
echo [PUBLISH] LevelRank.Request.Sql
dotnet publish "%ROOT_DIR%LevelRank.RequestManager\LevelRank.Request.Sql.csproj" -c %BUILD_CONFIG% -o "%MODULES_DIR%\LevelRank.Request.Sql" --nologo -v q
if !ERRORLEVEL! neq 0 (
    echo [FAILED] LevelRank.Request.Sql
    set "FAILED=1"
    goto :end
) else (
    echo [OK] LevelRank.Request.Sql
)
echo.

:: Publish LevelRank
echo [PUBLISH] LevelRank
dotnet publish "%ROOT_DIR%LevelRank\LevelRank.csproj" -c %BUILD_CONFIG% -o "%MODULES_DIR%\LevelRank" --nologo -v q
if !ERRORLEVEL! neq 0 (
    echo [FAILED] LevelRank
    set "FAILED=1"
    goto :end
) else (
    echo [OK] LevelRank
)
echo.

:: Remove LevelRank.Shared from module folders
echo [CLEANUP] Removing LevelRank.Shared from modules...
del /q "%MODULES_DIR%\LevelRank\LevelRank.Shared.*" >nul 2>&1
del /q "%MODULES_DIR%\LevelRank.Request.Sql\LevelRank.Shared.*" >nul 2>&1
echo [OK] Cleanup complete
echo.

:end
echo ========================================
if %FAILED%==1 (
    echo Publish completed with errors.
    exit /b 1
) else (
    echo Publish completed successfully.
    echo Output: %MODULES_DIR% and %SHARED_DIR%
    exit /b 0
)
