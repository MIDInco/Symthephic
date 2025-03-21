using System;

[Serializable]
public class SongData
{
    public string MidiFileName;   // MIDIファイル名（MPTKで使用されるLabel）
    public string DisplayName;    // 本当の曲名
    public string AudioFileName;  // 対応するオーディオファイル名

    // コンストラクタ
    public SongData(string midiFileName, string displayName, string audioFileName)
    {
        MidiFileName = midiFileName;
        DisplayName = displayName;
        AudioFileName = audioFileName;
    }
}
