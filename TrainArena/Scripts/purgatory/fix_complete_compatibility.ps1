# ML-Agents Complete Compatibility Fix
# Addresses Python 3.12 + package version compatibility issues

param(
    [switch]$Force
)

Write-Host "🔧 ML-Agents Complete Compatibility Fix" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Check if virtual environment is active
if (!$env:VIRTUAL_ENV) {
    Write-Host "❌ No virtual environment detected" -ForegroundColor Red
    Write-Host "   Please activate your ML-Agents environment first:" -ForegroundColor White
    Write-Host "   .\activate_mlagents.ps1" -ForegroundColor Cyan
    exit 1
}

Write-Host "✅ Virtual environment active: $env:VIRTUAL_ENV" -ForegroundColor Green

# Check Python version
Write-Host "`n1. Checking Python version compatibility..." -ForegroundColor Yellow
try {
    $pythonVersion = python -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}.{sys.version_info.micro}')"
    Write-Host "   Python version: $pythonVersion" -ForegroundColor White
    
    # Parse version
    $versionParts = $pythonVersion.Split('.')
    $major = [int]$versionParts[0]
    $minor = [int]$versionParts[1]
    
    if ($major -eq 3 -and $minor -ge 12) {
        Write-Host "⚠️  Python 3.12+ detected - ML-Agents has compatibility issues" -ForegroundColor Yellow
        Write-Host "   ML-Agents works best with Python 3.8-3.11" -ForegroundColor White
        
        if (!$Force) {
            Write-Host "`n💡 Recommended solutions:" -ForegroundColor Cyan
            Write-Host "1. Install Python 3.11 and recreate virtual environment" -ForegroundColor White
            Write-Host "2. Use -Force flag to try compatibility fixes anyway" -ForegroundColor White
            $continue = Read-Host "`nTry compatibility fixes anyway? (y/N)"
            if ($continue -ne "y" -and $continue -ne "Y") {
                Write-Host "Fix cancelled. Consider using Python 3.11 for best compatibility." -ForegroundColor Yellow
                exit 1
            }
        }
    } else {
        Write-Host "✅ Python version is compatible with ML-Agents" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Could not determine Python version" -ForegroundColor Yellow
}

# Fix package compatibility issues
Write-Host "`n2. Installing compatible package versions..." -ForegroundColor Yellow
try {
    Write-Host "   Installing specific package versions for compatibility..." -ForegroundColor White
    
    # Install compatible versions in specific order
    python -m pip install "setuptools<66.0.0" --force-reinstall
    python -m pip install "protobuf==3.20.3" --force-reinstall  
    python -m pip install "cattr==1.10.0" --force-reinstall
    python -m pip install "cattrs==1.10.0" --force-reinstall
    python -m pip install "attrs==21.4.0" --force-reinstall
    
    Write-Host "✅ Compatible package versions installed" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Some package installations may have failed, continuing..." -ForegroundColor Yellow
}

# Reinstall ML-Agents with compatible dependencies
Write-Host "`n3. Reinstalling ML-Agents with fixed dependencies..." -ForegroundColor Yellow
try {
    Write-Host "   Uninstalling existing ML-Agents..." -ForegroundColor White
    python -m pip uninstall mlagents mlagents-envs -y
    
    Write-Host "   Installing ML-Agents with no-deps to avoid conflicts..." -ForegroundColor White
    python -m pip install mlagents --no-deps
    python -m pip install mlagents-envs --no-deps
    
    # Install required dependencies manually with compatible versions
    Write-Host "   Installing required dependencies..." -ForegroundColor White
    python -m pip install "torch>=1.8.0"
    python -m pip install "numpy>=1.19.0"
    python -m pip install "pyyaml>=5.1.0"
    python -m pip install "pillow>=7.1.0"
    python -m pip install "tensorboard>=2.2.0"
    python -m pip install "grpcio>=1.11.0"
    
    Write-Host "✅ ML-Agents reinstalled with compatible dependencies" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to reinstall ML-Agents: $_" -ForegroundColor Red
    
    Write-Host "`n   Trying alternative approach..." -ForegroundColor Yellow
    try {
        # Try older ML-Agents version
        python -m pip install "mlagents==0.27.0" --force-reinstall
        Write-Host "✅ Installed older ML-Agents version (0.27.0)" -ForegroundColor Green
    } catch {
        Write-Host "❌ Could not install any ML-Agents version" -ForegroundColor Red
        exit 1
    }
}

# Set environment variables for compatibility
Write-Host "`n4. Setting compatibility environment variables..." -ForegroundColor Yellow
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
$env:PYTHONPATH = "$env:VIRTUAL_ENV\Lib\site-packages"

Write-Host "✅ Environment variables set for compatibility" -ForegroundColor Green

# Test the installation
Write-Host "`n5. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ mlagents-learn command works!" -ForegroundColor Green
        
        # Save environment settings to activation script
        $activateScript = "$env:VIRTUAL_ENV\Scripts\activate.bat"
        if (Test-Path $activateScript) {
            Add-Content $activateScript "`nset PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python"
            Write-Host "✅ Compatibility settings saved to activation script" -ForegroundColor Green
        }
        
    } else {
        Write-Host "❌ Command still failing" -ForegroundColor Red
        if ($stderr) {
            # Show first few lines of error for diagnosis
            $errorLines = $stderr.Split("`n") | Select-Object -First 10
            Write-Host "   Error sample:" -ForegroundColor White
            $errorLines | ForEach-Object { Write-Host "   $_" -ForegroundColor Gray }
        }
        
        Write-Host "`n📋 Final troubleshooting options:" -ForegroundColor Cyan
        Write-Host "1. Use Python 3.11 instead of 3.12:" -ForegroundColor White
        Write-Host "   - Install Python 3.11 from python.org" -ForegroundColor Gray
        Write-Host "   - Recreate virtual environment with Python 3.11" -ForegroundColor Gray
        Write-Host "2. Try conda instead of venv:" -ForegroundColor White
        Write-Host "   conda create -n mlagents python=3.11" -ForegroundColor Gray
        Write-Host "   conda activate mlagents" -ForegroundColor Gray
        Write-Host "   pip install mlagents" -ForegroundColor Gray
        exit 1
    }
} catch {
    Write-Host "❌ Could not test command: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Compatibility Fix Complete!" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host "✅ ML-Agents should now work with your Python version" -ForegroundColor Green
Write-Host "✅ Compatibility environment variables set" -ForegroundColor Green
Write-Host "✅ Package versions fixed for stability" -ForegroundColor Green

Write-Host "`n🚀 Ready for training!" -ForegroundColor Cyan
Write-Host "   Run: .\Scripts\check_environment.ps1" -ForegroundColor White
Write-Host "   Then: .\Scripts\train_cube.ps1" -ForegroundColor White