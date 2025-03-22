using System;

[Serializable]
public class SongData
{
    public string MidiFileName;
    public string DisplayName;
    public string AudioFileName;
    public bool IsDLC;
    public string DLCId;

    public SongData(string midiFileName, string displayName, string audioFileName, bool isDLC = false, string dlcId = "")
    {
        MidiFileName = midiFileName;
        DisplayName = displayName;
        AudioFileName = audioFileName;
        IsDLC = isDLC;
        DLCId = dlcId;
    }
}

