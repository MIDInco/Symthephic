using UnityEngine;

public static class GameSettingsLoader
{
    private const string SpeedKey = "NoteSpeed";
    private const string VolumeKey = "MasterVolume";

    public static void Load()
    {
        GameSettings.NoteSpeed = PlayerPrefs.GetFloat(SpeedKey, 5.0f);
        GameSettings.MasterVolume = PlayerPrefs.GetFloat(VolumeKey, 0.8f);

        Debug.Log($"🔄 GameSettingsLoader: NoteSpeed={GameSettings.NoteSpeed}, Volume={GameSettings.MasterVolume}");
    }
}
