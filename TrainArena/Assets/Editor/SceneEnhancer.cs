using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Automated scene enhancement system for TrainArena
/// Applies post-processing, lighting, skybox, and camera settings automatically during scene generation
/// </summary>
public static class SceneEnhancer
{
    /// <summary>
    /// Configurable visual settings for scene enhancement
    /// </summary>
    [System.Serializable]
    public class VisualSettings
    {
        [Header("Skybox Settings")]
        public Color skyTint = new Color(0.3f, 0.7f, 1f, 1f);
        public Color groundColor = new Color(0.15f, 0.18f, 0.25f, 1f);
        public float atmosphereThickness = 1.2f;
        public float exposure = 1.3f;
        public float sunSize = 0.04f;
        public float sunSizeConvergence = 5f;
        
        [Header("Lighting Settings")]
        public Color ambientSkyColor = new Color(0.4f, 0.75f, 1.2f, 1f);
        public Color ambientEquatorColor = new Color(0.3f, 0.35f, 0.5f, 1f);
        public Color ambientGroundColor = new Color(0.1f, 0.12f, 0.2f, 1f);
        public Color sunColor = new Color(1f, 0.98f, 0.95f, 1f);
        public float sunIntensity = 2.2f;
        
        [Header("Fog Settings")]
        public Color fogColor = new Color(0.6f, 0.75f, 0.95f, 1f);
        public float fogDensityCube = 0.002f;
        public float fogDensityRagdoll = 0.003f;
        
        [Header("Post-Processing - Bloom")]
        public float bloomThreshold = 1.0f;
        public float bloomIntensity = 0.6f;
        public float bloomScatter = 0.8f;
        public Color bloomTint = new Color(0.95f, 0.98f, 1f, 1f);
        
        [Header("Post-Processing - Vignette")]
        public float vignetteIntensity = 0.12f;
        public float vignetteSmoothness = 0.4f;
        public Color vignetteColor = new Color(0.1f, 0.15f, 0.25f, 1f);
        
        [Header("Post-Processing - Depth of Field")]
        public float dofFocusDistance = 15f;
        public float dofAperture = 8f;
        
        [Header("Post-Processing - Color Grading")]
        public float colorPostExposure = 0.1f;
        public float colorContrast = 5f;
        public float colorSaturation = -5f;
        public Color colorFilter = new Color(0.98f, 0.99f, 1.05f, 1f);
        
        [Header("Post-Processing - White Balance")]
        public float whiteBalanceTemperature = -10f;
        public float whiteBalanceTint = 2f;
        
        [Header("Camera Settings")]
        public float fieldOfView = 60f;
        public float nearClipPlane = 0.3f;
        public float farClipPlane = 1000f;
        public float moveSpeed = 10f;
        public float fastMoveSpeed = 20f;
        public float mouseSensitivity = 0.5f;
        
        /// <summary>
        /// Preset for polished, cool aesthetic
        /// </summary>
        public static VisualSettings PolishedCool()
        {
            return new VisualSettings(); // Uses default values defined above
        }
        
        /// <summary>
        /// Preset for warm, cinematic aesthetic
        /// </summary>
        public static VisualSettings WarmCinematic()
        {
            var settings = new VisualSettings();
            settings.skyTint = new Color(1f, 0.8f, 0.6f, 1f);
            settings.groundColor = new Color(0.25f, 0.2f, 0.15f, 1f);
            settings.ambientSkyColor = new Color(1f, 0.9f, 0.7f, 1f);
            settings.fogColor = new Color(0.9f, 0.8f, 0.7f, 1f);
            settings.bloomTint = new Color(1f, 0.95f, 0.9f, 1f);
            settings.colorFilter = new Color(1.05f, 1f, 0.95f, 1f);
            settings.whiteBalanceTemperature = 10f;
            return settings;
        }
        
