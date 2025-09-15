#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder.Encoder;

public class RecorderUtility : MonoBehaviour
{
    [MenuItem("Tools/ML Hack/Start Recording")]
    static void StartRecording()
    {
        var recorderController = new RecorderController(new RecorderControllerSettings());
        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "HackVideo";
        videoRecorder.Enabled = true;
        
        // Use the new EncoderSettings API instead of deprecated properties
        var coreEncoderSettings = new CoreEncoderSettings
        {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
        };
        videoRecorder.EncoderSettings = coreEncoderSettings;
        
        videoRecorder.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = 1920,
            OutputHeight = 1080
        };
        videoRecorder.AudioInputSettings.PreserveAudio = true;
        videoRecorder.OutputFile = "Recordings/Hackathon_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        recorderController.Settings.AddRecorderSettings(videoRecorder);
        recorderController.Settings.SetRecordModeToManual();
        recorderController.PrepareRecording();
        recorderController.StartRecording();
        Debug.Log("Recording started. Stop via Tools/ML Hack/Stop Recording.");
    }

    [MenuItem("Tools/ML Hack/Stop Recording")]
    static void StopRecording()
    {
        RecorderWindow recorderWindow = EditorWindow.GetWindow<RecorderWindow>();
        if (recorderWindow != null)
        {
            recorderWindow.StopRecording();
            Debug.Log("Recording stopped.");
        }
    }
}
#endif