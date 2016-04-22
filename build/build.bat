@echo off

.paket\paket.bootstrapper.exe
if errorlevel 1 (
    exit /b %errorlevel%
)

.paket\paket.exe install
if errorlevel 1 (
    exit /b %errorlevel%
)

REM packages\FAKE\tools\FAKE.exe build.fsx %* --version
packages\FAKE\tools\FAKE.exe build.fsx %*
