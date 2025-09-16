# Complete ML-Agents reinstall for Python 3.11 with proper dependency management
# This fixes stubborn protobuf and dependency issues

Write-Host "🔧 Complete ML-Agents Reinstall (Python 3.11)" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# Check environment
if (!$env:VIRTUAL_ENV) {
    Write-Host "❌ No virtual environment detected" -ForegroundColor Red
    Write-Host "   Please activate your Python 3.11 environment first:" -ForegroundColor White
    Write-Host "   .\activate_mlagents_py311.ps1" -ForegroundColor Cyan
    exit 1
}

Write-Host "✅ Virtual environment: $env:VIRTUAL_ENV" -ForegroundColor Green

# Step 1: Complete cleanup
Write-Host "`n1. Cleaning up existing ML-Agents installation..." -ForegroundColor Yellow
try {
    Write-Host "   Uninstalling mlagents packages..." -ForegroundColor White
    python -m pip uninstall mlagents mlagents-envs -y
    
    Write-Host "   Uninstalling problematic packages..." -ForegroundColor White
    python -m pip uninstall protobuf grpcio tensorboard torch -y
    
    Write-Host "✅ Cleanup complete" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Cleanup had some issues, continuing..." -ForegroundColor Yellow
}

# Step 2: Install core dependencies in correct order
Write-Host "`n2. Installing core dependencies in proper order..." -ForegroundColor Yellow
try {
    Write-Host "   Installing protobuf 3.20.3..." -ForegroundColor White
    python -m pip install "protobuf==3.20.3"
    
    Write-Host "   Installing grpcio..." -ForegroundColor White
    python -m pip install "grpcio>=1.11.0,<2.0.0"
    
    Write-Host "   Installing numpy..." -ForegroundColor White
    python -m pip install "numpy>=1.21.0,<2.0.0"
    
    Write-Host "   Installing PyYAML..." -ForegroundColor White
    python -m pip install "PyYAML>=5.1.0"
    
    Write-Host "✅ Core dependencies installed" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install core dependencies: $_" -ForegroundColor Red
    exit 1
}

# Step 3: Install torch (CPU version for compatibility)
Write-Host "`n3. Installing PyTorch (CPU version)..." -ForegroundColor Yellow
try {
    Write-Host "   Installing PyTorch CPU..." -ForegroundColor White
    python -m pip install torch torchvision --index-url https://download.pytorch.org/whl/cpu
    
    Write-Host "✅ PyTorch installed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  PyTorch installation had issues, trying alternative..." -ForegroundColor Yellow
    try {
        python -m pip install torch
        Write-Host "✅ PyTorch installed (standard version)" -ForegroundColor Green
    } catch {
        Write-Host "❌ Could not install PyTorch: $_" -ForegroundColor Red
        exit 1
    }
}

# Step 4: Install TensorBoard
Write-Host "`n4. Installing TensorBoard..." -ForegroundColor Yellow
try {
    python -m pip install "tensorboard>=2.8.0,<3.0.0"
    Write-Host "✅ TensorBoard installed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  TensorBoard installation issues, continuing..." -ForegroundColor Yellow
}

# Step 5: Install ML-Agents with no-deps to avoid conflicts
Write-Host "`n5. Installing ML-Agents (controlled installation)..." -ForegroundColor Yellow
try {
    Write-Host "   Installing mlagents-envs..." -ForegroundColor White
    python -m pip install mlagents-envs --no-deps
    
    Write-Host "   Installing mlagents..." -ForegroundColor White
    python -m pip install mlagents --no-deps
    
    Write-Host "   Installing remaining ML-Agents dependencies..." -ForegroundColor White
    python -m pip install gym cloudpickle pettingzoo cattrs
    
    Write-Host "✅ ML-Agents installed with controlled dependencies" -ForegroundColor Green
} catch {
    Write-Host "❌ ML-Agents installation failed: $_" -ForegroundColor Red
    Write-Host "`n   Trying standard installation as fallback..." -ForegroundColor Yellow
    
    try {
        python -m pip install mlagents==0.30.0  # Try newer version
        Write-Host "✅ ML-Agents 0.30.0 installed" -ForegroundColor Green
    } catch {
        try {
            python -m pip install mlagents==0.27.0  # Try older stable version
            Write-Host "✅ ML-Agents 0.27.0 installed" -ForegroundColor Green
        } catch {
            Write-Host "❌ Could not install any ML-Agents version" -ForegroundColor Red
            exit 1
        }
    }
}

