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

# Check if we're in a virtual environment (required for training)
if ($env:VIRTUAL_ENV) {
    Write-Host "✅ Using virtual environment: $env:VIRTUAL_ENV" -ForegroundColor Green
    
    # Check environment compatibility
    if ($env:VIRTUAL_ENV -match "mlagents-py310") {
        Write-Host "   ✅ Python 3.10 environment (Unity recommended)" -ForegroundColor Green
    } elseif ($env:VIRTUAL_ENV -match "mlagents-py311") {
        Write-Host "   ⚠️  Python 3.11 environment - may have issues" -ForegroundColor Yellow
        Write-Host "   Consider using Python 3.10: .\Scripts\setup_python310.ps1" -ForegroundColor Cyan
    }
} else {
    Write-Host "❌ No virtual environment detected" -ForegroundColor Red
    Write-Host "   Setup (if needed):" -ForegroundColor White
    Write-Host "   .\Scripts\setup_python310.ps1" -ForegroundColor Cyan
    Write-Host "   Important! Activate environment before running this script:" -ForegroundColor White
    Write-Host "   .\Scripts\activate_mlagents_py310.ps1" -ForegroundColor Cyan
    exit 1
}

# Set compatibility environment variables (prevents protobuf errors)
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
Write-Host "🔧 Protobuf compatibility mode enabled" -ForegroundColor Yellow

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

# Execute the training command and capture the exit code
Write-Host "`n🚀 Launching ML-Agents training..." -ForegroundColor Green
try {
    # Use Start-Process to properly capture exit codes
    $process = Start-Process -FilePath "mlagents-learn" -ArgumentList "`"$ConfigPath`"", "--run-id=`"$RunId`"", "--train" -NoNewWindow -Wait -PassThru
    
    if ($process.ExitCode -eq 0) {
        Write-Host "`n✅ Training completed successfully!" -ForegroundColor Green
        Write-Host "Results saved to: Results/$RunId" -ForegroundColor Cyan
        Write-Host "TensorBoard logs available at: Results/$RunId" -ForegroundColor Cyan
        
        # Check if .onnx model was created
        $onnxFiles = Get-ChildItem "Results/$RunId" -Filter "*.onnx" -Recurse
        if ($onnxFiles) {
            Write-Host "`n🧠 Trained models:" -ForegroundColor Yellow
            foreach ($file in $onnxFiles) {
                Write-Host "   📄 $($file.FullName)" -ForegroundColor White
            }
        }
        
        Write-Host "`n🎯 Next steps:" -ForegroundColor Yellow
        Write-Host "   1. In Unity, set CubeAgent Behavior Type to 'Inference Only'" -ForegroundColor White
        Write-Host "   2. Drag the .onnx file to the Model field" -ForegroundColor White
        Write-Host "   3. Press Play to test your trained agent!" -ForegroundColor White
    } else {
        Write-Host "`n❌ Training failed with exit code: $($process.ExitCode)" -ForegroundColor Red
        Write-Host "Check the error messages above for details." -ForegroundColor Yellow
        
        # Common troubleshooting tips
        Write-Host "`n🔧 Troubleshooting:" -ForegroundColor Cyan
        Write-Host "   • Make sure Unity is running and in Play mode" -ForegroundColor White
        Write-Host "   • Check that the scene has CubeAgent behaviors" -ForegroundColor White
        Write-Host "   • Verify the config file: $ConfigPath" -ForegroundColor White
        Write-Host "   • Try: .\Scripts\check_environment.ps1" -ForegroundColor White
        
        exit $process.ExitCode
    }
} catch {
    Write-Host "`n❌ Failed to start training: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`n🔧 Troubleshooting:" -ForegroundColor Cyan
    Write-Host "   • Make sure environment is activated: .\Scripts\activate_mlagents_py310.ps1" -ForegroundColor White
    Write-Host "   • Check mlagents-learn is available: mlagents-learn --help" -ForegroundColor White
    exit 1
}