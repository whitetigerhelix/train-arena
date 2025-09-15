#!/usr/bin/env bash
# Requires ffmpeg installed and on PATH
set -euo pipefail
IN_DIR="${1:-Recordings}"
OUT_MP4="${2:-out.mp4}"
OUT_GIF="${3:-out.gif}"
FPS="${4:-30}"

ffmpeg -y -framerate "$FPS" -pattern_type glob -i "$IN_DIR/frame_*.png" -c:v libx264 -pix_fmt yuv420p "$OUT_MP4"
ffmpeg -y -i "$OUT_MP4" -vf "fps=$FPS,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" "$OUT_GIF"
echo "Wrote $OUT_MP4 and $OUT_GIF"