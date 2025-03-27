using UnityEngine;
using UnityEngine.Audio;

public class AudioMasterDebug : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    private const string VolumeParameter = "MasterVolume";

    void Start()
    {
        ApplyCurrentVolume();
        LogCurrentMixerVolume();
    }

    public void ApplyCurrentVolume()
    {
        float value = Mathf.Clamp(GameSettings.MasterVolume, 0.0001f, 1f);
        float db = Mathf.Lerp(-40f, 0f, Mathf.Pow(value, 0.4f));
        audioMixer.SetFloat(VolumeParameter, db);

        Debug.Log($"🎛 [AudioMasterDebug] SetFloat({VolumeParameter}, {db}dB) from MasterVolume={value}");
    }

    public void LogCurrentMixerVolume()
    {
        if (audioMixer.GetFloat(VolumeParameter, out float resultDb))
        {
            Debug.Log($"🔍 [AudioMasterDebug] 実際のMixer設定値: {VolumeParameter} = {resultDb} dB");
        }
        else
        {
            Debug.LogWarning($"⚠️ [AudioMasterDebug] Mixerから{VolumeParameter}の値を取得できませんでした！");
        }
    }
}
