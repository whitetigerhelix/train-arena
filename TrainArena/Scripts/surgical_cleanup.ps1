# Surgical Cleanup Script for TrainArena ML-Agents
# Provides selective cleanup options for easy project reset

param(
    [switch]$PythonEnv,      # Clean only Python environment
    [switch]$TrainingResults, # Clean only training results
    [switch]$UnityLogs,      # Clean only Unity logs
    [switch]$All,            # Clean everything
    [switch]$ListOnly,       # Show what would be cleaned (dry run)
    [switch]$Force           # Skip confirmation prompts
)

# Color functions for better output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Get project root (where this script is located)
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectRoot = Split-Path -Parent $ScriptDir

Write-Info "🔧 TrainArena Surgical Cleanup Tool"
Write-Info "Project Root: $ProjectRoot"
Write-Host ""

# Define cleanup targets
$CleanupTargets = @{
    "PythonEnv" = @{
        "Description" = "Python virtual environment"
        "Paths" = @(
            "$ProjectRoot\venv\mlagents-py310",
            "$ProjectRoot\venv\mlagents-py311"
        )
        "Size" = "~500MB"
    }
    "TrainingResults" = @{
        "Description" = "ML-Agents training results and models"
        "Paths" = @(
            "$ProjectRoot\results",
            "$ProjectRoot\*.onnx"
        )
        "Size" = "~50-200MB"
    }
    "UnityLogs" = @{
        "Description" = "Unity logs and temporary files"
        "Paths" = @(
            "$ProjectRoot\Logs",
            "$ProjectRoot\Library\ArtifactDB*",
            "$ProjectRoot\Library\Bee",
            "$ProjectRoot\Library\PackageCache",
            "$ProjectRoot\Temp"
        )
        "Size" = "~1-5GB"
    }
}

