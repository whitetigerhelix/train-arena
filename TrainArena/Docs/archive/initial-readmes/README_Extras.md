# TrainArena Extras: TensorBoard Dashboard & Model Hot-Reload

## TensorBoard Dashboard (in-Editor or Play Mode overlay)
- Menu: **Tools → ML Hack → Build TensorBoard Dashboard**
- Requires a TensorBoard server, e.g.:
  ```bash
  tensorboard --logdir results --port 6006
  ```
- In the generated panel, set `Run` to your run-id (e.g., `cube_run_01`), edit `Tags` if needed (common ML-Agents tags include `Environment/Cumulative Reward`, `Policy/Loss`, `Policy/Entropy`), then press **Refresh** or enable **Auto**.

Under the hood, the dashboard calls TensorBoard’s HTTP endpoints, e.g.:
- `/data/plugin/scalars/tags?run=<RUN>`
- `/data/plugin/scalars/scalars?tag=<TAG>&run=<RUN>&format=csv`

> Tip: if you log a *lot* of points, run TensorBoard with `--samples_per_plugin scalars=9999999` to avoid subsampling in some setups.

## Model Hot-Reload (Editor)
- Menu: **Tools → ML Hack → Model Hot-Reload**
- Click **Import Newest .onnx**: copies the most recent exported policy from `results/**.onnx` into your project (default `Assets/Models/TrainArena/latest.onnx`) and imports it as an `NNModel`.
- Click **Assign To All ModelSwitchers**: sets that model across all `ModelSwitcher` components in the open scenes and calls `Apply()`.

Now you can iterate like this:
1. Train PPO with `mlagents-learn` (exports `.onnx` under `results/<run-id>/`).
2. Open **Model Hot-Reload**, click **Import Newest .onnx**.
3. Click **Assign To All ModelSwitchers** → press Play → demo updates instantly.