@echo off
REM Updates packages in paket.dependencies to the latest version and updates paket.lock.

.paket\paket.bootstrapper.exe
if errorlevel 1 (
    exit /b %errorlevel%
)

REM The -f parameter forces the update which is needed for HTTP downloads.
.paket\paket.exe update -f
if errorlevel 1 (
    exit /b %errorlevel%
)
