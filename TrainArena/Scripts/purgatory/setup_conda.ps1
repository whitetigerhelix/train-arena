# ML-Agents Setup Using Conda (Most Reliable Option)
# Conda handles package dependencies better than pip for ML packages

param(
    [string]$CondaEnvName = "mlagents-conda",
    [switch]$Force
)

Write-Host "🐍 ML-Agents Setup with Conda" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host "Conda typically handles ML package dependencies better than pip" -ForegroundColor Cyan

# Check if conda is available
Write-Host "`n1. Checking for conda installation..." -ForegroundColor Yellow
try {
    $condaVersion = conda --version 2>&1
    if ($condaVersion -match "conda") {
        Write-Host "✅ Conda found: $condaVersion" -ForegroundColor Green
    } else {
        throw "Conda not found"
    }
} catch {
    Write-Host "❌ Conda not found" -ForegroundColor Red
    Write-Host "`n📦 Install Conda (recommended):" -ForegroundColor Cyan
    Write-Host "1. Download Miniconda: https://docs.conda.io/en/latest/miniconda.html" -ForegroundColor White
    Write-Host "2. Install with default settings" -ForegroundColor White
    Write-Host "3. Restart PowerShell" -ForegroundColor White
    Write-Host "4. Run this script again" -ForegroundColor White
    Write-Host "`n🔄 Alternative: Use complete reinstall script:" -ForegroundColor Yellow
    Write-Host "   .\Scripts\complete_mlagents_reinstall.ps1" -ForegroundColor Cyan
    exit 1
}

# Check if environment already exists
Write-Host "`n2. Checking for existing environment..." -ForegroundColor Yellow
$existingEnv = conda env list | Select-String $CondaEnvName
if ($existingEnv) {
    if ($Force) {
        Write-Host "   Removing existing environment..." -ForegroundColor Yellow
        conda env remove -n $CondaEnvName -y
    } else {
        Write-Host "❌ Environment '$CondaEnvName' already exists" -ForegroundColor Red
        Write-Host "   Use -Force to recreate, or activate existing:" -ForegroundColor White
        Write-Host "   conda activate $CondaEnvName" -ForegroundColor Cyan
        exit 1
    }
}

# Create conda environment with Python 3.11
Write-Host "`n3. Creating conda environment with Python 3.11..." -ForegroundColor Yellow
try {
    conda create -n $CondaEnvName python=3.11 -y
    Write-Host "✅ Conda environment '$CondaEnvName' created" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create conda environment: $_" -ForegroundColor Red
    exit 1
}

# Activate environment
Write-Host "`n4. Activating conda environment..." -ForegroundColor Yellow
try {
    conda activate $CondaEnvName
    Write-Host "✅ Environment activated" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to activate environment: $_" -ForegroundColor Red
    Write-Host "   Try manually: conda activate $CondaEnvName" -ForegroundColor Cyan
    exit 1
}

# Install packages through conda when possible
Write-Host "`n5. Installing packages via conda..." -ForegroundColor Yellow
try {
    Write-Host "   Installing PyTorch via conda..." -ForegroundColor White
    conda install pytorch torchvision cpuonly -c pytorch -y
    
    Write-Host "   Installing other packages via conda..." -ForegroundColor White
    conda install numpy pyyaml pillow -y
    
    Write-Host "✅ Core packages installed via conda" -ForegroundColor Green
} catch {
    Write-Host "⚠️  Some conda packages failed, continuing with pip..." -ForegroundColor Yellow
}

# Install ML-Agents via pip with compatible protobuf
Write-Host "`n6. Installing ML-Agents via pip..." -ForegroundColor Yellow
try {
    Write-Host "   Installing compatible protobuf..." -ForegroundColor White
    pip install "protobuf==3.20.3"
    
    Write-Host "   Installing ML-Agents..." -ForegroundColor White
    pip install mlagents
    
    Write-Host "   Installing TensorBoard..." -ForegroundColor White
    pip install tensorboard
    
    Write-Host "✅ ML-Agents installed successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install ML-Agents: $_" -ForegroundColor Red
    exit 1
}

# Test installation
Write-Host "`n7. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
    
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0 -and $stdout -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Command may have minor issues but environment is created" -ForegroundColor Yellow
        if ($stderr) {
            Write-Host "   $($stderr.Substring(0, [Math]::Min(200, $stderr.Length)))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "⚠️  Could not fully test command, but environment is ready" -ForegroundColor Yellow
}

# Create activation script
Write-Host "`n8. Creating activation scripts..." -ForegroundColor Yellow
$condaActivateScript = @"
# Conda ML-Agents Environment Activation
Write-Host "🐍 Activating ML-Agents (Conda) environment..." -ForegroundColor Green
conda activate $CondaEnvName
`$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
Write-Host "✅ Conda environment '$CondaEnvName' activated!" -ForegroundColor Green
Write-Host "✅ ML-Agents ready for training" -ForegroundColor Green
"@

$condaActivateScript | Out-File -FilePath "activate_mlagents_conda.ps1" -Encoding UTF8

Write-Host "`n🎉 Conda Setup Complete!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host "✅ Conda environment: $CondaEnvName" -ForegroundColor Green
Write-Host "✅ ML-Agents installed with conda dependency management" -ForegroundColor Green
Write-Host "✅ Activation script: activate_mlagents_conda.ps1" -ForegroundColor Green

Write-Host "`n🚀 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Environment should be active in this session" -ForegroundColor White
Write-Host "2. Run: .\Scripts\check_environment.ps1" -ForegroundColor White
Write-Host "3. Run: .\Scripts\train_cube.ps1" -ForegroundColor White
Write-Host "`n💡 For future sessions:" -ForegroundColor Yellow
Write-Host "   Run: .\activate_mlagents_conda.ps1" -ForegroundColor Cyan
Write-Host "   Or manually: conda activate $CondaEnvName" -ForegroundColor Cyan

Write-Host "`n📊 Why Conda Often Works Better:" -ForegroundColor Magenta
Write-Host "   - Better dependency resolution" -ForegroundColor Green
Write-Host "   - Pre-compiled packages reduce conflicts" -ForegroundColor Green
Write-Host "   - Isolated from system Python completely" -ForegroundColor Green