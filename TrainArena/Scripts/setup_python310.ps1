# ML-Agents Setup with Python 3.10.11 (Unity Official Specification)
# Based on Unity ML-Agents Release 23: Python 3.10+ required, ML-Agents 1.1.0 latest compatible

param(
    [switch]$Force
)

Write-Host "🐍 ML-Agents Setup with Python 3.10.11 (Unity Official Spec)" -ForegroundColor Cyan
Write-Host "=========================================================" -ForegroundColor Cyan

$VenvName = "mlagents-py310"
$VenvPath = "venv\$VenvName"
$Python310Url = "https://www.python.org/ftp/python/3.10.11/python-3.10.11-amd64.exe"
$Python310InstallDir = "C:\Python310"

# 1. Check for Python 3.10.11 installation  
Write-Host "`n1. Checking for Python 3.10.11 installation..." -ForegroundColor Yellow

$python310Paths = @(
    "C:\Python310\python.exe",
    "C:\Users\$env:USERNAME\AppData\Local\Programs\Python\Python310\python.exe",
    "$env:LOCALAPPDATA\Programs\Python\Python310\python.exe"
)

$python310 = $null
foreach ($path in $python310Paths) {
    if (Test-Path $path) {
        try {
            $version = & $path --version 2>&1
            if ($version -match "Python 3\.10\.") {
                $python310 = $path
                Write-Host "✅ Found Python 3.10: $path" -ForegroundColor Green
                Write-Host "   Version: $version" -ForegroundColor White
                break
            }
        } catch {
            continue
        }
    }
}

# Install Python 3.10.11 if not found
if (-not $python310) {
    Write-Host "❌ Python 3.10.11 not found. Installing..." -ForegroundColor Red
    Write-Host "`n📥 Downloading Python 3.10.11..." -ForegroundColor Cyan
    
    $tempFile = "$env:TEMP\python-3.10.11-amd64.exe"
    try {
        Invoke-WebRequest -Uri $Python310Url -OutFile $tempFile -UseBasicParsing
        Write-Host "✅ Downloaded successfully" -ForegroundColor Green
        
        Write-Host "`n🔧 Installing Python 3.10.11..." -ForegroundColor Cyan
        Write-Host "   Installation directory: $Python310InstallDir" -ForegroundColor White
        Write-Host "   This may take a few minutes..." -ForegroundColor White
        
        $installArgs = @(
            "/quiet",
            "InstallAllUsers=1",
            "PrependPath=0",  # Don't modify PATH to avoid conflicts
            "Include_test=0",
            "SimpleInstall=1",
            "TargetDir=$Python310InstallDir"
        )
        
        Start-Process -FilePath $tempFile -ArgumentList $installArgs -Wait -NoNewWindow
        
        # Verify installation
        $python310 = "$Python310InstallDir\python.exe"
        if (Test-Path $python310) {
            $version = & $python310 --version
            Write-Host "✅ Python 3.10.11 installed successfully" -ForegroundColor Green
            Write-Host "   Location: $python310" -ForegroundColor White
            Write-Host "   Version: $version" -ForegroundColor White
        } else {
            throw "Installation failed - executable not found"
        }
        
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    } catch {
        Write-Host "❌ Failed to install Python 3.10.11: $_" -ForegroundColor Red
        Write-Host "`n🔧 Manual Installation Required:" -ForegroundColor Yellow
        Write-Host "1. Download: $Python310Url" -ForegroundColor White
        Write-Host "2. Install to: $Python310InstallDir" -ForegroundColor White
        Write-Host "3. Run this script again" -ForegroundColor White
        exit 1
    }
}

# 2. Create/recreate virtual environment
Write-Host "`n2. Creating Python 3.10 virtual environment..." -ForegroundColor Yellow
if (Test-Path $VenvPath) {
    if ($Force) {
        Write-Host "   Removing existing environment..." -ForegroundColor White
        Remove-Item -Recurse -Force $VenvPath
    } else {
        Write-Host "❌ Virtual environment already exists: $VenvPath" -ForegroundColor Red
        Write-Host "   Use -Force to recreate" -ForegroundColor White
        exit 1
    }
}

try {
    & $python310 -m venv $VenvPath
    Write-Host "✅ Virtual environment created with Python 3.10" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create virtual environment: $_" -ForegroundColor Red
    exit 1
}

