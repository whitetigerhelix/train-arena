using UnityEngine;
using UnityEngine.UI;

public class DomainRandomizationUI : MonoBehaviour
{
    public DomainRandomizer randomizer;

    void Start()
    {
        if (!randomizer) randomizer = FindFirstObjectByType<DomainRandomizer>(); //TODO: FindAnyObjectByType?  Have a smarter way to select lights
        BuildUI();
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("DomainUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.7f, 0.02f);
        rt.anchorMax = new Vector2(0.98f, 0.28f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0,0,0,0.35f);

        AddToggle(panel.transform, "Mass", new Vector2(0.05f, 0.65f), v => randomizer.randomizeMass = v, randomizer.randomizeMass);
        AddToggle(panel.transform, "Friction", new Vector2(0.05f, 0.45f), v => randomizer.randomizeFriction = v, randomizer.randomizeFriction);
        AddToggle(panel.transform, "Lighting", new Vector2(0.05f, 0.25f), v => randomizer.randomizeLighting = v, randomizer.randomizeLighting);
        AddToggle(panel.transform, "Gravity", new Vector2(0.05f, 0.05f), v => randomizer.randomizeGravity = v, randomizer.randomizeGravity);

        // Apply button
        var btnGO = new GameObject("ApplyBtn", typeof(Button), typeof(Image));
        btnGO.transform.SetParent(panel.transform, false);
        var brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.55f, 0.05f);
        brt.anchorMax = new Vector2(0.95f, 0.35f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        btnGO.GetComponent<Image>().color = new Color(0.2f,0.7f,0.3f,0.8f);
        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() => randomizer.ApplyOnce());

        var txtGO = new GameObject("ApplyText", typeof(Text));
        txtGO.transform.SetParent(btnGO.transform, false);
        var txt = txtGO.GetComponent<Text>();
        txt.text = "Apply Randomization";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.color = Color.white;
        txt.rectTransform.anchorMin = new Vector2(0,0);
        txt.rectTransform.anchorMax = new Vector2(1,1);
        txt.rectTransform.offsetMin = txt.rectTransform.offsetMax = Vector2.zero;
    }

    void AddToggle(Transform parent, string label, Vector2 anchors, System.Action<bool> onChange, bool start)
    {
        var tgo = new GameObject(label+"Toggle", typeof(Toggle));
        tgo.transform.SetParent(parent, false);
        var t = tgo.GetComponent<Toggle>();
        t.isOn = start;
        t.onValueChanged.AddListener(v => onChange(v));
        var rt = tgo.GetComponent<RectTransform>();
        rt.anchorMin = anchors;
        rt.anchorMax = anchors + new Vector2(0.4f, 0.2f);
        rt.offsetMin = rt.offsetMax = Vector2.zero;

        var textGO = new GameObject("Label", typeof(Text));
        textGO.transform.SetParent(tgo.transform, false);
        var txt = textGO.GetComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.alignment = TextAnchor.MiddleLeft;
        txt.rectTransform.anchorMin = new Vector2(0.2f, 0);
        txt.rectTransform.anchorMax = new Vector2(1,1);
        txt.rectTransform.offsetMin = txt.rectTransform.offsetMax = Vector2.zero;
    }
}