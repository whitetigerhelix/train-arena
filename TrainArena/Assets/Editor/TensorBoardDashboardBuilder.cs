using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TrainArena.Dashboard;

public static class TensorBoardDashboardBuilder
{
    [MenuItem("TrainArena/Dashboard/Build TensorBoard Dashboard")]
    public static void Build()
    {
        var canvasGO = new GameObject("TB Dashboard", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.02f, 0.6f);
        prt.anchorMax = new Vector2(0.98f, 0.98f);
        prt.offsetMin = prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0,0,0,0.45f);

        // Controls
        var btnGO = new GameObject("RefreshBtn", typeof(Button), typeof(Image));
        btnGO.transform.SetParent(panel.transform, false);
        var brt = btnGO.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0.02f, 0.05f);
        brt.anchorMax = new Vector2(0.12f, 0.18f);
        brt.offsetMin = brt.offsetMax = Vector2.zero;
        btnGO.GetComponent<Image>().color = new Color(0.2f,0.6f,0.9f,0.9f);
        var btxtGO = new GameObject("Text", typeof(Text));
        btxtGO.transform.SetParent(btnGO.transform, false);
        var btxt = btxtGO.GetComponent<Text>();
        btxt.text = "Refresh";
        btxt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        btxt.alignment = TextAnchor.MiddleCenter;
        btxt.color = Color.white;
        btxt.rectTransform.anchorMin = new Vector2(0,0);
        btxt.rectTransform.anchorMax = new Vector2(1,1);
        btxt.rectTransform.offsetMin = btxt.rectTransform.offsetMax = Vector2.zero;

        var toggleGO = new GameObject("AutoRefresh", typeof(Toggle));
        toggleGO.transform.SetParent(panel.transform, false);
        var trt = toggleGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.14f, 0.05f);
        trt.anchorMax = new Vector2(0.28f, 0.18f);
        trt.offsetMin = trt.offsetMax = Vector2.zero;

        var tlabelGO = new GameObject("Label", typeof(Text));
        tlabelGO.transform.SetParent(toggleGO.transform, false);
        var tlabel = tlabelGO.GetComponent<Text>();
        tlabel.text = "Auto";
        tlabel.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        tlabel.alignment = TextAnchor.MiddleLeft;
        tlabel.rectTransform.anchorMin = new Vector2(0.3f,0);
        tlabel.rectTransform.anchorMax = new Vector2(1,1);
        tlabel.rectTransform.offsetMin = tlabel.rectTransform.offsetMax = Vector2.zero;

        // Charts container
        var charts = new GameObject("Charts", typeof(RectTransform));
        charts.transform.SetParent(panel.transform, false);
        var crt = charts.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0.02f, 0.22f);
        crt.anchorMax = new Vector2(0.98f, 0.95f);
        crt.offsetMin = crt.offsetMax = Vector2.zero;

        // Chart prefab object
        var prefabGO = new GameObject("ChartPrefab", typeof(RawImage), typeof(TrainArena.Dashboard.TBLineChart));
        prefabGO.SetActive(false);

        // Dashboard controller
        var dash = panel.AddComponent<TensorBoardDashboard>();
        dash.chartContainer = crt;
        dash.chartPrefab = prefabGO.GetComponent<TBLineChart>();
        dash.refreshButton = btnGO.GetComponent<Button>();
        dash.autoRefreshToggle = toggleGO.GetComponent<Toggle>();

        Debug.Log("TensorBoard Dashboard built. Start TensorBoard and set run/tag fields as needed.");
    }
}