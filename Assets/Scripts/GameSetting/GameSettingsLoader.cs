using UnityEngine;
using UnityEngine.Audio;

public static class GameSettingsLoader
{
    private const string SpeedKey = "NoteSpeed";
    private const string VolumeKey = "MasterVolume";
    private const string VolumeParameter = "MasterVolume";

public static void Load(AudioMixer audioMixer = null)
{
    float rawSpeed = PlayerPrefs.GetFloat(SpeedKey, 10.0f);
    float rawVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);
    float offset = PlayerPrefs.GetFloat("NoteOffsetValue", 0.0f);
    float delay = PlayerPrefs.GetFloat("ChartDelay", 0.2f);

    Debug.Log("🟡 [GameSettingsLoader] PlayerPrefs からの読み込み値:");
    Debug.Log($"    NoteSpeed = {rawSpeed}");
    Debug.Log($"    MasterVolume = {rawVolume}");
    Debug.Log($"    NoteOffsetValue = {offset}");
    Debug.Log($"    ChartDelay = {delay}");

    GameSettings.NoteSpeed = rawSpeed;
    GameSettings.MasterVolume = rawVolume;
    GameSettings.NoteOffsetValue = offset;
    GameSettings.ChartDelay = delay;

    if (audioMixer != null)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Pow(GameSettings.MasterVolume, 0.3f));
        audioMixer.SetFloat(VolumeParameter, db);
        Debug.Log($"🔊 GameSettingsLoader: マスターボリューム dB = {db}");
    }
}

}
