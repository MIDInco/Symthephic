using UnityEngine;
using UnityEngine.Audio;

public static class GameSettingsLoader
{
    private const string SpeedKey = "NoteSpeed";
    private const string VolumeKey = "MasterVolume";
    private const string VolumeParameter = "MasterVolume";

    public static void Load(AudioMixer audioMixer = null)
{
    // 🔍 ここで PlayerPrefs に保存されている「生の値」を取得
    float rawSpeed = PlayerPrefs.GetFloat(SpeedKey, 5.0f);
    float rawVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
    float offset = PlayerPrefs.GetFloat("NoteOffsetValue", 0.0f);
    float delay = PlayerPrefs.GetFloat("ChartDelay", 0.1f);
    GameSettings.NoteOffsetValue = offset;
    GameSettings.ChartDelay = delay;
    
    // 🟡 追加：生の値をログ出力（デフォルトで読み込まれたかも確認）
    Debug.Log($"🟡 [PrefsRaw] Volume (saved) = {rawVolume}, Speed (saved) = {rawSpeed}");

    GameSettings.NoteSpeed = rawSpeed;
    GameSettings.MasterVolume = rawVolume;

    Debug.Log($"🔄 GameSettingsLoader: NoteSpeed={GameSettings.NoteSpeed}, Volume={GameSettings.MasterVolume}");

    if (audioMixer != null)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(GameSettings.MasterVolume, 0.3f));
        audioMixer.SetFloat(VolumeParameter, db);
        Debug.Log($"🔊 GameSettingsLoader: マスターボリュームを即時反映 → dB={db}");
    }
}

}