# Step 6: Set environment variables
Write-Host "`n6. Setting compatibility environment variables..." -ForegroundColor Yellow
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
$env:PYTHONPATH = "$env:VIRTUAL_ENV\Lib\site-packages"
Write-Host "✅ Environment variables set" -ForegroundColor Green

# Step 7: Verify package versions
Write-Host "`n7. Verifying installed packages..." -ForegroundColor Yellow
try {
    Write-Host "Package versions:" -ForegroundColor White
    $packages = python -m pip list | Select-String -Pattern "(mlagents|protobuf|torch|tensorboard|grpcio)"
    $packages | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
    
    Write-Host "`nTesting protobuf import..." -ForegroundColor White
    python -c "import google.protobuf; print(f'Protobuf {google.protobuf.__version__} works')"
    
    Write-Host "Testing torch import..." -ForegroundColor White
    python -c "import torch; print(f'PyTorch {torch.__version__} works')"
    
    Write-Host "Testing mlagents import..." -ForegroundColor White
    python -c "import mlagents_envs; print('ML-Agents environments import successful')"
    
    Write-Host "✅ All key packages import successfully" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Some package verification failed, but continuing to test command..." -ForegroundColor Yellow
}

# Step 8: Test mlagents-learn command
Write-Host "`n8. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0 -and $stdout -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
        
        # Make settings permanent
        Write-Host "`n9. Making settings permanent..." -ForegroundColor Yellow
        $activateScript = "$env:VIRTUAL_ENV\Scripts\activate.bat"
        if (Test-Path $activateScript) {
            $content = Get-Content $activateScript -Raw
            if ($content -notmatch "PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION") {
                Add-Content $activateScript "`nset PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python"
                Add-Content $activateScript "set PYTHONPATH=$env:VIRTUAL_ENV\Lib\site-packages"
                Write-Host "✅ Environment variables added to activation script" -ForegroundColor Green
            }
        }
        
        $psActivateScript = "$env:VIRTUAL_ENV\Scripts\Activate.ps1"
        if (Test-Path $psActivateScript) {
            $content = Get-Content $psActivateScript -Raw
            if ($content -notmatch "PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION") {
                Add-Content $psActivateScript "`n`$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = 'python'"
                Add-Content $psActivateScript "`$env:PYTHONPATH = '$env:VIRTUAL_ENV\Lib\site-packages'"
                Write-Host "✅ Environment variables added to PowerShell activation script" -ForegroundColor Green
            }
        }
        
    } else {
        Write-Host "❌ Command still failing:" -ForegroundColor Red
        if ($stderr) {
            # Show first 10 lines of error
            $errorLines = $stderr.Split("`n") | Select-Object -First 10
            Write-Host "   Error details:" -ForegroundColor White
            $errorLines | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        }
        
        Write-Host "`n🆘 Final troubleshooting steps:" -ForegroundColor Cyan
        Write-Host "1. Try a fresh Python 3.11 environment:" -ForegroundColor White
        Write-Host "   Remove-Item -Recurse -Force venv\mlagents-py311" -ForegroundColor Gray
        Write-Host "   .\Scripts\setup_python311.ps1 -Force" -ForegroundColor Gray
        Write-Host "2. Use conda instead of venv:" -ForegroundColor White
        Write-Host "   conda create -n mlagents python=3.11" -ForegroundColor Gray
        Write-Host "   conda activate mlagents" -ForegroundColor Gray
        Write-Host "   pip install protobuf==3.20.3 mlagents" -ForegroundColor Gray
        exit 1
    }
} catch {
    Write-Host "❌ Could not test command: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Complete Reinstall Successful!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "✅ Clean ML-Agents installation" -ForegroundColor Green
Write-Host "✅ Compatible package versions" -ForegroundColor Green
Write-Host "✅ mlagents-learn command working" -ForegroundColor Green
Write-Host "✅ Environment variables configured" -ForegroundColor Green

Write-Host "`n🚀 Ready for Training!" -ForegroundColor Cyan
Write-Host "   Run: .\Scripts\check_environment.ps1" -ForegroundColor White
Write-Host "   Then: .\Scripts\train_cube.ps1" -ForegroundColor White