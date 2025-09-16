# ML-Agents Environment Verification Script
# Use this to check if your Python environment is properly set up for ML-Agents training

Write-Host "🔍 ML-Agents Environment Check" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Check if we're in a virtual environment
Write-Host "`n0. Checking virtual environment..." -ForegroundColor Yellow
if ($env:VIRTUAL_ENV) {
    Write-Host "✅ Virtual environment active: $env:VIRTUAL_ENV" -ForegroundColor Green
} else {
    Write-Host "⚠️  No virtual environment detected" -ForegroundColor Yellow
    Write-Host "   Consider running: .\Scripts\setup_venv.ps1" -ForegroundColor Cyan
    Write-Host "   Or activate existing: .\activate_mlagents.ps1" -ForegroundColor Cyan
}

# Check Python installation and version compatibility
Write-Host "`n1. Checking Python installation..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
    
    # Check for Python 3.12+ compatibility issues
    if ($pythonVersion -match "Python 3\.1[2-9]") {
        Write-Host "⚠️  Python 3.12+ detected - may have ML-Agents compatibility issues" -ForegroundColor Yellow
        Write-Host "   Consider using Python 3.11 for best ML-Agents compatibility" -ForegroundColor White
        Write-Host "   Run: .\Scripts\setup_python311.ps1 (if you have Python 3.11 installed)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "❌ Python not found in PATH" -ForegroundColor Red
    Write-Host "   Please install Python 3.8-3.11 and add to PATH" -ForegroundColor White
    exit 1
}

# Check pip installation
Write-Host "`n2. Checking pip..." -ForegroundColor Yellow
try {
    $pipVersion = pip --version 2>&1
    Write-Host "✅ Pip found: $pipVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Pip not found" -ForegroundColor Red
    exit 1
}

# Check mlagents package
Write-Host "`n3. Checking mlagents package..." -ForegroundColor Yellow
try {
    $mlagentsCheck = pip show mlagents 2>&1
    if ($mlagentsCheck -match "Version:") {
        $version = ($mlagentsCheck -split "`n" | Where-Object { $_ -match "Version:" }) -replace "Version:\s*", ""
        Write-Host "✅ mlagents package found: $version" -ForegroundColor Green
    } else {
        throw "Package not found"
    }
} catch {
    Write-Host "❌ mlagents package not installed" -ForegroundColor Red
    Write-Host "`n📦 To install ML-Agents:" -ForegroundColor Cyan
    Write-Host "   pip install mlagents" -ForegroundColor White
    Write-Host "`n   Or with specific version:" -ForegroundColor White
    Write-Host "   pip install mlagents==1.0.0" -ForegroundColor White
    exit 1
}

# Check mlagents-learn command
Write-Host "`n4. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    # Use Start-Process for better error capture
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0 -and ($stdout -match "usage:" -or $stdout -match "mlagents-learn")) {
        Write-Host "✅ mlagents-learn command works correctly" -ForegroundColor Green
    } elseif ($stderr -match "protobuf.*Descriptors cannot be created directly") {
        Write-Host "⚠️  Protobuf compatibility issue detected" -ForegroundColor Yellow
        Write-Host "   This is a known issue with ML-Agents and newer protobuf versions" -ForegroundColor White
        Write-Host "`n🔧 Fix this by running:" -ForegroundColor Cyan
        Write-Host "   pip install 'protobuf==3.20.3' --force-reinstall" -ForegroundColor White
        Write-Host "   Or set environment variable: PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python" -ForegroundColor White
        exit 1
    } else {
        Write-Host "❌ mlagents-learn command failed" -ForegroundColor Red
        Write-Host "   Exit Code: $($process.ExitCode)" -ForegroundColor White
        if ($stderr) {
            Write-Host "   Error Details: $($stderr.Trim())" -ForegroundColor White
        }
        if ($stdout) {
            Write-Host "   Output: $($stdout.Trim())" -ForegroundColor White
        }
        
        Write-Host "`n🔧 Troubleshooting steps:" -ForegroundColor Cyan
        Write-Host "   1. Ensure virtual environment is activated" -ForegroundColor White
        Write-Host "   2. Try: pip install --upgrade mlagents" -ForegroundColor White
        Write-Host "   3. Check: python -m pip list | findstr mlagents" -ForegroundColor White
        exit 1
    }
} catch {
    Write-Host "❌ mlagents-learn command failed with exception" -ForegroundColor Red
    Write-Host "   Exception: $_" -ForegroundColor White
    exit 1
}

# Check TensorBoard
Write-Host "`n5. Checking TensorBoard..." -ForegroundColor Yellow
try {
    $tensorboardCheck = tensorboard --version 2>&1
    Write-Host "✅ TensorBoard found: $tensorboardCheck" -ForegroundColor Green
} catch {
    Write-Host "⚠️  TensorBoard not found (optional but recommended)" -ForegroundColor Yellow
    Write-Host "   Install with: pip install tensorboard" -ForegroundColor White
}

Write-Host "`n🎉 Environment Check Complete!" -ForegroundColor Green
Write-Host "✅ Your Python environment is ready for ML-Agents training" -ForegroundColor Green
Write-Host "`n� Additional Diagnostics:" -ForegroundColor Magenta
Write-Host "Python packages installed:" -ForegroundColor Yellow
try {
    $packages = python -m pip list | Select-String -Pattern "(mlagents|torch|tensorboard|numpy)"
    $packages | ForEach-Object { Write-Host "   $_" -ForegroundColor White }
} catch {
    Write-Host "   Could not list packages" -ForegroundColor Red
}

Write-Host "`n�🚀 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Run: .\Scripts\train_cube.ps1" -ForegroundColor White
Write-Host "   2. Open Unity and press PLAY when training starts" -ForegroundColor White
Write-Host "   3. Monitor progress at http://localhost:6006" -ForegroundColor White
Write-Host "   4. Read: .\TRAINING_GUIDE.md for detailed instructions" -ForegroundColor White