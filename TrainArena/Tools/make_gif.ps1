param(
  [string]$InputDir = "Recordings",
  [string]$OutMp4 = "out.mp4",
  [string]$OutGif = "out.gif",
  [int]$Fps = 30
)
# Requires ffmpeg on PATH
ffmpeg -y -framerate $Fps -pattern_type glob -i "$InputDir/frame_*.png" -c:v libx264 -pix_fmt yuv420p $OutMp4
ffmpeg -y -i $OutMp4 -vf "fps=$Fps,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" $OutGif
Write-Host "Wrote $OutMp4 and $OutGif"