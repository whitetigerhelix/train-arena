param([string]$RunId = "ragdoll_run_01")
mlagents-learn Assets/ML-Agents/Configs/ragdoll_ppo.yaml --run-id=$RunId --train