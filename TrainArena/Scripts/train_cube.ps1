# ML-Agents Cube Training Script
# This script launches PPO training for the CubeAgent with proper logging and TensorBoard

param(
    [string]$RunId = "cube_run_$(Get-Date -Format 'yyyyMMdd_HHmmss')",
    [string]$ConfigPath = "Assets/ML-Agents/Configs/cube_ppo.yaml",
    [switch]$Resume,
    [switch]$SkipTensorBoard,
    [int]$TimeoutWait = 30
)

Write-Host "🚀 Starting ML-Agents Cube Training" -ForegroundColor Green
Write-Host "Run ID: $RunId" -ForegroundColor Cyan
Write-Host "Config: $ConfigPath" -ForegroundColor Cyan
Write-Host "Unity Timeout: $TimeoutWait seconds" -ForegroundColor Cyan

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

# Ensure we're in the Unity project root directory
$currentDir = Get-Location
Write-Host "📁 Current directory: $currentDir" -ForegroundColor Cyan

# Check for Unity project indicators
$unityProjectFiles = @("Assets", "ProjectSettings", "Packages")
$missingFiles = $unityProjectFiles | Where-Object { !(Test-Path $_) }

if ($missingFiles.Count -gt 0) {
    Write-Host "❌ Not in Unity project root directory!" -ForegroundColor Red
    Write-Host "Missing required directories: $($missingFiles -join ', ')" -ForegroundColor Yellow
    Write-Host "Please run this script from the Unity project root (TrainArena folder)" -ForegroundColor White
    Write-Host "Current location: $currentDir" -ForegroundColor White
    exit 1
}

# Ensure config file exists
if (!(Test-Path $ConfigPath)) {
    Write-Error "Config file not found: $ConfigPath"
    Write-Host "Current directory: $currentDir"
    
    if (Test-Path "Assets/ML-Agents/Configs") {
        Write-Host "Available configs:"
        Get-ChildItem "Assets/ML-Agents/Configs" -Filter "*.yaml" | Format-Table Name
    } else {
        Write-Host "Assets/ML-Agents/Configs directory not found!" -ForegroundColor Red
    }
    exit 1
}

Write-Host "✅ Unity project structure verified" -ForegroundColor Green
Write-Host "✅ Config file found: $ConfigPath" -ForegroundColor Green

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

