# Fix corrupted pip and install ML-Agents properly
# Handles pip corruption and ensures complete ML-Agents installation

param(
    [switch]$Force
)

Write-Host "🔧 Fixing Pip Corruption and Installing ML-Agents" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Check if we're in the Python 3.11 environment
if (!$env:VIRTUAL_ENV) {
    Write-Host "❌ No virtual environment detected" -ForegroundColor Red
    Write-Host "   Please activate your Python 3.11 environment first:" -ForegroundColor White
    Write-Host "   .\activate_mlagents_py311.ps1" -ForegroundColor Cyan
    exit 1
}

if ($env:VIRTUAL_ENV -notmatch "py311") {
    Write-Host "⚠️  This doesn't look like the Python 3.11 environment" -ForegroundColor Yellow
    Write-Host "   Current environment: $env:VIRTUAL_ENV" -ForegroundColor White
}

Write-Host "✅ Environment: $env:VIRTUAL_ENV" -ForegroundColor Green

# Step 1: Fix corrupted pip installation
Write-Host "`n1. Fixing corrupted pip installation..." -ForegroundColor Yellow
try {
    Write-Host "   Downloading fresh pip installer..." -ForegroundColor White
    
    # Download get-pip.py directly from PyPA
    $pipInstaller = "get-pip.py"
    if (Test-Path $pipInstaller) {
        Remove-Item $pipInstaller
    }
    
    Invoke-WebRequest -Uri "https://bootstrap.pypa.io/get-pip.py" -OutFile $pipInstaller
    
    Write-Host "   Installing fresh pip..." -ForegroundColor White
    python $pipInstaller --force-reinstall
    
    # Clean up
    Remove-Item $pipInstaller -ErrorAction SilentlyContinue
    
    Write-Host "✅ Pip reinstalled successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to fix pip: $_" -ForegroundColor Red
    Write-Host "   Trying alternative method..." -ForegroundColor Yellow
    
    try {
        # Alternative: use ensurepip
        python -m ensurepip --upgrade --force
        Write-Host "✅ Pip fixed using ensurepip" -ForegroundColor Green
    } catch {
        Write-Host "❌ Could not fix pip installation" -ForegroundColor Red
        Write-Host "`n🆘 Manual fix required:" -ForegroundColor Cyan
        Write-Host "1. Delete the environment: Remove-Item -Recurse -Force venv\mlagents-py311" -ForegroundColor White
        Write-Host "2. Recreate: .\Scripts\setup_python311.ps1 -Force" -ForegroundColor White
        Write-Host "3. Or try conda: .\Scripts\setup_conda.ps1" -ForegroundColor White
        exit 1
    }
}

# Step 2: Verify pip is working
Write-Host "`n2. Verifying pip installation..." -ForegroundColor Yellow
try {
    $pipVersion = python -m pip --version
    Write-Host "✅ Pip working: $pipVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Pip still not working properly" -ForegroundColor Red
    exit 1
}

# Step 3: Install core dependencies first
Write-Host "`n3. Installing core dependencies..." -ForegroundColor Yellow
try {
    Write-Host "   Installing setuptools and wheel..." -ForegroundColor White
    python -m pip install --upgrade setuptools wheel
    
    Write-Host "   Installing protobuf 3.20.3..." -ForegroundColor White
    python -m pip install "protobuf==3.20.3"
    
    Write-Host "   Installing numpy..." -ForegroundColor White
    python -m pip install "numpy>=1.21.0,<2.0.0"
    
    Write-Host "   Installing PyYAML..." -ForegroundColor White
    python -m pip install "PyYAML>=5.1.0"
    
    Write-Host "✅ Core dependencies installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install core dependencies: $_" -ForegroundColor Red
    exit 1
}

# Step 4: Install PyTorch
Write-Host "`n4. Installing PyTorch..." -ForegroundColor Yellow
try {
    Write-Host "   Installing PyTorch (CPU version for compatibility)..." -ForegroundColor White
    python -m pip install torch torchvision --index-url https://download.pytorch.org/whl/cpu
    
    Write-Host "✅ PyTorch installed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  PyTorch installation failed, trying standard version..." -ForegroundColor Yellow
    try {
        python -m pip install torch
        Write-Host "✅ PyTorch (standard) installed" -ForegroundColor Green
    } catch {
        Write-Host "❌ Could not install PyTorch: $_" -ForegroundColor Red
        Write-Host "   Continuing without PyTorch - ML-Agents will install its own" -ForegroundColor Yellow
    }
}

