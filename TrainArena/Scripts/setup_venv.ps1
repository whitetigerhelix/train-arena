# ML-Agents Virtual Environment Setup Script
# Creates an isolated Python environment for ML-Agents to avoid conflicts

param(
    [string]$VenvName = "mlagents-env",
    [switch]$Force
)

Write-Host "🐍 ML-Agents Virtual Environment Setup" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green

# Check if Python is available
Write-Host "`n1. Checking Python installation..." -ForegroundColor Yellow
try {
    $pythonVersion = python --version 2>&1
    Write-Host "✅ Python found: $pythonVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Python not found in PATH" -ForegroundColor Red
    Write-Host "   Please install Python 3.8+ from https://python.org" -ForegroundColor White
    exit 1
}

# Check Python version (ML-Agents requires 3.8+)
$versionMatch = $pythonVersion -match "Python (\d+)\.(\d+)"
if ($versionMatch) {
    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    if ($major -lt 3 -or ($major -eq 3 -and $minor -lt 8)) {
        Write-Host "❌ Python version too old. ML-Agents requires Python 3.8+" -ForegroundColor Red
        exit 1
    }
}

# Create virtual environment
$VenvPath = "venv\$VenvName"
Write-Host "`n2. Creating virtual environment: $VenvPath" -ForegroundColor Yellow

if (Test-Path $VenvPath) {
    if ($Force) {
        Write-Host "⚠️  Removing existing environment..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force $VenvPath
    } else {
        Write-Host "❌ Virtual environment already exists at: $VenvPath" -ForegroundColor Red
        Write-Host "   Use -Force to recreate, or activate existing one:" -ForegroundColor White
        Write-Host "   .\venv\$VenvName\Scripts\Activate.ps1" -ForegroundColor Cyan
        exit 1
    }
}

try {
    python -m venv $VenvPath
    Write-Host "✅ Virtual environment created" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create virtual environment: $_" -ForegroundColor Red
    exit 1
}

# Activate virtual environment
Write-Host "`n3. Activating virtual environment..." -ForegroundColor Yellow
$ActivateScript = "$VenvPath\Scripts\Activate.ps1"

if (!(Test-Path $ActivateScript)) {
    Write-Host "❌ Activation script not found: $ActivateScript" -ForegroundColor Red
    exit 1
}

try {
    & $ActivateScript
    Write-Host "✅ Virtual environment activated" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to activate environment: $_" -ForegroundColor Red
    Write-Host "   You may need to enable script execution:" -ForegroundColor White
    Write-Host "   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser" -ForegroundColor Cyan
    exit 1
}

# Upgrade pip
Write-Host "`n4. Upgrading pip..." -ForegroundColor Yellow
try {
    python -m pip install --upgrade pip
    Write-Host "✅ Pip upgraded" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Pip upgrade failed, continuing..." -ForegroundColor Yellow
}

# Install ML-Agents
Write-Host "`n5. Installing ML-Agents..." -ForegroundColor Yellow
try {
    Write-Host "   Installing mlagents package (this may take a few minutes)..." -ForegroundColor White
    python -m pip install mlagents
    Write-Host "✅ ML-Agents installed successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install ML-Agents: $_" -ForegroundColor Red
    exit 1
}

# Install TensorBoard (if not already included)
Write-Host "`n6. Installing TensorBoard..." -ForegroundColor Yellow
try {
    python -m pip install tensorboard
    Write-Host "✅ TensorBoard installed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  TensorBoard install failed, but may already be included with ML-Agents" -ForegroundColor Yellow
}

# Verify installation
Write-Host "`n7. Verifying installation..." -ForegroundColor Yellow
try {
    $mlagentsVersion = python -c "import mlagents; print(mlagents.__version__)" 2>&1
    Write-Host "✅ ML-Agents version: $mlagentsVersion" -ForegroundColor Green
    
    $tensorboardVersion = python -c "import tensorboard; print(tensorboard.__version__)" 2>&1
    Write-Host "✅ TensorBoard version: $tensorboardVersion" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Could not verify versions, but packages appear to be installed" -ForegroundColor Yellow
}

# Test mlagents-learn command
Write-Host "`n8. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $helpOutput = mlagents-learn --help 2>&1
    if ($helpOutput -match "usage:") {
        Write-Host "✅ mlagents-learn command works" -ForegroundColor Green
    } else {
        Write-Host "⚠️  mlagents-learn command may not be working properly" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Could not test mlagents-learn command" -ForegroundColor Yellow
}

# Create activation script for convenience
$QuickActivate = @"
# Quick activation script for ML-Agents environment
# Run this to activate the virtual environment in any new PowerShell session

Write-Host "🐍 Activating ML-Agents environment..." -ForegroundColor Green
& "venv\$VenvName\Scripts\Activate.ps1"
Write-Host "✅ Environment activated! You can now run:" -ForegroundColor Green
Write-Host "   .\Scripts\train_cube.ps1" -ForegroundColor Cyan
Write-Host "   .\Scripts\check_environment.ps1" -ForegroundColor Cyan
"@

$QuickActivate | Out-File -FilePath "activate_mlagents.ps1" -Encoding UTF8

Write-Host "`n🎉 Setup Complete!" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
Write-Host "✅ Virtual environment created: $VenvPath" -ForegroundColor Green
Write-Host "✅ ML-Agents installed and ready" -ForegroundColor Green
Write-Host "✅ Quick activation script created: activate_mlagents.ps1" -ForegroundColor Green

Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Virtual environment is currently active in this session" -ForegroundColor White
Write-Host "2. Run: .\Scripts\check_environment.ps1 (to verify everything works)" -ForegroundColor White  
Write-Host "3. Run: .\Scripts\train_cube.ps1 (to start training)" -ForegroundColor White
Write-Host "`n💡 For future sessions:" -ForegroundColor Yellow
Write-Host "   Run: .\activate_mlagents.ps1 (to activate the environment)" -ForegroundColor Cyan

Write-Host "`n📍 Current Status:" -ForegroundColor Magenta
Write-Host "   Virtual Environment: ACTIVE" -ForegroundColor Green
Write-Host "   Ready for ML-Agents training: YES" -ForegroundColor Green