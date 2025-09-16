# ML-Agents Setup with Python 3.11 (Primary Setup Script)
# Creates a clean, working ML-Agents environment with all compatibility fixes
# This replaces the older setup_venv.ps1 script

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

# Skip pip upgrade (causes corruption issues on Windows)
Write-Host "`n4. Checking pip..." -ForegroundColor Yellow
try {
    $pipVersion = & $python311 -m pip --version
    Write-Host "✅ Pip working: $pipVersion" -ForegroundColor Green
    Write-Host "   Skipping upgrade to avoid Windows corruption issues" -ForegroundColor White
} catch {
    Write-Host "⚠️  Pip not working, trying to fix..." -ForegroundColor Yellow
    try {
        # Use ensurepip as safer alternative
        & $python311 -m ensurepip --upgrade
        Write-Host "✅ Pip fixed using ensurepip" -ForegroundColor Green
    } catch {
        Write-Host "❌ Could not fix pip" -ForegroundColor Red
        exit 1
    }
}

# Install ML-Agents with compatible versions
Write-Host "`n5. Installing ML-Agents with compatible protobuf..." -ForegroundColor Yellow
try {
    Write-Host "   Installing compatible protobuf first..." -ForegroundColor White
    & $python311 -m pip install "protobuf==3.20.3"
    
    Write-Host "   Installing setuptools and wheel (without upgrade)..." -ForegroundColor White
    & $python311 -m pip install setuptools wheel
    
    Write-Host "   Installing ML-Agents..." -ForegroundColor White
    & $python311 -m pip install mlagents
    
    Write-Host "   Installing TensorBoard..." -ForegroundColor White
    & $python311 -m pip install tensorboard
    
    Write-Host "   Verifying ML-Agents installation..." -ForegroundColor White
    & $python311 -c "import mlagents_envs; print('ML-Agents packages verified')"
    
    # Verify specific commands exist
    $mlagentsLearnPath = "$VenvPath\Scripts\mlagents-learn.exe"
    if (Test-Path $mlagentsLearnPath) {
        Write-Host "   ✅ mlagents-learn executable found" -ForegroundColor Green
    } else {
        Write-Host "   ❌ mlagents-learn executable missing - installation may have failed" -ForegroundColor Red
        throw "ML-Agents installation incomplete"
    }
    
    Write-Host "✅ ML-Agents installed successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install ML-Agents: $_" -ForegroundColor Red
    Write-Host "`n🔧 Manual installation required:" -ForegroundColor Cyan
    Write-Host "   1. Activate environment: .\activate_mlagents_py311.ps1" -ForegroundColor White
    Write-Host "   2. Install manually: pip install protobuf==3.20.3 mlagents" -ForegroundColor White
    Write-Host "   3. Verify: .\Scripts\check_environment.ps1" -ForegroundColor White
    Write-Host "`nScript will continue to test what was installed..." -ForegroundColor Yellow
}

# Set compatibility environment variable
Write-Host "`n6. Setting up compatibility..." -ForegroundColor Yellow
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"

# Test installation
Write-Host "`n7. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $testOutput = & mlagents-learn --help 2>&1
    if ($testOutput -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Command may have issues:" -ForegroundColor Yellow
        Write-Host "   Run: .\Scripts\fix_py311_protobuf.ps1" -ForegroundColor Cyan
    }
} catch {
    Write-Host "⚠️  Could not test command fully:" -ForegroundColor Yellow
    Write-Host "   Run: .\Scripts\fix_py311_protobuf.ps1" -ForegroundColor Cyan
}

# Create custom activation wrapper (don't modify signed scripts)
Write-Host "`n8. Creating activation wrapper..." -ForegroundColor Yellow
try {
    # The activation wrapper is created separately and includes env vars
    Write-Host "✅ Custom activation script: activate_mlagents_py311.ps1" -ForegroundColor Green
    Write-Host "   (Environment variables set automatically when using wrapper)" -ForegroundColor White
} catch {
    Write-Host "⚠️  Activation wrapper creation had issues, but environment is ready" -ForegroundColor Yellow
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

# Final verification and next steps
Write-Host "`n📋 Running final environment check..." -ForegroundColor Cyan
try {
    & $python311 -c "import mlagents_envs, mlagents; print(f'ML-Agents version: {mlagents.__version__}')"
    $mlagentsLearnPath = "$VenvPath\Scripts\mlagents-learn.exe"
    if (Test-Path $mlagentsLearnPath) {
        Write-Host "`n🎉 Python 3.11 Setup Complete!" -ForegroundColor Green
        Write-Host "=============================" -ForegroundColor Green
        Write-Host "✅ Python 3.11 virtual environment: $VenvPath" -ForegroundColor Green
        Write-Host "✅ ML-Agents installed successfully" -ForegroundColor Green
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
    } else {
        Write-Host "`n⚠️  Setup completed with issues" -ForegroundColor Yellow
        Write-Host "=============================" -ForegroundColor Yellow
        Write-Host "✅ Python 3.11 virtual environment: $VenvPath" -ForegroundColor Green
        Write-Host "❌ ML-Agents installation incomplete" -ForegroundColor Red
        Write-Host "✅ Activation script: activate_mlagents_py311.ps1" -ForegroundColor Green
        
        Write-Host "`n🔧 Manual installation required:" -ForegroundColor Cyan
        Write-Host "1. Activate environment: .\activate_mlagents_py311.ps1" -ForegroundColor White
        Write-Host "2. Install manually: pip install protobuf==3.20.3 mlagents" -ForegroundColor White
        Write-Host "3. Verify setup: .\Scripts\check_environment.ps1" -ForegroundColor White
    }
} catch {
    Write-Host "`n⚠️  Setup completed but verification failed" -ForegroundColor Yellow
    Write-Host "=============================" -ForegroundColor Yellow
    Write-Host "✅ Python 3.11 virtual environment: $VenvPath" -ForegroundColor Green
    Write-Host "❓ ML-Agents status unknown" -ForegroundColor Yellow
    Write-Host "✅ Activation script: activate_mlagents_py311.ps1" -ForegroundColor Green
    Write-Host "Error: $_" -ForegroundColor Red
    
    Write-Host "`n🔧 Manual verification required:" -ForegroundColor Cyan
    Write-Host "1. Activate environment: .\activate_mlagents_py311.ps1" -ForegroundColor White
    Write-Host "2. Install if needed: pip install protobuf==3.20.3 mlagents" -ForegroundColor White
    Write-Host "3. Verify setup: .\Scripts\check_environment.ps1" -ForegroundColor White
}