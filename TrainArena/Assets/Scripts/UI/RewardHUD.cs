using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RewardHUD : MonoBehaviour
{
    [System.Serializable]
    public class RewardBar
    {
        public string name;
        public Slider slider;
        public float smoothing = 0.2f;
        [HideInInspector] public float target;
        [HideInInspector] public float value;
    }

    public List<RewardBar> bars = new List<RewardBar>();

    public void SetReward(string name, float v)
    {
        var b = bars.Find(x => x.name == name);
        if (b == null) return;
        b.target = Mathf.Clamp(v, -1f, 1f);
    }

    void Update()
    {
        foreach (var b in bars)
        {
            b.value = Mathf.Lerp(b.value, b.target, b.smoothing);
            if (b.slider) b.slider.value = (b.value + 1f) * 0.5f; // map [-1,1] to [0,1]
        }
    }
}