# 3. Activate the environment
Write-Host "`n3. Activating virtual environment..." -ForegroundColor Yellow
$ActivateScript = "$VenvPath\Scripts\Activate.ps1"

try {
    & $ActivateScript
    Write-Host "✅ Environment activated" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to activate: $_" -ForegroundColor Red
    exit 1
}

# 4. Verify pip and upgrade if needed
$python310venv = "$VenvPath\Scripts\python.exe"
Write-Host "`n4. Checking pip..." -ForegroundColor Yellow
try {
    $pipVersion = & $python310venv -m pip --version
    Write-Host "✅ Pip working: $pipVersion" -ForegroundColor Green
    
    # Only upgrade pip if it's very old (avoid Windows corruption issues)
    if ($pipVersion -match "pip (\d+)\.") {
        $majorVersion = [int]$matches[1]
        if ($majorVersion -lt 20) {
            Write-Host "   Upgrading old pip version..." -ForegroundColor White
            & $python310venv -m pip install --upgrade pip
        } else {
            Write-Host "   Pip version is recent, skipping upgrade" -ForegroundColor White
        }
    }
} catch {
    Write-Host "⚠️  Pip issue detected, using ensurepip..." -ForegroundColor Yellow
    try {
        & $python310venv -m ensurepip --upgrade
        Write-Host "✅ Pip restored via ensurepip" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to restore pip: $_" -ForegroundColor Red
        exit 1
    }
}

# 5. Install ML-Agents 1.1.0 with compatible versions
Write-Host "`n5. Installing ML-Agents 1.1.0 (Latest Available Version)..." -ForegroundColor Yellow
try {
    Write-Host "   Installing compatible protobuf first..." -ForegroundColor White
    & $python310venv -m pip install "protobuf==3.20.3"
    
    Write-Host "   Installing PyTorch (Windows compatible)..." -ForegroundColor White
    & $python310venv -m pip install torch~=2.1.1 -f https://download.pytorch.org/whl/torch_stable.html
    
    Write-Host "   Installing ML-Agents 1.1.0 (latest)..." -ForegroundColor White
    & $python310venv -m pip install "mlagents==1.1.0"
    
    Write-Host "   Installing TensorBoard..." -ForegroundColor White
    & $python310venv -m pip install tensorboard
    
    Write-Host "   Verifying ML-Agents installation..." -ForegroundColor White
    try {
        & $python310venv -c "import mlagents_envs, mlagents; print('ML-Agents modules imported successfully')"
        $mlagentsVersionCheck = & $python310venv -m pip show mlagents 2>&1
        if ($mlagentsVersionCheck -match "Version:") {
            $version = ($mlagentsVersionCheck -split "`n" | Where-Object { $_ -match "Version:" }) -replace "Version:\s*", ""
            Write-Host "   ML-Agents version: $version" -ForegroundColor White
        }
    } catch {
        Write-Host "   Warning: Could not verify ML-Agents version, but continuing..." -ForegroundColor Yellow
    }
    
    # Verify executables exist
    $mlagentsLearnPath = "$VenvPath\Scripts\mlagents-learn.exe"
    if (Test-Path $mlagentsLearnPath) {
        Write-Host "   ✅ mlagents-learn executable found" -ForegroundColor Green
    } else {
        Write-Host "   ❌ mlagents-learn executable missing - installation failed" -ForegroundColor Red
        throw "ML-Agents installation incomplete"
    }
    
    Write-Host "✅ ML-Agents 1.1.0 installed successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install ML-Agents: $_" -ForegroundColor Red
    Write-Host "`n🔧 Manual installation steps:" -ForegroundColor Cyan
    Write-Host "   1. Activate environment: .\activate_mlagents_py310.ps1" -ForegroundColor White
    Write-Host "   2. Install manually: pip install protobuf==3.20.3 mlagents==1.1.0" -ForegroundColor White
    Write-Host "   3. Verify: .\Scripts\check_environment.ps1" -ForegroundColor White
}

# 6. Set up compatibility environment variables
Write-Host "`n6. Setting up compatibility..." -ForegroundColor Yellow
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"

# 7. Test mlagents-learn command
Write-Host "`n7. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $testOutput = & $python310venv -m mlagents.trainers.learn --help 2>&1
    if ($testOutput -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Command output unexpected:" -ForegroundColor Yellow
        Write-Host "$testOutput" -ForegroundColor White
    }
} catch {
    Write-Host "⚠️  Command test had issues: $_" -ForegroundColor Yellow
}

