# ML-Agents Ragdoll Training Script
# Advanced ragdoll locomotion training with proper logging and TensorBoard

param(
    [string]$RunId = "ragdoll_run_$(Get-Date -Format 'yyyyMMdd_HHmmss')",
    [string]$ConfigPath = "Assets/ML-Agents/Configs/ragdoll_ppo.yaml",
    [switch]$Resume,
    [switch]$SkipTensorBoard,
    [int]$TimeoutWait = 300,  # Longer timeout for complex ragdoll initialization
    [int]$TimeScale = 50
)

Write-Host "🎭 Starting ML-Agents Ragdoll Training" -ForegroundColor Green
Write-Host "Run ID: $RunId" -ForegroundColor Cyan
Write-Host "Config: $ConfigPath" -ForegroundColor Cyan
Write-Host "Time Scale: ${TimeScale}x" -ForegroundColor Cyan
Write-Host "Unity Timeout: $TimeoutWait seconds" -ForegroundColor Cyan

# Check if we're in a virtual environment, activate if needed
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
    Write-Host "🔄 No virtual environment detected - auto-activating..." -ForegroundColor Yellow
    
    # Try to activate the ML-Agents environment automatically
    if (Test-Path ".\Scripts\activate_mlagents_py310.ps1") {
        Write-Host "   🐍 Activating Python ML-Agents environment..." -ForegroundColor Yellow
        
        try {
            # Source the activation script in this session
            & ".\Scripts\activate_mlagents_py310.ps1"
            
            # Check if environment is now active by checking for virtual environment
            if ($env:VIRTUAL_ENV -and (Test-Path "$env:VIRTUAL_ENV\Scripts\python.exe")) {
                Write-Host "   ✅ Environment activated successfully!" -ForegroundColor Green
            } elseif (Test-Path "venv\mlagents-py310\Scripts\python.exe") {
                # Sometimes VIRTUAL_ENV isn't set immediately, but the environment works
                Write-Host "   ✅ Environment available and ready!" -ForegroundColor Green
            } else {
                throw "Virtual environment not properly activated"
            }
        } catch {
            Write-Host "   ❌ Failed to activate environment: $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "   Please run manually:" -ForegroundColor White
            Write-Host "   .\Scripts\setup_python310.ps1" -ForegroundColor Cyan
            Write-Host "   .\Scripts\activate_mlagents_py310.ps1" -ForegroundColor Cyan
            exit 1
        }
    } else {
        Write-Host "   ❌ Activation script not found: .\Scripts\activate_mlagents_py310.ps1" -ForegroundColor Red
        Write-Host "   Setup environment first:" -ForegroundColor White
        Write-Host "   .\Scripts\setup_python310.ps1" -ForegroundColor Cyan
        exit 1
    }
}

