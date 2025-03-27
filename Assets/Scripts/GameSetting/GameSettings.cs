[System.Serializable]
public class GameSettingsData
{
    public float NoteSpeed = 10.0f;
    public float MasterVolume = 1f;
    public float NoteOffsetValue = 0.0f;
    public float ChartDelay = 0.05f;
}

public static class GameSettings
{
    public static float NoteSpeed;
    public static float MasterVolume;
    public static float NoteOffsetValue;
    public static float ChartDelay;

    public static void ApplyFromData(GameSettingsData data)
    {
        NoteSpeed = data.NoteSpeed;
        MasterVolume = data.MasterVolume;
        NoteOffsetValue = data.NoteOffsetValue;
        ChartDelay = data.ChartDelay;
    }

    public static GameSettingsData ToData()
    {
        return new GameSettingsData()
        {
            NoteSpeed = NoteSpeed,
            MasterVolume = MasterVolume,
            NoteOffsetValue = NoteOffsetValue,
            ChartDelay = ChartDelay
        };
    }
}
