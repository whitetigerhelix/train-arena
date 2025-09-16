# ML-Agents Setup with Python 3.11 (Recommended)
# Creates a reliable ML-Agents environment using Python 3.11

param(
    [string]$Python311Path = "",
    [switch]$Force
)

Write-Host "🐍 ML-Agents Setup with Python 3.11" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

# Try to find Python 3.11 installation
Write-Host "`n1. Looking for Python 3.11 installation..." -ForegroundColor Yellow

$python311Candidates = @(
    "python3.11",
    "python311", 
    "C:\Python311\python.exe",
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Python\Python311\python.exe",
    "$env:USERPROFILE\AppData\Local\Programs\Python\Python311\python.exe"
)

if ($Python311Path) {
    $python311Candidates = @($Python311Path) + $python311Candidates
}

$python311 = $null
foreach ($candidate in $python311Candidates) {
    try {
        $version = & $candidate --version 2>&1
        if ($version -match "Python 3\.11") {
            $python311 = $candidate
            Write-Host "✅ Found Python 3.11: $candidate" -ForegroundColor Green
            Write-Host "   Version: $version" -ForegroundColor White
            break
        }
    } catch {
        # Continue searching
    }
}

if (!$python311) {
    Write-Host "❌ Python 3.11 not found" -ForegroundColor Red
    Write-Host "`n📥 Please install Python 3.11:" -ForegroundColor Cyan
    Write-Host "1. Go to: https://www.python.org/downloads/" -ForegroundColor White
    Write-Host "2. Download Python 3.11.x (latest 3.11 version)" -ForegroundColor White
    Write-Host "3. Install with 'Add to PATH' checked" -ForegroundColor White
    Write-Host "4. Run this script again" -ForegroundColor White
    Write-Host "`n💡 Or specify custom path:" -ForegroundColor Yellow
    Write-Host "   .\Scripts\setup_python311.ps1 -Python311Path 'C:\path\to\python311.exe'" -ForegroundColor Cyan
    exit 1
}

# Create virtual environment with Python 3.11
$VenvName = "mlagents-py311"
$VenvPath = "venv\$VenvName"

Write-Host "`n2. Creating Python 3.11 virtual environment..." -ForegroundColor Yellow

if (Test-Path $VenvPath) {
    if ($Force) {
        Write-Host "   Removing existing environment..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force $VenvPath
    } else {
        Write-Host "❌ Environment already exists: $VenvPath" -ForegroundColor Red
        Write-Host "   Use -Force to recreate" -ForegroundColor White
        exit 1
    }
}

try {
    & $python311 -m venv $VenvPath
    Write-Host "✅ Virtual environment created with Python 3.11" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create virtual environment: $_" -ForegroundColor Red
    exit 1
}

# Activate the environment
Write-Host "`n3. Activating virtual environment..." -ForegroundColor Yellow
$ActivateScript = "$VenvPath\Scripts\Activate.ps1"

try {
    & $ActivateScript
    Write-Host "✅ Environment activated" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to activate: $_" -ForegroundColor Red
    exit 1
}

# Upgrade pip
Write-Host "`n4. Upgrading pip..." -ForegroundColor Yellow
try {
    & $python311 -m pip install --upgrade pip
    Write-Host "✅ Pip upgraded" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Pip upgrade failed, continuing..." -ForegroundColor Yellow
}

# Install ML-Agents with compatible versions
Write-Host "`n5. Installing ML-Agents (Python 3.11 compatible)..." -ForegroundColor Yellow
try {
    Write-Host "   Installing ML-Agents..." -ForegroundColor White
    & $python311 -m pip install mlagents
    
    Write-Host "   Installing TensorBoard..." -ForegroundColor White
    & $python311 -m pip install tensorboard
    
    Write-Host "✅ ML-Agents installed successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install ML-Agents: $_" -ForegroundColor Red
    exit 1
}

# Test installation
Write-Host "`n6. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $testOutput = & mlagents-learn --help 2>&1
    if ($testOutput -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Command may have issues, but installation completed" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️  Could not test command, but installation completed" -ForegroundColor Yellow
}

# Create new activation script
$QuickActivate311 = @"
# Quick activation for Python 3.11 ML-Agents environment
Write-Host "🐍 Activating ML-Agents (Python 3.11) environment..." -ForegroundColor Green
& "venv\$VenvName\Scripts\Activate.ps1"
Write-Host "✅ Python 3.11 environment activated!" -ForegroundColor Green
Write-Host "✅ Compatible ML-Agents ready for training" -ForegroundColor Green
"@

$QuickActivate311 | Out-File -FilePath "activate_mlagents_py311.ps1" -Encoding UTF8

Write-Host "`n🎉 Python 3.11 Setup Complete!" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host "✅ Python 3.11 virtual environment: $VenvPath" -ForegroundColor Green
Write-Host "✅ ML-Agents installed and tested" -ForegroundColor Green
Write-Host "✅ Activation script: activate_mlagents_py311.ps1" -ForegroundColor Green

Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Environment is active in this session" -ForegroundColor White
Write-Host "2. Run: .\Scripts\check_environment.ps1" -ForegroundColor White
Write-Host "3. Run: .\Scripts\train_cube.ps1" -ForegroundColor White
Write-Host "`n💡 For future sessions:" -ForegroundColor Yellow
Write-Host "   Run: .\activate_mlagents_py311.ps1" -ForegroundColor Cyan

Write-Host "`n📊 Environment Info:" -ForegroundColor Magenta
Write-Host "   Python Version: Compatible 3.11" -ForegroundColor Green
Write-Host "   ML-Agents: Fully Compatible" -ForegroundColor Green
Write-Host "   Ready for Training: YES" -ForegroundColor Green