using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TrainArena.Dashboard
{
    [Serializable]
    public class TBScalarPoint
    {
        public double wallTime;
        public int step;
        public float value;
    }

    public static class TensorBoardClient
    {
        // Example endpoints (TensorBoard must be running; defaults to http://localhost:6006)
        //  GET /data/plugin/scalars/tags?run=<RUN>
        //  GET /data/plugin/scalars/scalars?tag=<TAG>&run=<RUN>&format=csv
        public static async Task<List<string>> FetchTags(string serverUrl, string run)
        {
            string url = $"{serverUrl.TrimEnd('/')}/data/plugin/scalars/tags?run={UnityWebRequest.EscapeURL(run)}";
            using (var req = UnityWebRequest.Get(url))
            {
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
#if UNITY_2020_1_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    Debug.LogWarning($"[TensorBoardClient] FetchTags failed: {req.error} ({url})");
                    return new List<string>();
                }
                // Response is JSON list, e.g., ["Environment/Cumulative Reward","Policy/Loss",...]
                try
                {
                    var json = req.downloadHandler.text;
                    // naive JSON list parse
                    var tags = new List<string>();
                    int i = 0;
                    while (i < json.Length)
                    {
                        int q = json.IndexOf('"', i);
                        if (q < 0) break;
                        int q2 = json.IndexOf('"', q + 1);
                        if (q2 < 0) break;
                        tags.Add(json.Substring(q + 1, q2 - q - 1));
                        i = q2 + 1;
                    }
                    return tags;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[TensorBoardClient] Parse tags failed: {e}");
                    return new List<string>();
                }
            }
        }

        public static async Task<List<TBScalarPoint>> FetchScalarsCSV(string serverUrl, string run, string tag)
        {
            string url = $"{serverUrl.TrimEnd('/')}/data/plugin/scalars/scalars?tag={UnityWebRequest.EscapeURL(tag)}&run={UnityWebRequest.EscapeURL(run)}&format=csv";
            using (var req = UnityWebRequest.Get(url))
            {
                var op = req.SendWebRequest();
                while (!op.isDone) await Task.Yield();
#if UNITY_2020_1_OR_NEWER
                if (req.result != UnityWebRequest.Result.Success)
#else
                if (req.isNetworkError || req.isHttpError)
#endif
                {
                    Debug.LogWarning($"[TensorBoardClient] FetchScalars failed: {req.error} ({url})");
                    return new List<TBScalarPoint>();
                }
                var text = req.downloadHandler.text;
                // CSV header usually: "Wall time, Step, Value"
                var lines = text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                var list = new List<TBScalarPoint>(lines.Length);
                foreach (var line in lines.Skip(1)) // skip header
                {
                    var parts = line.Split(',');
                    if (parts.Length < 3) continue;
                    double wall;
                    int step;
                    float val;
                    if (double.TryParse(parts[0].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out wall) &&
                        int.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out step) &&
                        float.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out val))
                    {
                        list.Add(new TBScalarPoint { wallTime = wall, step = step, value = val });
                    }
                }
                return list;
            }
        }
    }
}