@echo off
cls

if not exist .paket (
    @echo "Installing Paket"
    mkdir .paket
    curl https://github.com/fsprojects/Paket/releases/download/3.31.0/paket.bootstrapper.exe -L --insecure -o .paket\paket.bootstrapper.exe
    .paket\paket.bootstrapper.exe
    if errorlevel 1 (
        exit /b %errorlevel%
    )
)
    
if not exist paket.lock (
    @echo "Installing dependencies"
    .paket\paket.exe install
) else (
    @echo "Restoring dependencies"
    .paket\paket.exe restore
)