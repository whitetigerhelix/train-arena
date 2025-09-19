using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Unity.Barracuda;
using Unity.MLAgents.Policies;

public class ModelHotReloadWindow : EditorWindow
{
    string resultsDir = "results";
    string destAssetPath = "Assets/Models/TrainArena/latest.onnx";

    [MenuItem("TrainArena/Models/Model Hot-Reload")]
    public static void Open() => GetWindow<ModelHotReloadWindow>("Model Hot-Reload");

    void OnGUI()
    {
        GUILayout.Label("Import newest .onnx from results and assign to ModelSwitcher components.", EditorStyles.wordWrappedLabel);
        resultsDir = EditorGUILayout.TextField("Results dir", resultsDir);
        destAssetPath = EditorGUILayout.TextField("Dest asset path", destAssetPath);

        GUILayout.Space(8);
        if (GUILayout.Button("Import Newest .onnx"))
        {
            ImportNewest();
        }
        if (GUILayout.Button("Assign To All ModelSwitchers"))
        {
            AssignToAll();
        }
    }

    void ImportNewest()
    {
        if (!Directory.Exists(resultsDir))
        {
            EditorUtility.DisplayDialog("Model Hot-Reload", $"Results directory not found:\n{resultsDir}", "OK");
            return;
        }
        var onnx = Directory.EnumerateFiles(resultsDir, "*.onnx", SearchOption.AllDirectories)
            .Select(p => new FileInfo(p))
            .OrderByDescending(f => f.LastWriteTimeUtc)
            .FirstOrDefault();
        if (onnx == null)
        {
            EditorUtility.DisplayDialog("Model Hot-Reload", "No .onnx found in results.", "OK");
            return;
        }
        var destDir = Path.GetDirectoryName(destAssetPath);
        if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);
        File.Copy(onnx.FullName, destAssetPath, true);
        AssetDatabase.ImportAsset(destAssetPath);
        EditorUtility.DisplayDialog("Model Hot-Reload", $"Imported:\n{onnx.FullName}\n→ {destAssetPath}", "Nice");
    }

    void AssignToAll()
    {
        var model = AssetDatabase.LoadAssetAtPath<NNModel>(destAssetPath);
        if (!model)
        {
            EditorUtility.DisplayDialog("Model Hot-Reload", $"No NNModel at:\n{destAssetPath}", "OK");
            return;
        }
        var switchers = FindObjectsByType<ModelSwitcher>(FindObjectsSortMode.None);
        foreach (var s in switchers)
        {
            s.trainedModel = model;
            s.Apply();
            EditorUtility.SetDirty(s);
        }
        EditorUtility.DisplayDialog("Model Hot-Reload", $"Assigned model to {switchers.Length} ModelSwitcher(s).", "Done");
    }
}