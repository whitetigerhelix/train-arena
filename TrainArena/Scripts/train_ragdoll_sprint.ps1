# Ragdoll Training Script - Day 1 Sprint
# Activates Python environment and starts ragdoll training

param(
    [string]$RunId = "ragdoll_sprint_day1",
    [switch]$Resume = $false,
    [int]$TimeScale = 20
)

Write-Host "🎭 Starting Ragdoll Training (Day 1 Sprint)" -ForegroundColor Green
Write-Host "Run ID: $RunId" -ForegroundColor Cyan
Write-Host "Time Scale: ${TimeScale}x" -ForegroundColor Cyan

# Activate ML-Agents environment
Write-Host "`n🐍 Activating Python ML-Agents environment..." -ForegroundColor Yellow
& ".\activate_mlagents_py310.ps1"

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to activate Python environment" -ForegroundColor Red
    exit 1
}

# Check if resuming existing training
$resumeFlag = ""
if ($Resume) {
    $resumeFlag = "--resume"
    Write-Host "📂 Resuming existing training..." -ForegroundColor Yellow
}

# Start training
Write-Host "`n🚀 Launching ragdoll training..." -ForegroundColor Green
Write-Host "Config: Assets/ML-Agents/Configs/ragdoll_ppo.yaml" -ForegroundColor Gray
Write-Host "Expected training time: 2-4 hours for initial results" -ForegroundColor Gray
Write-Host "`nPress Ctrl+C to stop training and save checkpoint.`n" -ForegroundColor Yellow

$command = "mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=$RunId --train $resumeFlag --time-scale=$TimeScale"
Write-Host "Executing: $command" -ForegroundColor Gray

# Execute training
Invoke-Expression $command

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n✅ Ragdoll training completed successfully!" -ForegroundColor Green
    Write-Host "📁 Results saved in: results/$RunId/" -ForegroundColor Cyan
    Write-Host "🧠 Model files (.onnx) should be in: Assets/ML-Agents/Models/" -ForegroundColor Cyan
} else {
    Write-Host "`n❌ Training failed or was interrupted" -ForegroundColor Red
    Write-Host "Check results/$RunId/ for logs and any partial progress" -ForegroundColor Yellow
}