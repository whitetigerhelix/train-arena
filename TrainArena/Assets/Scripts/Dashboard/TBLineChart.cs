using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TrainArena.Dashboard
{
    [RequireComponent(typeof(RawImage))]
    public class TBLineChart : MonoBehaviour
    {
        public int width = 480;
        public int height = 160;
        public int padding = 8;
        public Color background = new Color(0,0,0,0.6f);
        public Color axisColor = new Color(1,1,1,0.3f);
        public Color lineColor = new Color(0.2f, 0.8f, 1f, 1f);
        public float smooth = 0.0f; // 0..1 EMA smoothing for values

        Texture2D tex;
        Color[] clearBuf;
        float ema;

        void Awake()
        {
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            clearBuf = new Color[width * height];
            for (int i=0;i<clearBuf.Length;i++) clearBuf[i] = background;
            GetComponent<RawImage>().texture = tex;
            GetComponent<RawImage>().rectTransform.sizeDelta = new Vector2(width, height);
            Clear();
        }

        public void Clear()
        {
            tex.SetPixels(clearBuf);
            // axes
            DrawLine(padding, height - padding, width - padding, height - padding, axisColor);
            DrawLine(padding, padding, padding, height - padding, axisColor);
            tex.Apply();
        }

        public void Plot(List<float> xs, List<float> ys)
        {
            if (xs == null || ys == null || xs.Count == 0) return;
            Clear();

            float xmin = xs[0], xmax = xs[0], ymin = ys[0], ymax = ys[0];
            for (int i=0;i<xs.Count;i++){ xmin=Mathf.Min(xmin,xs[i]); xmax=Mathf.Max(xmax,xs[i]); ymin=Mathf.Min(ymin,ys[i]); ymax=Mathf.Max(ymax,ys[i]); }
            if (Mathf.Approximately(xmax, xmin)) xmax = xmin + 1f;
            if (Mathf.Approximately(ymax, ymin)) ymax = ymin + 1f;

            int n = xs.Count;
            Vector2 prev = Vector2.zero;
            for (int i=0;i<n;i++)
            {
                float x = Mathf.Lerp(padding, width - padding, (xs[i] - xmin) / (xmax - xmin));
                float yv = ys[i];
                if (smooth > 0f) { ema = Mathf.Lerp(ema, yv, 1f - Mathf.Pow(1f - smooth, 1)); yv = ema; }
                float y = Mathf.Lerp(padding, height - padding, (yv - ymin) / (ymax - ymin));
                Vector2 p = new Vector2(x, y);
                if (i>0) DrawLine((int)prev.x,(int)prev.y,(int)p.x,(int)p.y,lineColor);
                prev = p;
            }
            tex.Apply();
        }

        void DrawLine(int x0, int y0, int x1, int y1, Color c)
        {
            int dx = Mathf.Abs(x1 - x0), dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height) tex.SetPixel(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }
    }
}