# Quick activation for Python 3.10 ML-Agents environment
# Based on Unity ML-Agents Official Python 3.10.12 requirement

Write-Host "🐍 Activating ML-Agents (Python 3.10) environment..." -ForegroundColor Green

if (-not (Test-Path "venv\mlagents-py310")) {
    Write-Host "❌ Virtual environment not found: venv\mlagents-py310" -ForegroundColor Red
    Write-Host "   Run: .\Scripts\setup_python310.ps1" -ForegroundColor Cyan
    return
}

try {
    & "venv\mlagents-py310\Scripts\Activate.ps1"
    $env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
    $env:CUDA_VISIBLE_DEVICES = ""  # Force CPU-only operation
    
    Write-Host "✅ Python 3.10 environment activated!" -ForegroundColor Green
    Write-Host "✅ ML-Agents 0.30.0 ready for training" -ForegroundColor Green
    Write-Host "✅ Compatible with Unity ML-Agents Package" -ForegroundColor Green
    
    Write-Host "
📋 Next steps:" -ForegroundColor Cyan
    Write-Host "   1. Check setup: .\Scripts\check_environment.ps1" -ForegroundColor White
    Write-Host "   2. Start training: .\Scripts\train_cube.ps1" -ForegroundColor White
} catch {
    Write-Host "❌ Failed to activate environment: $_" -ForegroundColor Red
}
