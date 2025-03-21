using System; // 🎯 Guid を使用するために追加
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class NotesGenerator : MonoBehaviour
{
    public GameObject Notes;
    public Transform spawnPoint;
    public float noteSpeed = 1f;

    // noteControllers を private のままにする
    public List<NoteController> noteControllers = new List<NoteController>();
    public double startTime { get; private set; }
    public MidiFilePlayer midiFilePlayer;

    public int TPQN { get; private set; }
    public float BPM { get; private set; }

    public float chartDelay = 0.0f; // 🎯 譜面の再生遅延時間をInspectorから設定可能に

    private Dictionary<string, double> noteReachTimes = new Dictionary<string, double>();
    private Dictionary<string, float> noteOnTimes = new Dictionary<string, float>();

    public bool isReady { get; private set; } = false; // 🎯 ロード完了フラグ
    public int chartDelayInTicks = 0; // 🎯 Tick 単位で譜面の遅延を指定

    public Transform judgmentLine; // 判定ラインのTransformを設定

    public event Action OnChartPlaybackStart; // 🎯 イベントを追加

    public event System.Action<NoteController> OnNoteGenerated; // 🎯 ノート生成イベント追加



    void Awake()
    {
        Debug.Log("✅ NotesGenerator の Awake が実行されました");
    }

void Start()
{
    // ✅ 受け取ったデータをログに出力
    if (SongManager.SelectedSong != null)
    {
        Debug.Log($"🎯 GameScene で受け取ったMIDI: {SongManager.SelectedSong.MidiFileName} / {SongManager.SelectedSong.DisplayName}");
    }
    else
    {
        Debug.LogError("❌ GameScene に MIDI データが渡っていません！");
    }
}



private void OnAudioStarted()
{
    Debug.Log("🎯 NotesGenerator: OnAudioStarted() 実行！");

    // 🎯 カウントダウン終了時の正確な時間を取得
    double playbackStartTime = AudioSettings.dspTime;

    // 🎯 オーディオを再生
    AudioManager.Instance.PlayAudioNow();

    // 🎯 譜面の開始時間をセット
    this.StartPlayback(); // ✅ `StartPlayback()` に変更

    Debug.Log("✅ オーディオの再生を検知し、譜面を開始！");
}


  void Update()
{
    // 🎯 isReady の状態と現在の時間をデバッグログに出力
    //Debug.Log($"🔍 Update() 実行 - isReady={isReady}, CurrentTime={AudioSettings.dspTime:F3}");

    if (!isReady) 
    {
        Debug.Log("⏳ NotesGenerator の Update() は isReady=false のため動作しません");
        return;
    }

    double currentTime = AudioSettings.dspTime - startTime;
    noteControllers.RemoveAll(note => note == null);

    for (int i = noteControllers.Count - 1; i >= 0; i--)
    {
        if (noteControllers[i] != null)
        {
            noteControllers[i].UpdatePosition((float)currentTime);
           // Debug.Log($"🎵 ノーツ更新 - NoteID: {noteControllers[i].uniqueID}, Z位置: {noteControllers[i].transform.position.z}");
        }
    }
}


    public void RemoveNote(NoteController note)
    {
        noteControllers.Remove(note);
    }

IEnumerator LoadMidiFileAsync()
{
    isReady = false; // 🎯 再生開始フラグをオフ

    if (midiFilePlayer == null) yield break;

    // 🎯 ① MIDIデータのロード（即時処理）
    MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
    if (midiLoad == null) yield break;
    Debug.Log("🎵 MIDIデータのロード完了 → 譜面を即時生成");

    // 🎯 ② TPQN & BPM の取得
    TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
    BPM = (float)midiFilePlayer.MPTK_Tempo;

    Debug.Log($"🎼 BPM={BPM}, TPQN={TPQN}");

    // 🎯 ③ Tick を 秒数 に変換
    float quarterNoteDuration = 60f / BPM;
    float tickDuration = quarterNoteDuration / TPQN;
    float chartDelayFromTicks = chartDelayInTicks * tickDuration;
    float totalChartDelay = chartDelay + chartDelayFromTicks;

    // 🎯 ④ **譜面の生成を完全に終えてから、再生を開始**
    GenerateNotes(midiLoad);
    Debug.Log($"✅ ノート生成完了！");

    // 🎯 **オーディオの開始時間は変更しない**
    double audioStartTime = AudioSettings.dspTime;

    // 🎯 **譜面の開始時間を Noteoffset の Chart Delay で遅らせる**
    float chartDelayOffset = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
    startTime = audioStartTime + chartDelayOffset;

    Debug.Log($"⏳ 譜面の開始時間を {chartDelayOffset} 秒遅らせる (startTime = {startTime:F3})");

    isReady = false; // 🎯 譜面再生フラグをON
}
//メモメモメモ　Trueかもしれない。




void GenerateNotes(MidiLoad midiLoad)
{
    Debug.Log("📜 ノート生成開始...");

    double TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
    double BPM = midiFilePlayer.MPTK_Tempo;

    double quarterNoteDuration = 60.0 / BPM;
    double tickDuration = quarterNoteDuration / TPQN;

    Dictionary<long, List<MPTKEvent>> tickNotesMap = new Dictionary<long, List<MPTKEvent>>();
    int globalIndex = 0; // シンプルな連番管理

    // ① ノートを Tick ごとにグループ化
    foreach (MPTKEvent ev in midiLoad.MPTK_MidiEvents)
    {
        if (ev.Command == MPTKCommand.NoteOn)
        {
            if (!tickNotesMap.ContainsKey(ev.Tick))
                tickNotesMap[ev.Tick] = new List<MPTKEvent>();

            tickNotesMap[ev.Tick].Add(ev);
        }
    }

    // ② Tick ごとにノートを処理（高い音から順にソート）
    foreach (var kvp in tickNotesMap)
    {
        long tick = kvp.Key;
        List<MPTKEvent> notesAtTick = kvp.Value;

        // 高いノート順にソート
        notesAtTick.Sort((a, b) => b.Value.CompareTo(a.Value));

        foreach (var ev in notesAtTick)
        {
            double noteTime = tick * tickDuration; // 🎯 発音時間（秒）を計算
            double startZ = -noteSpeed * noteTime;
            float startX = GetFixedXPosition(ev.Value);

            GameObject note = Instantiate(Notes, new Vector3(startX, spawnPoint.position.y, (float)startZ), Quaternion.identity);
            note.SetActive(true);

            NoteController noteController = note.GetComponent<NoteController>();
            if (noteController != null)
            {
                // シンプルな連番をノートIDとして使用
                string uniqueID = globalIndex.ToString();
                globalIndex++;

                noteController.Initialize(noteTime, this, uniqueID);
                noteController.noteValue = ev.Value;
                noteController.tick = tick;
                noteControllers.Add(noteController);

                // 🎯 デバッグログを改良（ノートID, Tick, 発音時間を出力）
                Debug.Log($"🎵 [ノート生成] ID={uniqueID}, Note={noteController.noteValue}, Tick={noteController.tick}, 発音時間={noteTime:F3} sec");
            }
        }
    }

    Debug.Log("✅ ノート生成完了！");
}


    public float GetFixedXPosition(int noteValue)
    {
        switch (noteValue)
        {
            case 60: return -2.5f; // A キー (C4)
            case 61: return -1.5f; // S キー (C#4)
            case 62: return -0.5f; // D キー (D4)
            case 63: return  0.5f; // K キー (D#4)
            case 64: return  1.5f; // L キー (E4)
            case 65: return  2.5f; // : キー (F4)
            default: return 0f; // その他のノートは中央に配置
        }
    }


    public JudgmentManager judgmentManager; // 判定処理を委譲

// 外部から安全に取得するための Getter メソッドを追加
    public List<NoteController> GetNoteControllers()
    {
        return noteControllers;
    }


    public void StartPlayback()
    {
        if (isReady) return; // 🎯 すでに再生中なら無視

        startTime = AudioSettings.dspTime; // 🎯 再生開始時間をセット
        isReady = true; // 🎯 ここで譜面の再生を開始
        Debug.Log($"🎵 譜面の再生を開始！ (startTime={startTime:F3})");
    }

    public void SetStartTime(double time)
{
    startTime = time;
    Debug.Log($"🎵 NotesGenerator: startTime を {startTime:F3} に設定");
}

    }
