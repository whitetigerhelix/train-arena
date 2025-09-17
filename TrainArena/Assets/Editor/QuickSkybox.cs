using UnityEditor;
using UnityEngine;

// Utility to quickly set a simple skybox for the scene
public static class QuickSkybox
{
    [MenuItem("Tools/ML Hack/Quick Skybox/Procedural Blue")]
    static void ApplyProcedural()
    {
        var mat = new Material(Shader.Find("Skybox/Procedural"));
        mat.SetColor("_SkyTint", new Color(0.38f,0.58f,1f));   // sky
        mat.SetColor("_GroundColor", new Color(0.26f,0.29f,0.34f));
        mat.SetFloat("_AtmosphereThickness", 0.95f);
        RenderSettings.skybox = mat;
        var sun = Object.FindObjectOfType<Light>();
        if (sun && sun.type == LightType.Directional) RenderSettings.sun = sun;
        DynamicGI.UpdateEnvironment();
    }
    
    [MenuItem("Tools/ML Hack/Quick Skybox/Simple Gradient")]
    static void ApplyGradient()
    {
        var mat = new Material(Shader.Find("Skybox/Gradient"));
        mat.SetColor("_Top", new Color(0.38f,0.58f,1f));   // sky
        mat.SetColor("_Bottom", new Color(0.26f,0.29f,0.34f));
        RenderSettings.skybox = mat;
        var sun = Object.FindObjectOfType<Light>();
        if (sun && sun.type == LightType.Directional) RenderSettings.sun = sun;
        DynamicGI.UpdateEnvironment();
    }
}
