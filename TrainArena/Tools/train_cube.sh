#!/usr/bin/env bash
set -euo pipefail
mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=${1:-cube_run_01} --train