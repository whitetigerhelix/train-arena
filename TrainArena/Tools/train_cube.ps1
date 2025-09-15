param([string]$RunId = "cube_run_01")
mlagents-learn Assets/ML-Agents/Configs/cube_ppo.yaml --run-id=$RunId --train