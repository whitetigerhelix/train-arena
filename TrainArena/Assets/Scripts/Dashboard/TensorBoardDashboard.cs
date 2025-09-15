using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TrainArena.Dashboard
{
    public class TensorBoardDashboard : MonoBehaviour
    {
        [Header("Server")]
        public string serverUrl = "http://localhost:6006";
        public string run = "cube_run_01";

        [Header("Tags to plot")]
        public List<string> tags = new List<string> { "Environment/Cumulative Reward", "Policy/Loss", "Policy/Entropy" };

        [Header("UI Refs")]
        public TBLineChart chartPrefab;
        public RectTransform chartContainer;
        public Button refreshButton;
        public Toggle autoRefreshToggle;
        public float refreshInterval = 5f;

        float timer;
        List<TBLineChart> charts = new List<TBLineChart>();

        async void Start()
        {
            if (refreshButton) refreshButton.onClick.AddListener(async ()=> await Refresh());
            if (autoRefreshToggle) autoRefreshToggle.isOn = true;

            await BuildCharts();
            await Refresh();
        }

        void Update()
        {
            if (autoRefreshToggle && autoRefreshToggle.isOn)
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= refreshInterval)
                {
                    timer = 0f;
                    _ = Refresh();
                }
            }
        }

        async Task BuildCharts()
        {
            // Clear existing
            foreach (Transform child in chartContainer) Destroy(child.gameObject);
            charts.Clear();

            // If tags empty, fetch suggestions
            if (tags == null || tags.Count == 0)
            {
                var available = await TensorBoardClient.FetchTags(serverUrl, run);
                tags = available.Take(3).ToList();
            }

            foreach (var tag in tags)
            {
                var chart = Instantiate(chartPrefab, chartContainer);
                chart.name = $"Chart_{tag.Replace('/','_')}";
                // Add a label
                var go = new GameObject("Label", typeof(Text));
                go.transform.SetParent(chart.transform, false);
                var t = go.GetComponent<Text>();
                t.text = tag;
                t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                t.alignment = TextAnchor.UpperLeft;
                t.color = Color.white;
                var rt = t.rectTransform;
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(0,1);
                rt.anchoredPosition = new Vector2(8, -4);
                charts.Add(chart);
            }
        }

        public async Task Refresh()
        {
            if (charts.Count != tags.Count) await BuildCharts();
            for (int i=0;i<tags.Count;i++)
            {
                var data = await TensorBoardClient.FetchScalarsCSV(serverUrl, run, tags[i]);
                if (data == null || data.Count == 0) continue;
                var xs = data.Select(d => (float)d.step).ToList();
                var ys = data.Select(d => d.value).ToList();
                charts[i].Plot(xs, ys);
            }
        }
    }
}