# Function to get actual size of paths
function Get-PathSize {
    param($Paths)
    $TotalSize = 0
    foreach ($Path in $Paths) {
        if (Test-Path $Path) {
            if (Test-Path $Path -PathType Container) {
                $Size = (Get-ChildItem -Path $Path -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum
                if ($Size) { $TotalSize += $Size }
            } else {
                $Size = (Get-Item $Path -ErrorAction SilentlyContinue).Length
                if ($Size) { $TotalSize += $Size }
            }
        }
    }
    return $TotalSize
}

# Function to format file size
function Format-FileSize {
    param($Bytes)
    if ($Bytes -eq 0) { return "0 B" }
    $Sizes = @("B", "KB", "MB", "GB", "TB")
    $Index = [math]::Floor([math]::Log($Bytes, 1024))
    $Size = [math]::Round($Bytes / [math]::Pow(1024, $Index), 2)
    return "$Size $($Sizes[$Index])"
}

# Function to clean paths
function Remove-Paths {
    param($Paths, $Description)
    
    $ActuallyRemoved = @()
    foreach ($Path in $Paths) {
        if (Test-Path $Path) {
            try {
                if ($ListOnly) {
                    Write-Info "  Would remove: $Path"
                } else {
                    Remove-Item -Path $Path -Recurse -Force -ErrorAction Stop
                    Write-Success "  ✅ Removed: $Path"
                    $ActuallyRemoved += $Path
                }
            } catch {
                Write-Error "  ❌ Failed to remove: $Path - $($_.Exception.Message)"
            }
        } else {
            Write-Info "  ⏭️  Not found: $Path"
        }
    }
    return $ActuallyRemoved
}

# Show current status
Write-Info "📊 Current Project Status:"
foreach ($Key in $CleanupTargets.Keys) {
    $Target = $CleanupTargets[$Key]
    $ActualSize = Get-PathSize -Paths $Target.Paths
    $ExistingPaths = $Target.Paths | Where-Object { Test-Path $_ }
    
    if ($ExistingPaths.Count -gt 0) {
        Write-Host "  $($Target.Description): " -NoNewline
        Write-Host "$(Format-FileSize $ActualSize)" -ForegroundColor Yellow
        foreach ($Path in $ExistingPaths) {
            Write-Host "    📁 $Path" -ForegroundColor DarkGray
        }
    } else {
        Write-Host "  $($Target.Description): " -NoNewline -ForegroundColor DarkGray
        Write-Host "Not present" -ForegroundColor DarkGreen
    }
}
Write-Host ""

# Determine what to clean
$ToClean = @()

if ($All) {
    $ToClean = $CleanupTargets.Keys
} else {
    if ($PythonEnv) { $ToClean += "PythonEnv" }
    if ($TrainingResults) { $ToClean += "TrainingResults" }
    if ($UnityLogs) { $ToClean += "UnityLogs" }
}

# If no specific options, show help
if ($ToClean.Count -eq 0 -and -not $ListOnly) {
    Write-Info "🎯 Surgical Cleanup Options:"
    Write-Host "  -PythonEnv        Clean Python virtual environment (~500MB)"
    Write-Host "  -TrainingResults  Clean ML-Agents results and models (~50-200MB)"
    Write-Host "  -UnityLogs       Clean Unity cache and logs (~1-5GB)"
    Write-Host "  -All             Clean everything"
    Write-Host "  -ListOnly        Show what would be cleaned (dry run)"
    Write-Host "  -Force           Skip confirmation prompts"
    Write-Host ""
    Write-Info "💡 Examples:"
    Write-Host "  .\Scripts\surgical_cleanup.ps1 -PythonEnv         # Clean only Python"
    Write-Host "  .\Scripts\surgical_cleanup.ps1 -All -ListOnly     # See what -All would do"
    Write-Host "  .\Scripts\surgical_cleanup.ps1 -UnityLogs -Force  # Quick Unity cleanup"
    Write-Host ""
    return
}

# Show what will be cleaned
if ($ToClean.Count -gt 0) {
    $Action = if ($ListOnly) { "Would clean" } else { "Will clean" }
    Write-Warning "⚠️  $Action the following:"
    
    $TotalEstimatedSize = 0
    foreach ($Key in $ToClean) {
        $Target = $CleanupTargets[$Key]
        $ActualSize = Get-PathSize -Paths $Target.Paths
        $TotalEstimatedSize += $ActualSize
        
        Write-Host "  📦 $($Target.Description)" -ForegroundColor Yellow
        Write-Host "     Size: $(Format-FileSize $ActualSize)" -ForegroundColor DarkGray
        foreach ($Path in $Target.Paths) {
            if (Test-Path $Path) {
                Write-Host "     📁 $Path" -ForegroundColor DarkGray
            }
        }
    }
    
    Write-Host ""
    Write-Info "🗂️  Total estimated recovery: $(Format-FileSize $TotalEstimatedSize)"
    
    # Confirm action
    if (-not $ListOnly -and -not $Force) {
        Write-Host ""
        $Confirmation = Read-Host "Continue with cleanup? (y/N)"
        if ($Confirmation -ne "y" -and $Confirmation -ne "Y") {
            Write-Info "Cleanup cancelled."
            return
        }
    }
}

# Perform cleanup
if (-not $ListOnly -and $ToClean.Count -gt 0) {
    Write-Host ""
    Write-Info "🚀 Starting cleanup..."
    
    $TotalFreed = 0
    foreach ($Key in $ToClean) {
        $Target = $CleanupTargets[$Key]
        $SizeBefore = Get-PathSize -Paths $Target.Paths
        
        Write-Info "Cleaning $($Target.Description)..."
        $RemovedPaths = Remove-Paths -Paths $Target.Paths -Description $Target.Description
        
        if ($RemovedPaths.Count -gt 0) {
            $TotalFreed += $SizeBefore
        }
    }
    
    Write-Host ""
    Write-Success "✅ Cleanup completed!"
    Write-Info "💾 Space freed: $(Format-FileSize $TotalFreed)"
    
    # Suggest next steps
    Write-Host ""
    Write-Info "🎯 Suggested next steps:"
    if ($ToClean -contains "PythonEnv") {
        Write-Host "  1. Run: .\Scripts\setup_python310.ps1"
        Write-Host "  2. Run: .\Scripts\check_environment.ps1"
    }
    if ($ToClean -contains "UnityLogs") {
        Write-Host "  • Open Unity to rebuild cache (first load will be slower)"
    }
}

Write-Host ""
Write-Info "🎉 Surgical cleanup complete!"