# Quick fix for protobuf issue in Python 3.11 environment
# Run this after installing mlagents in your Python 3.11 environment

Write-Host "🔧 Fixing Protobuf Compatibility (Python 3.11 Environment)" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

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
    Write-Host "   Expected to see 'py311' in the path" -ForegroundColor White
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        exit 1
    }
}

Write-Host "✅ Python 3.11 environment detected: $env:VIRTUAL_ENV" -ForegroundColor Green

# Fix protobuf version
Write-Host "`n1. Installing compatible protobuf version..." -ForegroundColor Yellow
try {
    Write-Host "   Downgrading protobuf to 3.20.3..." -ForegroundColor White
    python -m pip install "protobuf==3.20.3" --force-reinstall
    
    $protobufVersion = python -c "import google.protobuf; print(google.protobuf.__version__)"
    Write-Host "✅ Protobuf version: $protobufVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to install protobuf: $_" -ForegroundColor Red
    exit 1
}

# Set environment variable for additional safety
Write-Host "`n2. Setting compatibility environment variable..." -ForegroundColor Yellow
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
Write-Host "✅ Set PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python" -ForegroundColor Green

# Test mlagents-learn
Write-Host "`n3. Testing mlagents-learn command..." -ForegroundColor Yellow
try {
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "--help" -NoNewWindow -Wait -PassThru -RedirectStandardOutput "temp_output.txt" -RedirectStandardError "temp_error.txt"
    $stdout = Get-Content "temp_output.txt" -Raw -ErrorAction SilentlyContinue
    $stderr = Get-Content "temp_error.txt" -Raw -ErrorAction SilentlyContinue
    Remove-Item "temp_output.txt" -ErrorAction SilentlyContinue
    Remove-Item "temp_error.txt" -ErrorAction SilentlyContinue
    
    if ($process.ExitCode -eq 0 -and $stdout -match "usage:") {
        Write-Host "✅ mlagents-learn command works perfectly!" -ForegroundColor Green
    } else {
        Write-Host "❌ Still having issues:" -ForegroundColor Red
        if ($stderr) {
            Write-Host "   Error: $($stderr.Substring(0, [Math]::Min(500, $stderr.Length)))" -ForegroundColor White
        }
        exit 1
    }
} catch {
    Write-Host "❌ Could not test command: $_" -ForegroundColor Red
    exit 1
}

# Make the fix permanent by updating activation script
Write-Host "`n4. Making fix permanent..." -ForegroundColor Yellow
$activateScript = "$env:VIRTUAL_ENV\Scripts\activate.bat"
if (Test-Path $activateScript) {
    $content = Get-Content $activateScript
    if ($content -notmatch "PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION") {
        Add-Content $activateScript "`nset PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python"
        Write-Host "✅ Added environment variable to activation script" -ForegroundColor Green
    } else {
        Write-Host "✅ Environment variable already in activation script" -ForegroundColor Green
    }
}

# Update PowerShell activation script too
$psActivateScript = "$env:VIRTUAL_ENV\Scripts\Activate.ps1"
if (Test-Path $psActivateScript) {
    $content = Get-Content $psActivateScript -Raw
    if ($content -notmatch "PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION") {
        Add-Content $psActivateScript "`n`$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = 'python'"
        Write-Host "✅ Added environment variable to PowerShell activation script" -ForegroundColor Green
    }
}

Write-Host "`n🎉 Protobuf Fix Complete!" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host "✅ Python 3.11 environment ready" -ForegroundColor Green
Write-Host "✅ Protobuf compatibility fixed" -ForegroundColor Green
Write-Host "✅ mlagents-learn command working" -ForegroundColor Green
Write-Host "✅ Fix will persist in future sessions" -ForegroundColor Green

Write-Host "`n🚀 Ready for Training!" -ForegroundColor Cyan
Write-Host "   Run: .\Scripts\check_environment.ps1 (should pass all tests)" -ForegroundColor White
Write-Host "   Then: .\Scripts\train_cube.ps1" -ForegroundColor White