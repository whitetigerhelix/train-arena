# ML-Agents Environment Activation (Python 3.11)
# Activates the Python 3.11 virtual environment and sets compatibility variables

Write-Host "🐍 Activating ML-Agents (Python 3.11) environment..." -ForegroundColor Green

# Check if environment exists
if (!(Test-Path "venv\mlagents-py311\Scripts\Activate.ps1")) {
    Write-Host "❌ Environment not found: venv\mlagents-py311\" -ForegroundColor Red
    Write-Host "   Create it first: .\Scripts\setup_python311.ps1" -ForegroundColor Cyan
    exit 1
}

# Activate the virtual environment
& "venv\mlagents-py311\Scripts\Activate.ps1"

# Set compatibility environment variables
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"

Write-Host "✅ Python 3.11 environment activated!" -ForegroundColor Green
Write-Host "✅ Protobuf compatibility mode enabled" -ForegroundColor Green
Write-Host "✅ Ready for ML-Agents training" -ForegroundColor Green

Write-Host "`n🚀 Next steps:" -ForegroundColor Cyan
Write-Host "   .\Scripts\check_environment.ps1  # Verify setup" -ForegroundColor White
Write-Host "   .\Scripts\train_cube.ps1         # Start training" -ForegroundColor White