        /// <summary>
        /// Preset for minimal, clean aesthetic
        /// </summary>
        public static VisualSettings MinimalClean()
        {
            var settings = new VisualSettings();
            settings.bloomIntensity = 0.3f;
            settings.vignetteIntensity = 0.05f;
            settings.colorSaturation = -15f;
            settings.fogDensityCube = 0.001f;
            settings.fogDensityRagdoll = 0.001f;
            return settings;
        }
    }
    
    // Current visual settings (can be modified at runtime)
    public static VisualSettings CurrentSettings = VisualSettings.PolishedCool();

    /// <summary>
    /// Apply complete scene enhancement package with default settings
    /// </summary>
    public static void EnhanceScene(Camera mainCamera, bool isRagdollScene = false)
    {
        EnhanceScene(mainCamera, isRagdollScene, CurrentSettings);
    }
    
    /// <summary>
    /// Apply complete scene enhancement package with custom settings
    /// </summary>
    public static void EnhanceScene(Camera mainCamera, bool isRagdollScene, VisualSettings settings)
    {
        TrainArenaDebugManager.Log($"🎨 Applying scene enhancements with {settings.GetType().Name}...", TrainArenaDebugManager.DebugLogLevel.Important);
        
        // Apply post-processing
        ApplyPostProcessing(settings);
        
        // Configure camera settings
        EnhanceCamera(mainCamera);
        
        // Apply skybox and environment
        ApplyEnvironmentSettings(settings);
        
        // Configure lighting and fog
        ConfigureLighting(isRagdollScene, settings);
        
        TrainArenaDebugManager.Log("✅ Scene enhancement complete!", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    /// <summary>
    /// Apply post-processing effects for enhanced visuals (fully procedural)
    /// </summary>
    public static void ApplyPostProcessing()
    {
        ApplyPostProcessing(CurrentSettings);
    }
    
    /// <summary>
    /// Apply post-processing effects with custom configuration
    /// </summary>
    public static void ApplyPostProcessing(VisualSettings settings)
    {
        // Always create procedural post-processing - no hardcoded prefab dependencies
        CreatePostProcessingVolume(settings);
        TrainArenaDebugManager.Log("📸 Created configurable post-processing volume programmatically", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Create post-processing volume with configurable settings
    /// </summary>
    static void CreatePostProcessingVolume(VisualSettings settings)
    {
        var postProcessGO = new GameObject("PostProcess Volume");
        var volume = postProcessGO.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 0;
        
        // Always create new profile procedurally (no hardcoded asset dependencies)
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            
        // Add configurable Bloom
        var bloom = profile.Add<Bloom>(true);
        bloom.threshold.value = settings.bloomThreshold;
        bloom.intensity.value = settings.bloomIntensity;
        bloom.scatter.value = settings.bloomScatter;
        bloom.tint.value = settings.bloomTint;
        bloom.active = true;
        
        // Add configurable Vignette
        var vignette = profile.Add<Vignette>(true);
        vignette.intensity.value = settings.vignetteIntensity;
        vignette.smoothness.value = settings.vignetteSmoothness;
        vignette.color.value = settings.vignetteColor;
        vignette.active = true;
        
        // Add configurable Depth of Field
        var dof = profile.Add<DepthOfField>(true);
        dof.mode.value = DepthOfFieldMode.Bokeh;
        dof.focusDistance.value = settings.dofFocusDistance;
        dof.aperture.value = settings.dofAperture;
        dof.active = true;
        
        // Add configurable Color Grading
        var colorGrading = profile.Add<ColorAdjustments>(true);
        colorGrading.postExposure.value = settings.colorPostExposure;
        colorGrading.contrast.value = settings.colorContrast;
        colorGrading.saturation.value = settings.colorSaturation;
        colorGrading.colorFilter.value = settings.colorFilter;
        colorGrading.active = true;
        
        // Add configurable White Balance
        var whiteBalance = profile.Add<WhiteBalance>(true);
        whiteBalance.temperature.value = settings.whiteBalanceTemperature;
        whiteBalance.tint.value = settings.whiteBalanceTint;
        whiteBalance.active = true;
        
        TrainArenaDebugManager.Log("Created configurable post-process profile (Bloom + Vignette + DOF + ColorGrading + WhiteBalance)", TrainArenaDebugManager.DebugLogLevel.Important);
        
        volume.sharedProfile = profile;
    }
    
    /// <summary>
    /// Enhance camera with professional settings
    /// </summary>
    public static void EnhanceCamera(Camera camera)
    {
        if (camera == null) return;
        
        // Apply enhanced camera settings for better visuals
        camera.allowHDR = true;
        camera.allowMSAA = true;
        
        // Ensure URP camera data exists
        var urpCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (urpCameraData == null)
        {
            urpCameraData = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
        }
        
        // Configure URP settings for quality
        urpCameraData.renderPostProcessing = true;
        urpCameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        urpCameraData.antialiasingQuality = AntialiasingQuality.High;
        
        TrainArenaDebugManager.Log("📹 Enhanced camera settings applied", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    /// <summary>
    /// Apply enhanced skybox and environment settings
    /// </summary>
    public static void ApplyEnvironmentSettings()
    {
        ApplyEnvironmentSettings(CurrentSettings);
    }
    
    /// <summary>
    /// Apply enhanced skybox and environment settings with custom configuration
    /// </summary>
    public static void ApplyEnvironmentSettings(VisualSettings settings)
    {
        // Create configurable procedural skybox
        var skyboxMaterial = new Material(Shader.Find("Skybox/Procedural"));
        if (skyboxMaterial != null)
        {
            // Apply configurable sky colors
            skyboxMaterial.SetColor("_SkyTint", settings.skyTint);
            skyboxMaterial.SetColor("_GroundColor", settings.groundColor);
            skyboxMaterial.SetFloat("_AtmosphereThickness", settings.atmosphereThickness);
            skyboxMaterial.SetFloat("_Exposure", settings.exposure);
            skyboxMaterial.SetFloat("_SunSize", settings.sunSize);
            skyboxMaterial.SetFloat("_SunSizeConvergence", settings.sunSizeConvergence);
            
            RenderSettings.skybox = skyboxMaterial;
            
            // Configure directional light with tunable settings
            var sun = Object.FindFirstObjectByType<Light>();
            if (sun && sun.type == LightType.Directional)
            {
                RenderSettings.sun = sun;
                
                // Apply configurable sun light settings
                sun.color = settings.sunColor;
                sun.intensity = settings.sunIntensity;
                sun.shadows = LightShadows.Soft;                 // Always soft shadows for polish
                
                TrainArenaDebugManager.Log($"🌞 Enhanced directional light (intensity: {settings.sunIntensity})", TrainArenaDebugManager.DebugLogLevel.Important);
            }
            
            // Update environment lighting
            DynamicGI.UpdateEnvironment();
            
            TrainArenaDebugManager.Log("🌤️ Applied enhanced skybox and environment", TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }

    /// <summary>
    /// Configure professional lighting and fog settings
    /// </summary>
    public static void ConfigureLighting(bool isRagdollScene = false)
    {
        ConfigureLighting(isRagdollScene, CurrentSettings);
    }
    
    /// <summary>
    /// Configure professional lighting and fog settings with custom configuration
    /// </summary>
    public static void ConfigureLighting(bool isRagdollScene, VisualSettings settings)
    {
        // Apply configurable ambient lighting
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = settings.ambientSkyColor;
        RenderSettings.ambientEquatorColor = settings.ambientEquatorColor;
        RenderSettings.ambientGroundColor = settings.ambientGroundColor;
        
        // Apply configurable atmospheric fog
        RenderSettings.fog = true;
        RenderSettings.fogColor = settings.fogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        
        // Use appropriate fog density based on scene type
        RenderSettings.fogDensity = isRagdollScene ? settings.fogDensityRagdoll : settings.fogDensityCube;
        
        TrainArenaDebugManager.Log($"💡 Configured lighting and fog (ragdoll mode: {isRagdollScene}, fog density: {RenderSettings.fogDensity:F4})", TrainArenaDebugManager.DebugLogLevel.Important);
    }

    /// <summary>
    /// Apply optimal camera settings procedurally (no prefab dependencies)
    /// </summary>
    public static void ApplyCameraPrefabSettings(Camera targetCamera)
    {
        ApplyCameraPrefabSettings(targetCamera, CurrentSettings);
    }
    
    /// <summary>
    /// Apply camera settings with custom configuration
    /// </summary>
    public static void ApplyCameraPrefabSettings(Camera targetCamera, VisualSettings settings)
    {
        if (targetCamera != null)
        {
            // Apply configurable camera settings
            targetCamera.fieldOfView = settings.fieldOfView;
            targetCamera.nearClipPlane = settings.nearClipPlane;
            targetCamera.farClipPlane = settings.farClipPlane;
            targetCamera.allowHDR = true;          // Always enabled for quality
            targetCamera.allowMSAA = true;         // Always enabled for quality
            
            TrainArenaDebugManager.Log($"📹 Applied configurable camera settings (FOV: {settings.fieldOfView}°, Near: {settings.nearClipPlane}, Far: {settings.farClipPlane})", TrainArenaDebugManager.DebugLogLevel.Important);
            
            // Add EditorCameraController with configurable settings if not present
            var controller = targetCamera.GetComponent<EditorCameraController>();
            if (controller == null)
            {
                controller = targetCamera.gameObject.AddComponent<EditorCameraController>();
            }
            
            // Configure camera controller with configurable settings
            controller.moveSpeed = settings.moveSpeed;
            controller.fastMoveSpeed = settings.fastMoveSpeed;
            controller.mouseSensitivity = settings.mouseSensitivity;
            
            TrainArenaDebugManager.Log($"🎮 Configured EditorCameraController (Speed: {settings.moveSpeed}, Fast: {settings.fastMoveSpeed}, Sensitivity: {settings.mouseSensitivity})", TrainArenaDebugManager.DebugLogLevel.Important);
        }
    }

    /// <summary>
    /// Quick preset switching methods
    /// </summary>
    public static void SetPolishedCoolPreset()
    {
        CurrentSettings = VisualSettings.PolishedCool();
        TrainArenaDebugManager.Log("🎨 Switched to Polished Cool preset", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    public static void SetWarmCinematicPreset()
    {
        CurrentSettings = VisualSettings.WarmCinematic();
        TrainArenaDebugManager.Log("🎨 Switched to Warm Cinematic preset", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    public static void SetMinimalCleanPreset()
    {
        CurrentSettings = VisualSettings.MinimalClean();
        TrainArenaDebugManager.Log("🎨 Switched to Minimal Clean preset", TrainArenaDebugManager.DebugLogLevel.Important);
    }
    
    /// <summary>
    /// Menu items for visual preset switching
    /// </summary>
    [MenuItem("Tools/ML Hack/Visual Presets/Polished Cool (Default)")]
    public static void MenuSetPolishedCool() => SetPolishedCoolPreset();
    
    [MenuItem("Tools/ML Hack/Visual Presets/Warm Cinematic")]
    public static void MenuSetWarmCinematic() => SetWarmCinematicPreset();
    
    [MenuItem("Tools/ML Hack/Visual Presets/Minimal Clean")]  
    public static void MenuSetMinimalClean() => SetMinimalCleanPreset();
    
    [MenuItem("Tools/ML Hack/Visual Presets/Re-apply Current Scene Enhancement")]
    public static void MenuReapplyEnhancement()
    {
        var camera = Object.FindFirstObjectByType<Camera>();
        if (camera != null)
        {
            // Detect if ragdoll scene by looking for ragdoll agents
            bool isRagdollScene = Object.FindFirstObjectByType<RagdollAgent>() != null;
            EnhanceScene(camera, isRagdollScene);
        }
        else
        {
            TrainArenaDebugManager.LogWarning("No camera found in scene to enhance");
        }
    }
}