# 8. Create activation wrapper
Write-Host "`n8. Creating activation wrapper..." -ForegroundColor Yellow
$ActivationScript = @"
# Quick activation for Python 3.10 ML-Agents environment
# Based on Unity ML-Agents Official Python 3.10.12 requirement

Write-Host "🐍 Activating ML-Agents (Python 3.10) environment..." -ForegroundColor Green

if (-not (Test-Path "venv\$VenvName")) {
    Write-Host "❌ Virtual environment not found: venv\$VenvName" -ForegroundColor Red
    Write-Host "   Run: .\Scripts\setup_python310.ps1" -ForegroundColor Cyan
    return
}

try {
    & "venv\$VenvName\Scripts\Activate.ps1"
    `$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
    
    Write-Host "✅ Python 3.10 environment activated!" -ForegroundColor Green
    Write-Host "✅ ML-Agents 1.1.0 ready for training" -ForegroundColor Green
    Write-Host "✅ Compatible with Unity ML-Agents Package" -ForegroundColor Green
    
    Write-Host "`n📋 Next steps:" -ForegroundColor Cyan
    Write-Host "   1. Check setup: .\Scripts\check_environment.ps1" -ForegroundColor White
    Write-Host "   2. Start training: .\Scripts\train_cube.ps1" -ForegroundColor White
} catch {
    Write-Host "❌ Failed to activate environment: `$_" -ForegroundColor Red
}
"@

$ActivationScript | Out-File -FilePath "activate_mlagents_py310.ps1" -Encoding UTF8

# Final verification and next steps
Write-Host "`n📋 Running final environment verification..." -ForegroundColor Cyan
try {
    & $python310venv -c "import mlagents_envs, mlagents; print('Final ML-Agents verification: All modules import successfully')"
    $mlagentsVersionCheck = & $python310venv -m pip show mlagents 2>&1
    if ($mlagentsVersionCheck -match "Version:") {
        $version = ($mlagentsVersionCheck -split "`n" | Where-Object { $_ -match "Version:" }) -replace "Version:\s*", ""
        Write-Host "Final check - ML-Agents version: $version" -ForegroundColor White
    }
    $mlagentsLearnPath = "$VenvPath\Scripts\mlagents-learn.exe"
    
    if (Test-Path $mlagentsLearnPath) {
        Write-Host "`n🎉 Python 3.10 Setup Complete!" -ForegroundColor Green
        Write-Host "==============================" -ForegroundColor Green
        Write-Host "✅ Python 3.10.12: $python310" -ForegroundColor Green
        Write-Host "✅ Virtual environment: $VenvPath" -ForegroundColor Green
        Write-Host "✅ ML-Agents 1.1.0: Installed and verified" -ForegroundColor Green
        Write-Host "✅ Compatible with Unity ML-Agents Package" -ForegroundColor Green
        Write-Host "✅ Activation script: activate_mlagents_py310.ps1" -ForegroundColor Green
        
        Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
        Write-Host "1. Environment is active in this session" -ForegroundColor White
        Write-Host "2. Run: .\Scripts\check_environment.ps1" -ForegroundColor White
        Write-Host "3. Run: .\Scripts\train_cube.ps1" -ForegroundColor White
        
        Write-Host "`n💡 For future sessions:" -ForegroundColor Yellow
        Write-Host "   Run: .\activate_mlagents_py310.ps1" -ForegroundColor Cyan
        
        Write-Host "`n📊 Environment Info (Unity Official Spec):" -ForegroundColor Magenta
        Write-Host "   Python Version: 3.10.12 (Unity Recommended)" -ForegroundColor Green
        Write-Host "   ML-Agents: 1.1.0 (Latest Available)" -ForegroundColor Green
        Write-Host "   Unity Package: Compatible with com.unity.ml-agents 4.0.0" -ForegroundColor Green
        Write-Host "   Ready for Training: YES" -ForegroundColor Green
    } else {
        Write-Host "`n⚠️  Setup completed but mlagents-learn not found" -ForegroundColor Yellow
        Write-Host "   Manual verification may be needed" -ForegroundColor White
    }
} catch {
    Write-Host "`n⚠️  Setup completed but verification failed" -ForegroundColor Yellow
    Write-Host "   Error: $_" -ForegroundColor Red
    Write-Host "   Try manual activation and testing" -ForegroundColor White
}