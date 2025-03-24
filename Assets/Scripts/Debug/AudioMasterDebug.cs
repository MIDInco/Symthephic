using UnityEngine;
using UnityEngine.Audio;

public class MixerDebugLogger : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    private const string VolumeKey = "MasterVolume";
    private const string VolumeParameter = "MasterVolume";

    void Start()
    {
        // 保存値を読み込み
        float savedVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
        float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(savedVolume, 0.3f));

        // Mixer に反映
        bool setResult = audioMixer.SetFloat(VolumeParameter, db);
        Debug.Log($"🎛 [MixerDebug] SetFloat({VolumeParameter}, {db}) 成功: {setResult}");

        // Mixer に反映された値を取得して確認
        if (audioMixer.GetFloat(VolumeParameter, out float resultDb))
        {
            Debug.Log($"🔍 [MixerDebug] 実際のMixer設定値: {VolumeParameter} = {resultDb} dB");
        }
        else
        {
            Debug.LogWarning($"⚠️ [MixerDebug] Mixerから{VolumeParameter}の値を取得できませんでした！");
        }
    }
}
