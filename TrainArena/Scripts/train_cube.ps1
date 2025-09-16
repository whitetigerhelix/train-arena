# ML-Agents Cube Training Script
# This script launches PPO training for the CubeAgent with proper logging and TensorBoard

param(
    [string]$RunId = "cube_run_$(Get-Date -Format 'yyyyMMdd_HHmmss')",
    [string]$ConfigPath = "Assets/ML-Agents/Configs/cube_ppo.yaml",
    [switch]$Resume,
    [switch]$SkipTensorBoard
)

Write-Host "🚀 Starting ML-Agents Cube Training" -ForegroundColor Green
Write-Host "Run ID: $RunId" -ForegroundColor Cyan
Write-Host "Config: $ConfigPath" -ForegroundColor Cyan

# Check if we're in a virtual environment (recommended)
if ($env:VIRTUAL_ENV) {
    Write-Host "✅ Using virtual environment: $env:VIRTUAL_ENV" -ForegroundColor Green
} else {
    Write-Host "⚠️  No virtual environment detected" -ForegroundColor Yellow
    Write-Host "   Consider activating ML-Agents environment: .\activate_mlagents.ps1" -ForegroundColor Cyan
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue -ne "y" -and $continue -ne "Y") {
        Write-Host "Training cancelled. Set up virtual environment first:" -ForegroundColor Red
        Write-Host "   .\Scripts\setup_venv.ps1" -ForegroundColor Cyan
        exit 1
    }
}

# Set protobuf compatibility environment variable (prevents common errors)
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
Write-Host "🔧 Set protobuf compatibility mode" -ForegroundColor Yellow

# Ensure we're in the right directory
if (!(Test-Path $ConfigPath)) {
    Write-Error "Config file not found: $ConfigPath"
    Write-Host "Current directory: $(Get-Location)"
    Write-Host "Available configs:"
    Get-ChildItem "Assets/ML-Agents/Configs" -Filter "*.yaml" | Format-Table Name
    exit 1
}

# Create results directory
$ResultsDir = "Results/$RunId"
if (!(Test-Path "Results")) {
    New-Item -ItemType Directory -Path "Results" | Out-Null
}

# Launch TensorBoard in background (unless skipped)
if (!$SkipTensorBoard) {
    Write-Host "📊 Launching TensorBoard..." -ForegroundColor Yellow
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "tensorboard --logdir=Results --port=6006; Write-Host 'TensorBoard running at http://localhost:6006' -ForegroundColor Green"
    Start-Sleep 3  # Give TensorBoard time to start
}

# Build the training command
$TrainCmd = "mlagents-learn `"$ConfigPath`" --run-id=`"$RunId`""

if ($Resume) {
    $TrainCmd += " --resume"
    Write-Host "⚡ Resuming training from checkpoint" -ForegroundColor Yellow
}

$TrainCmd += " --train"

Write-Host "📝 Training Command:" -ForegroundColor Magenta
Write-Host $TrainCmd -ForegroundColor White

Write-Host "`n🎯 Instructions:" -ForegroundColor Yellow
Write-Host "1. Open Unity and load your Cube Training Scene" -ForegroundColor White
Write-Host "2. Press PLAY in Unity when training starts" -ForegroundColor White
Write-Host "3. Watch TensorBoard at http://localhost:6006" -ForegroundColor White
Write-Host "4. Press Ctrl+C here to stop training" -ForegroundColor White

Write-Host "`n⏳ Starting training in 5 seconds..." -ForegroundColor Yellow
Start-Sleep 5

try {
    # Execute the training command
    Invoke-Expression $TrainCmd
} catch {
    Write-Error "Training failed: $_"
    exit 1
}

Write-Host "`n✅ Training completed successfully!" -ForegroundColor Green
Write-Host "Results saved to: $ResultsDir" -ForegroundColor Cyan
Write-Host "TensorBoard logs available at: Results/$RunId" -ForegroundColor Cyan