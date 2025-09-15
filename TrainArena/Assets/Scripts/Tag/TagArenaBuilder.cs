using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents.Policies;

public class TagArenaBuilder : MonoBehaviour
{
    public int arenasX = 3;
    public int arenasZ = 3;
    public float spacing = 16f;
    public GameObject runnerPrefab;
    public GameObject taggerPrefab;
    public LayerMask wallMask;
    public CurriculumController curriculum;

    void Start()
    {
        if (!runnerPrefab || !taggerPrefab) { Debug.LogError("Assign prefabs"); return; }
        BuildUI();
        BuildArenas();
    }

    void BuildArenas()
    {
        for (int x = 0; x < arenasX; x++)
            for (int z = 0; z < arenasZ; z++)
                SpawnArena(new Vector3(x * spacing, 0f, z * spacing));
    }

    void SpawnArena(Vector3 center)
    {
        // Ground + boundary (cylinder walls)
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.position = center;
        ground.transform.localScale = Vector3.one * 1.5f;
        ground.name = "Ground";

        float radius = curriculum ? curriculum.CurrentArenaSize : 6f;

        // Runner
        var runner = Instantiate(runnerPrefab, center + Vector3.up * 0.5f, Quaternion.identity, transform);
        var ra = runner.GetComponent<RunnerAgent>();
        ra.arenaRadius = radius;

        // Tagger
        var tagger = Instantiate(taggerPrefab, center + new Vector3(2,0.5f,2), Quaternion.identity, transform);
        tagger.GetComponent<HeuristicTagger>().speed = curriculum ? curriculum.CurrentTaggerSpeed : 2.5f;

        ra.tagger = tagger.transform;
    }

    void BuildUI()
    {
        var canvasGO = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);

        // Difficulty slider
        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.02f, 0.02f);
        prt.anchorMax = new Vector2(0.35f, 0.2f);
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0,0,0,0.4f);

        // Difficulty label
        var labelGO = new GameObject("Label", typeof(Text));
        labelGO.transform.SetParent(panel.transform, false);
        var label = labelGO.GetComponent<Text>();
        label.text = "Difficulty";
        label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        label.alignment = TextAnchor.UpperLeft;
        label.rectTransform.anchorMin = new Vector2(0.05f, 0.6f);
        label.rectTransform.anchorMax = new Vector2(0.5f, 0.95f);
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;

        // Slider
        var sliderGO = new GameObject("DifficultySlider", typeof(Slider));
        sliderGO.transform.SetParent(panel.transform, false);
        var slider = sliderGO.GetComponent<Slider>();
        slider.minValue = 0; slider.maxValue = 5; slider.wholeNumbers = true;
        slider.value = 0;
        slider.GetComponent<RectTransform>().anchorMin = new Vector2(0.05f, 0.2f);
        slider.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.5f);
        slider.GetComponent<RectTransform>().offsetMin = slider.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // Mode dropdown (Random/Heuristic/Inference) for the first Runner found
        var ddGO = new GameObject("ModeDropdown", typeof(Dropdown));
        ddGO.transform.SetParent(panel.transform, false);
        var dd = ddGO.GetComponent<Dropdown>();
        dd.options.Clear();
        dd.options.Add(new Dropdown.OptionData("Random"));
        dd.options.Add(new Dropdown.OptionData("Heuristic"));
        dd.options.Add(new Dropdown.OptionData("Inference"));
        dd.value = 2;
        dd.GetComponent<RectTransform>().anchorMin = new Vector2(0.55f, 0.6f);
        dd.GetComponent<RectTransform>().anchorMax = new Vector2(0.95f, 0.95f);
        dd.GetComponent<RectTransform>().offsetMin = dd.GetComponent<RectTransform>().offsetMax = Vector2.zero;

        // Wire curriculum
        if (curriculum == null) curriculum = gameObject.AddComponent<CurriculumController>();
        slider.onValueChanged.AddListener(v => curriculum.Apply((int)v));

        // Wire dropdown after runners spawn (simple runtime binding)
        dd.onValueChanged.AddListener(i => {
            var runner = FindAnyObjectByType<RunnerAgent>(); // Assume only one
            if (runner)
            {
                var ms = runner.GetComponent<ModelSwitcher>();
                if (ms) ms.SetMode(i);
            }
        });
    }
}