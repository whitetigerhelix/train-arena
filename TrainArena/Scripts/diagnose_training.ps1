# Unity ML-Agents Training Diagnostic Script
# Helps diagnose common training setup issues

Write-Host "🔍 Unity ML-Agents Training Diagnostics" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Check ML-Agents environment
Write-Host "`n1. Python Environment Check:" -ForegroundColor Yellow
if ($env:VIRTUAL_ENV) {
    Write-Host "✅ Virtual environment active: $env:VIRTUAL_ENV" -ForegroundColor Green
    Write-Host "   Python version: $(python --version)" -ForegroundColor White
} else {
    Write-Host "❌ No virtual environment detected" -ForegroundColor Red
}

# Check ML-Agents installation
Write-Host "`n2. ML-Agents Installation:" -ForegroundColor Yellow
try {
    $mlagentsVersion = pip show mlagents | Select-String "Version:" | ForEach-Object { $_.ToString().Split(':')[1].Trim() }
    Write-Host "✅ ML-Agents version: $mlagentsVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ ML-Agents not found" -ForegroundColor Red
}

# Check configuration file
Write-Host "`n3. Configuration File Check:" -ForegroundColor Yellow
$configPath = "Assets/ML-Agents/Configs/cube_ppo.yaml"
if (Test-Path $configPath) {
    Write-Host "✅ Config file exists: $configPath" -ForegroundColor Green
    
    # Quick config validation
    try {
        $configContent = Get-Content $configPath -Raw
        if ($configContent -match "CubeAgent") {
            Write-Host "✅ CubeAgent behavior found in config" -ForegroundColor Green
        } else {
            Write-Host "⚠️  CubeAgent behavior not found in config" -ForegroundColor Yellow
        }
        
        if ($configContent -match "normalize:\s*true") {
            Write-Host "✅ Input normalization enabled" -ForegroundColor Green
        } else {
            Write-Host "⚠️  Input normalization not enabled" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ Could not read config file" -ForegroundColor Red
    }
} else {
    Write-Host "❌ Config file not found: $configPath" -ForegroundColor Red
}

# Check Results directory
Write-Host "`n4. Results Directory:" -ForegroundColor Yellow
if (Test-Path "Results") {
    $resultDirs = Get-ChildItem "Results" -Directory | Sort-Object LastWriteTime -Descending
    Write-Host "✅ Results directory exists" -ForegroundColor Green
    Write-Host "   Recent training runs:" -ForegroundColor White
    $resultDirs | Select-Object -First 3 | ForEach-Object {
        Write-Host "   📁 $($_.Name) - $($_.LastWriteTime)" -ForegroundColor Gray
        
        # Check for TensorBoard event files
        $eventFiles = Get-ChildItem $_.FullName -Filter "events.out.tfevents.*" -Recurse
        if ($eventFiles) {
            Write-Host "      ✅ TensorBoard event files found" -ForegroundColor Green
        } else {
            Write-Host "      ❌ No TensorBoard event files (training didn't start properly)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "❌ No Results directory found" -ForegroundColor Red
}

# Network diagnostics
Write-Host "`n5. Network Port Check:" -ForegroundColor Yellow
try {
    $netstat = netstat -an | Select-String ":5004"
    if ($netstat) {
        Write-Host "⚠️  Port 5004 might be in use:" -ForegroundColor Yellow
        Write-Host "   $netstat" -ForegroundColor Gray
    } else {
        Write-Host "✅ Port 5004 appears available" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  Could not check port 5004" -ForegroundColor Yellow
}

Write-Host "`n🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan

Write-Host "`n📋 Unity Scene Setup:" -ForegroundColor Yellow
Write-Host "   1. In Unity: Tools → ML Hack → Build Cube Training Scene" -ForegroundColor White
Write-Host "   2. Save the scene (Ctrl+S)" -ForegroundColor White
Write-Host "   3. Verify objects have 'Behavior Parameters' component" -ForegroundColor White
Write-Host "   4. Check Behavior Name = 'CubeAgent', Type = 'Default'" -ForegroundColor White

Write-Host "`n🚀 Training Workflow:" -ForegroundColor Yellow
Write-Host "   1. Run: .\Scripts\train_cube.ps1" -ForegroundColor White
Write-Host "   2. Wait for: 'Listening on port 5004'" -ForegroundColor White
Write-Host "   3. THEN press Play in Unity (you have 30 seconds by default)" -ForegroundColor White
Write-Host "   4. Watch for: 'Connected to Unity environment'" -ForegroundColor White
Write-Host "   5. If timeout issues: .\Scripts\train_cube.ps1 -TimeoutWait 300" -ForegroundColor White

Write-Host "`n📊 TensorBoard:" -ForegroundColor Yellow
Write-Host "   • URL: http://localhost:6006" -ForegroundColor White
Write-Host "   • Only shows data after successful training connection" -ForegroundColor White
Write-Host "   • If no data: Unity connection failed" -ForegroundColor White

Write-Host "`n✨ Success Indicators:" -ForegroundColor Green
Write-Host "   ✅ 'Connected to Unity environment' message" -ForegroundColor White
Write-Host "   ✅ TensorBoard shows learning curves" -ForegroundColor White
Write-Host "   ✅ Episode rewards in console output" -ForegroundColor White