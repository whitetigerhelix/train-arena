# ML-Agents Protobuf Fix Script
# Fixes the common "Descriptors cannot be created directly" error

Write-Host "🔧 ML-Agents Protobuf Compatibility Fix" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Check if virtual environment is active
if (!$env:VIRTUAL_ENV) {
    Write-Host "❌ No virtual environment detected" -ForegroundColor Red
    Write-Host "   Please activate your ML-Agents environment first:" -ForegroundColor White
    Write-Host "   .\activate_mlagents.ps1" -ForegroundColor Cyan
    exit 1
}

Write-Host "✅ Virtual environment active: $env:VIRTUAL_ENV" -ForegroundColor Green

# Check current protobuf version
Write-Host "`n1. Checking current protobuf version..." -ForegroundColor Yellow
try {
    $protobufVersion = python -c "import google.protobuf; print(google.protobuf.__version__)" 2>&1
    Write-Host "   Current protobuf version: $protobufVersion" -ForegroundColor White
} catch {
    Write-Host "   Could not determine protobuf version" -ForegroundColor Yellow
}

# Apply the fix
Write-Host "`n2. Applying protobuf compatibility fix..." -ForegroundColor Yellow
try {
    Write-Host "   Installing compatible protobuf version (3.20.3)..." -ForegroundColor White
    python -m pip install "protobuf==3.20.3" --force-reinstall
    
    Write-Host "   Verifying installation..." -ForegroundColor White
    $newVersion = python -c "import google.protobuf; print(google.protobuf.__version__)"
    Write-Host "✅ Protobuf updated to version: $newVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to fix protobuf version: $_" -ForegroundColor Red
    
    # Try alternative method
    Write-Host "`n   Trying alternative method (environment variable)..." -ForegroundColor Yellow
    Write-Host "   Setting PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python" -ForegroundColor White
    
    # Add to current session
    $env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
    
    # Add to activation script for persistence
    $activationScript = "$env:VIRTUAL_ENV\Scripts\activate.bat"
    if (Test-Path $activationScript) {
        Add-Content $activationScript "`nset PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python"
        Write-Host "✅ Environment variable added to activation script" -ForegroundColor Green
    }
}

# Test the fix
Write-Host "`n3. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ mlagents-learn command now works!" -ForegroundColor Green
    } else {
        Write-Host "❌ Command still failing:" -ForegroundColor Red
        if ($stderr) {
            Write-Host "   $stderr" -ForegroundColor White
        }
        
        # Show additional troubleshooting
        Write-Host "`n📋 Additional options to try:" -ForegroundColor Cyan
        Write-Host "1. Recreate virtual environment:" -ForegroundColor White
        Write-Host "   .\Scripts\setup_venv.ps1 -Force" -ForegroundColor Cyan
        Write-Host "2. Use older ML-Agents version:" -ForegroundColor White
        Write-Host "   pip install mlagents==0.27.0" -ForegroundColor Cyan
        Write-Host "3. Manual protobuf downgrade:" -ForegroundColor White
        Write-Host "   pip install protobuf==3.19.6" -ForegroundColor Cyan
        exit 1
    }
} catch {
    Write-Host "❌ Could not test command: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Fix Applied Successfully!" -ForegroundColor Green
Write-Host "=========================" -ForegroundColor Green
Write-Host "✅ ML-Agents should now work without protobuf errors" -ForegroundColor Green
Write-Host "✅ You can proceed with training" -ForegroundColor Green

Write-Host "`n🚀 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Run: .\Scripts\check_environment.ps1" -ForegroundColor White
Write-Host "   2. Run: .\Scripts\train_cube.ps1" -ForegroundColor White