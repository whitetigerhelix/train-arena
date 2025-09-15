#!/usr/bin/env bash
set -euo pipefail
mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=${1:-ragdoll_run_01} --train