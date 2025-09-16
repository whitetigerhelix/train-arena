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

# Check Python installation
Write-Host "`n1. Checking Python installation..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Python not found in PATH" -ForegroundColor Red
    Write-Host "   Please install Python 3.8+ and add to PATH" -ForegroundColor White
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
    $mlagentsHelp = mlagents-learn --help 2>&1
    if ($mlagentsHelp -match "usage:") {
        Write-Host "✅ mlagents-learn command works" -ForegroundColor Green
    } else {
        throw "Command failed"
    }
} catch {
    Write-Host "❌ mlagents-learn command failed" -ForegroundColor Red
    Write-Host "   Error: $_" -ForegroundColor White
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
Write-Host "`n🚀 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Run: .\Scripts\train_cube.ps1" -ForegroundColor White
Write-Host "   2. Open Unity and press PLAY when training starts" -ForegroundColor White
Write-Host "   3. Monitor progress at http://localhost:6006" -ForegroundColor White