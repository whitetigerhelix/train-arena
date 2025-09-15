using UnityEngine;
using System.IO;

public class SimpleRecorder : MonoBehaviour
{
    public int fps = 30;
    public string outputDir = "Recordings";
    public bool recording = false;
    int frameIdx = 0;

    void Start()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", outputDir));
        Time.captureFramerate = fps;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) { recording = !recording; Debug.Log("Recording: " + recording); }
    }

    void OnPostRender()
    {
        if (!recording) return;
        var tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0,0,Screen.width,Screen.height), 0, 0);
        tex.Apply();
        var bytes = tex.EncodeToPNG();
        Destroy(tex);
        var path = Path.Combine(Application.dataPath, "..", outputDir, $"frame_{frameIdx:000000}.png");
        File.WriteAllBytes(path, bytes);
        frameIdx++;
    }
}