# Step 5: Install ML-Agents
Write-Host "`n5. Installing ML-Agents..." -ForegroundColor Yellow
try {
    Write-Host "   Installing mlagents package..." -ForegroundColor White
    python -m pip install mlagents
    
    Write-Host "   Verifying mlagents installation..." -ForegroundColor White
    python -c "import mlagents_envs; print('ML-Agents environments import successful')"
    
    Write-Host "✅ ML-Agents installed and verified" -ForegroundColor Green
} catch {
    Write-Host "❌ ML-Agents installation failed: $_" -ForegroundColor Red
    
    # Try older version
    Write-Host "   Trying older ML-Agents version..." -ForegroundColor Yellow
    try {
        python -m pip install "mlagents==0.27.0"
        Write-Host "✅ ML-Agents 0.27.0 installed" -ForegroundColor Green
    } catch {
        Write-Host "❌ Could not install any ML-Agents version" -ForegroundColor Red
        exit 1
    }
}

# Step 6: Install TensorBoard
Write-Host "`n6. Installing TensorBoard..." -ForegroundColor Yellow
try {
    python -m pip install tensorboard
    Write-Host "✅ TensorBoard installed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  TensorBoard installation failed, but ML-Agents should work" -ForegroundColor Yellow
}

# Step 7: Set environment variables
Write-Host "`n7. Setting compatibility environment variables..." -ForegroundColor Yellow
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
$env:PYTHONPATH = "$env:VIRTUAL_ENV\Lib\site-packages"
Write-Host "✅ Environment variables set" -ForegroundColor Green

# Step 8: Test mlagents-learn
Write-Host "`n8. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0 -and $stdout -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
        
        # Update our custom activation script (don't modify signed scripts)
        Write-Host "`n9. Updating activation wrapper..." -ForegroundColor Yellow
        
        # Update the activate_mlagents_py311.ps1 to include environment variables
        $activationWrapper = "activate_mlagents_py311.ps1"
        if (Test-Path $activationWrapper) {
            $content = Get-Content $activationWrapper -Raw
            if ($content -notmatch "PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION") {
                # Add environment variable setting after the activation line
                $newContent = $content -replace '(& "venv\\mlagents-py311\\Scripts\\Activate\.ps1")', '$1`n`n# Set compatibility environment variables`n$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"'
                $newContent | Out-File -FilePath $activationWrapper -Encoding UTF8
                Write-Host "✅ Activation wrapper updated with environment variables" -ForegroundColor Green
            } else {
                Write-Host "✅ Activation wrapper already configured" -ForegroundColor Green
            }
        } else {
            Write-Host "⚠️  Activation wrapper not found, but environment is ready" -ForegroundColor Yellow
        }
        
    } else {
        Write-Host "❌ mlagents-learn still not working:" -ForegroundColor Red
        if ($stderr) {
            Write-Host "   Error: $($stderr.Substring(0, [Math]::Min(300, $stderr.Length)))" -ForegroundColor White
        }
        
        Write-Host "`n🔄 Try these alternatives:" -ForegroundColor Cyan
        Write-Host "1. Use conda (often more reliable):" -ForegroundColor White
        Write-Host "   .\Scripts\setup_conda.ps1" -ForegroundColor Gray
        Write-Host "2. Fresh environment with different approach:" -ForegroundColor White
        Write-Host "   Remove-Item -Recurse -Force venv\mlagents-py311" -ForegroundColor Gray
        Write-Host "   python -m venv venv\mlagents-fresh" -ForegroundColor Gray
        Write-Host "   .\venv\mlagents-fresh\Scripts\Activate.ps1" -ForegroundColor Gray
        Write-Host "   python -m pip install --upgrade pip" -ForegroundColor Gray
        Write-Host "   python -m pip install protobuf==3.20.3 mlagents" -ForegroundColor Gray
        exit 1
    }
} catch {
    Write-Host "❌ Could not test mlagents-learn: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Complete Fix Successful!" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green
Write-Host "✅ Pip corruption fixed" -ForegroundColor Green
Write-Host "✅ ML-Agents properly installed" -ForegroundColor Green
Write-Host "✅ mlagents-learn command working" -ForegroundColor Green
Write-Host "✅ Environment configured for training" -ForegroundColor Green

Write-Host "`n🚀 Ready to Train!" -ForegroundColor Cyan
Write-Host "   Run: .\Scripts\check_environment.ps1 (should pass all tests)" -ForegroundColor White
Write-Host "   Then: .\Scripts\train_cube.ps1" -ForegroundColor White