# Build the training command (--train is deprecated, now default)
$TrainArgs = @("`"$ConfigPath`"", "--run-id=`"$RunId`"")

# Configure Unity connection timeout (default is 30 seconds)
$TrainArgs += "--timeout-wait=$TimeoutWait"

if ($Resume) {
    $TrainArgs += "--resume"
    Write-Host "⚡ Resuming training from checkpoint" -ForegroundColor Yellow
}

$TrainCmd = "mlagents-learn " + ($TrainArgs -join " ")

Write-Host "📝 Training Command:" -ForegroundColor Magenta
Write-Host $TrainCmd -ForegroundColor White

Write-Host "`n🎯 Instructions:" -ForegroundColor Yellow
Write-Host "1. Open Unity and load your Cube Training Scene (if not already open)" -ForegroundColor White
Write-Host "2. When you see 'Listening on port 5004' below, switch to Unity" -ForegroundColor White
Write-Host "3. Press PLAY in Unity within $TimeoutWait seconds" -ForegroundColor White
Write-Host "4. Watch the agents switch to Default behavior and start training!" -ForegroundColor White
Write-Host "5. Monitor progress at http://localhost:6006 (TensorBoard)" -ForegroundColor White
Write-Host "6. Press Ctrl+C here to stop training when satisfied" -ForegroundColor White

Write-Host "`n⏳ Starting training with $TimeoutWait-second Unity timeout..." -ForegroundColor Yellow
Start-Sleep 5

# Execute the training command and capture both exit code and output
Write-Host "`n🚀 Launching ML-Agents training..." -ForegroundColor Green
try {
    # Execute mlagents-learn directly to see real-time output (including "Listening on port 5004")
    Write-Host "🔗 Executing: $TrainCmd" -ForegroundColor Cyan
    Write-Host "� Working Directory: $(Get-Location)" -ForegroundColor Cyan
    Write-Host "📄 Config Path: $ConfigPath" -ForegroundColor Cyan
    Write-Host "�👀 Watch for 'Listening on port 5004' message, then press PLAY in Unity!" -ForegroundColor Green
    
    # Use Invoke-Expression to see real-time output
    $output = ""
    $errorOutput = ""
    $process = $null
    
    try {
        # Execute the command and capture the process info
        $processInfo = New-Object System.Diagnostics.ProcessStartInfo
        $processInfo.FileName = "mlagents-learn"
        $processInfo.Arguments = $TrainArgs -join " "
        $processInfo.UseShellExecute = $false
        $processInfo.CreateNoWindow = $false
        $processInfo.WorkingDirectory = (Get-Location).Path  # Set working directory to current location
        
        $process = New-Object System.Diagnostics.Process
        $process.StartInfo = $processInfo
        $process.Start()
        
        Write-Host "📡 ML-Agents process started (PID: $($process.Id))" -ForegroundColor Green
        Write-Host "🎮 Switch to Unity and press PLAY when you see 'Listening on port 5004'!" -ForegroundColor Yellow
        
        # Wait for process to complete
        $process.WaitForExit()
        
        $exitCode = $process.ExitCode
    } catch {
        Write-Host "❌ Failed to start mlagents-learn: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
    
    # Since we're not capturing output, determine success by exit code
    $wasInterrupted = $exitCode -eq 130 -or $exitCode -eq 2  # Common interruption exit codes
    
    if ($exitCode -eq 0) {
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
    } elseif ($wasInterrupted) {
        Write-Host "`n❌ Training was interrupted!" -ForegroundColor Red
        Write-Host "This usually means Unity wasn't running or wasn't in Play mode." -ForegroundColor Yellow
        
        Write-Host "`n🔧 Common causes:" -ForegroundColor Cyan
        Write-Host "   • Unity Editor not running" -ForegroundColor White
        Write-Host "   • Unity scene not in Play mode when training started" -ForegroundColor White
        Write-Host "   • No CubeAgent behaviors in the scene" -ForegroundColor White
        Write-Host "   • Unity crashed or was closed during training" -ForegroundColor White
        
        Write-Host "`n🎯 Next steps:" -ForegroundColor Yellow
        Write-Host "   1. Open Unity and load your Cube Training Scene" -ForegroundColor White
        Write-Host "   2. Make sure the scene has CubeAgent objects" -ForegroundColor White
        Write-Host "   3. Run training script first, THEN press Play in Unity" -ForegroundColor White
        Write-Host "   4. Look for 'Listening on port 5004' message before pressing Play" -ForegroundColor White
        
        exit 1
    } elseif ($hasErrors) {
        Write-Host "`n❌ Training process ended unexpectedly" -ForegroundColor Red
        Write-Host "Check the console output above for error details." -ForegroundColor Yellow
        exit 1
    } else {
        Write-Host "`n❌ Training failed with exit code: $exitCode" -ForegroundColor Red
        Write-Host "Check the error messages above for details." -ForegroundColor Yellow
        
        # Common troubleshooting tips
        Write-Host "`n🔧 Troubleshooting:" -ForegroundColor Cyan
        Write-Host "   • Make sure Unity is running and in Play mode" -ForegroundColor White
        Write-Host "   • Check that the scene has CubeAgent behaviors" -ForegroundColor White
        Write-Host "   • Verify the config file: $ConfigPath" -ForegroundColor White
        Write-Host "   • Try: .\Scripts\check_environment.ps1" -ForegroundColor White
        
        exit $exitCode
    }
} catch {
    Write-Host "`n❌ Failed to start training: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "`n🔧 Troubleshooting:" -ForegroundColor Cyan
    Write-Host "   • Make sure environment is activated: .\Scripts\activate_mlagents_py310.ps1" -ForegroundColor White
    Write-Host "   • Check mlagents-learn is available: mlagents-learn --help" -ForegroundColor White
    exit 1
}