# Set compatibility environment variables (prevents protobuf errors)
$env:PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION = "python"
$env:CUDA_VISIBLE_DEVICES = ""  # Force CPU-only operation
Write-Host "🔧 Protobuf compatibility and CPU-only mode enabled" -ForegroundColor Yellow

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
$TrainArgs = @("`"$ConfigPath`"", "--run-id=`"$RunId`"")

# Configure Unity connection timeout (important for ragdoll complexity)
$TrainArgs += "--timeout-wait=$TimeoutWait"

# Add time scale for faster training (CRITICAL for preventing timeouts)
$TrainArgs += "--time-scale=$TimeScale"

# Add additional parameters to prevent timeouts and improve stability
$TrainArgs += "--num-envs=1"  # Force single environment to reduce complexity
$TrainArgs += "--width=640"   # Reduce render resolution for better performance
$TrainArgs += "--height=480"  # Reduce render resolution for better performance

if ($Resume) {
    $TrainArgs += "--resume"
    Write-Host "⚡ Resuming training from checkpoint" -ForegroundColor Yellow
}

$TrainCmd = "mlagents-learn " + ($TrainArgs -join " ")

Write-Host "📝 Training Command:" -ForegroundColor Magenta
Write-Host $TrainCmd -ForegroundColor White

Write-Host "`n🎯 Instructions:" -ForegroundColor Yellow
Write-Host "1. Open Unity and load your Ragdoll Training Scene (if not already open)" -ForegroundColor White
Write-Host "2. When you see 'Listening on port 5004' below, switch to Unity" -ForegroundColor White
Write-Host "3. Press PLAY in Unity within $TimeoutWait seconds" -ForegroundColor White
Write-Host "4. Watch the ragdoll agents switch to Default behavior and start training!" -ForegroundColor White
Write-Host "5. Monitor progress at http://localhost:6006 (TensorBoard)" -ForegroundColor White
Write-Host "6. Press Ctrl+C here to stop training when satisfied" -ForegroundColor White

Write-Host "`n📊 Expected Training Progress:" -ForegroundColor Cyan
Write-Host "   • Phase 1 (0-100k steps): Learning basic balance and joint control" -ForegroundColor White
Write-Host "   • Phase 2 (100k-300k steps): Developing locomotion patterns" -ForegroundColor White
Write-Host "   • Phase 3 (300k+ steps): Refining walking and navigation" -ForegroundColor White
Write-Host "   • Training time: 1-3 hours (reduced complexity for stability)" -ForegroundColor White
Write-Host "`n⚠️  IMPORTANT: Keep Unity responsive during training!" -ForegroundColor Yellow
Write-Host "   • Don't minimize Unity window or switch away for long periods" -ForegroundColor White
Write-Host "   • If training stalls, the timeout is now much shorter for quicker recovery" -ForegroundColor White

Write-Host "`n⏳ Starting training with $TimeoutWait-second Unity timeout..." -ForegroundColor Yellow
Start-Sleep 5

# Execute the training command
Write-Host "`n🚀 Launching ML-Agents ragdoll training..." -ForegroundColor Green
try {
    Write-Host "🔗 Executing: $TrainCmd" -ForegroundColor Cyan
    Write-Host "📁 Working Directory: $(Get-Location)" -ForegroundColor Cyan
    Write-Host "📄 Config Path: $ConfigPath" -ForegroundColor Cyan
    Write-Host "🎮 Watch for 'Listening on port 5004' message, then press PLAY in Unity!" -ForegroundColor Green
    
    # Execute the command and capture the process info
    $processInfo = New-Object System.Diagnostics.ProcessStartInfo
    $processInfo.FileName = "mlagents-learn"
    $processInfo.Arguments = $TrainArgs -join " "
    $processInfo.UseShellExecute = $false
    $processInfo.CreateNoWindow = $false
    $processInfo.WorkingDirectory = (Get-Location).Path
    
    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $processInfo
    $process.Start()
    
    Write-Host "📡 ML-Agents ragdoll training process started (PID: $($process.Id))" -ForegroundColor Green
    Write-Host "🎮 Switch to Unity and press PLAY when you see 'Listening on port 5004'!" -ForegroundColor Yellow
    
    # Wait for process to complete
    $process.WaitForExit()
    
    $exitCode = $process.ExitCode
} catch {
    Write-Host "❌ Failed to start mlagents-learn: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Determine success by exit code
$wasInterrupted = $exitCode -eq 130 -or $exitCode -eq 2  # Common interruption exit codes

if ($exitCode -eq 0) {
    Write-Host "`n✅ Ragdoll training completed successfully!" -ForegroundColor Green
    Write-Host "Results saved to: Results/$RunId" -ForegroundColor Cyan
    Write-Host "TensorBoard logs available at: Results/$RunId" -ForegroundColor Cyan
    
    # Check if .onnx model was created
    $onnxFiles = Get-ChildItem "Results/$RunId" -Filter "*.onnx" -Recurse
    if ($onnxFiles) {
        Write-Host "`n🧠 Trained ragdoll models:" -ForegroundColor Yellow
        foreach ($file in $onnxFiles) {
            Write-Host "   📄 $($file.FullName)" -ForegroundColor White
        }
    }
    
    Write-Host "`n🎯 Next steps:" -ForegroundColor Yellow
    Write-Host "   1. In Unity, set RagdollAgent Behavior Type to 'Inference Only'" -ForegroundColor White
    Write-Host "   2. Drag the .onnx file to the Model field" -ForegroundColor White
    Write-Host "   3. Press Play to test your trained ragdoll!" -ForegroundColor White
    Write-Host "   4. Watch your ragdoll walk, balance, and navigate!" -ForegroundColor White
} elseif ($wasInterrupted) {
    Write-Host "`n❌ Training was interrupted!" -ForegroundColor Red
    Write-Host "This usually means Unity wasn't running or wasn't in Play mode." -ForegroundColor Yellow
    
    Write-Host "`n🔧 Common causes:" -ForegroundColor Cyan
    Write-Host "   • Unity Editor not running" -ForegroundColor White
    Write-Host "   • Unity scene not in Play mode when training started" -ForegroundColor White
    Write-Host "   • No RagdollAgent behaviors in the scene" -ForegroundColor White
    Write-Host "   • Unity crashed or was closed during training" -ForegroundColor White
    Write-Host "   • Ragdoll physics issues causing immediate failures" -ForegroundColor White
    
    Write-Host "`n🎯 Next steps:" -ForegroundColor Yellow
    Write-Host "   1. Open Unity and load your Ragdoll Training Scene" -ForegroundColor White
    Write-Host "   2. Make sure the scene has RagdollAgent objects" -ForegroundColor White
    Write-Host "   3. Test ragdoll physics in heuristic mode first" -ForegroundColor White
    Write-Host "   4. Run training script first, THEN press Play in Unity" -ForegroundColor White
    Write-Host "   5. Look for 'Listening on port 5004' message before pressing Play" -ForegroundColor White
    
    exit 1
} else {
    Write-Host "`n❌ Training failed with exit code: $exitCode" -ForegroundColor Red
    Write-Host "Check the error messages above for details." -ForegroundColor Yellow
    
    # Ragdoll-specific troubleshooting tips
    Write-Host "`n🔧 Ragdoll Training Troubleshooting:" -ForegroundColor Cyan
    Write-Host "   • Make sure Unity is running and in Play mode" -ForegroundColor White
    Write-Host "   • Check that the scene has RagdollAgent behaviors" -ForegroundColor White
    Write-Host "   • Verify ragdoll joints are properly configured" -ForegroundColor White
    Write-Host "   • Test heuristic mode first to ensure physics work" -ForegroundColor White
    Write-Host "   • Verify the config file: $ConfigPath" -ForegroundColor White
    Write-Host "   • Try: .\Scripts\check_environment.ps1" -ForegroundColor White
    
    exit $exitCode
}