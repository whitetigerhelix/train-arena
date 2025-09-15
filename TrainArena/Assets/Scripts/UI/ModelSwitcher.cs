using UnityEngine;
using Unity.MLAgents.Policies;

public class ModelSwitcher : MonoBehaviour
{
    public enum Mode { Random, Heuristic, Inference }
    public Mode mode = Mode.Inference;
    public NNModel trainedModel;
    public BehaviorParameters behavior;

    void Reset()
    {
        behavior = GetComponent<BehaviorParameters>();
    }

    public void SetMode(int m)
    {
        mode = (Mode)m;
        Apply();
    }

    public void Apply()
    {
        if (behavior == null) return;
        switch (mode)
        {
            case Mode.Random:
                behavior.BehaviorType = BehaviorType.Default; // no model, random actions until trainer attaches
                behavior.Model = null;
                break;
            case Mode.Heuristic:
                behavior.BehaviorType = BehaviorType.HeuristicOnly;
                behavior.Model = null;
                break;
            case Mode.Inference:
                behavior.BehaviorType = BehaviorType.InferenceOnly;
                behavior.Model = trainedModel;
                break;
        }
    }
}