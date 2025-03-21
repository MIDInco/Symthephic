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

    public List<NoteController> noteControllers = new List<NoteController>();
    public double startTime { get; private set; }
    public MidiFilePlayer midiFilePlayer;

    public int TPQN { get; private set; }
    public float BPM { get; private set; }

    public float chartDelay = 0.0f;

    private Dictionary<string, double> noteReachTimes = new Dictionary<string, double>();
    private Dictionary<string, float> noteOnTimes = new Dictionary<string, float>();

    public bool isReady { get; private set; } = false;
    public int chartDelayInTicks = 0;

    public Transform judgmentLine;

    public event Action OnChartPlaybackStart;
    public event System.Action<NoteController> OnNoteGenerated;

    void Awake()
    {
        Debug.Log("✅ NotesGenerator の Awake が実行されました");
    }

    void Start()
    {
        if (SongManager.SelectedSong != null)
        {
            Debug.Log($"🎯 GameScene で受け取ったMIDI: {SongManager.SelectedSong.MidiFileName} / {SongManager.SelectedSong.DisplayName}");
            LoadSelectedMidiAndGenerateNotes(); // 🎯 追加したメソッドを呼び出す
        }
        else
        {
            Debug.LogError("❌ GameScene に MIDI データが渡っていません！");
        }
    }

    private void OnAudioStarted()
    {
        Debug.LogWarning("⚠ OnAudioStarted() は現在使用されていません。ChartPlaybackManager 経由で譜面を再生してください。");
    }

    void Update()
    {
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
            }
        }
    }

    public void RemoveNote(NoteController note)
    {
        noteControllers.Remove(note);
    }

 IEnumerator LoadMidiFileAsync()
{
    isReady = false;

    if (midiFilePlayer == null) yield break;

    MidiLoad midiLoad = midiFilePlayer.MPTK_Load();
    if (midiLoad == null) yield break;
    Debug.Log("🎵 MIDIデータのロード完了 → 譜面を即時生成");

    TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
    BPM = (float)midiFilePlayer.MPTK_Tempo;

    Debug.Log($"🎼 BPM={BPM}, TPQN={TPQN}");

    float quarterNoteDuration = 60f / BPM;
    float tickDuration = quarterNoteDuration / TPQN;
    float chartDelayFromTicks = chartDelayInTicks * tickDuration;
    float totalChartDelay = chartDelay + chartDelayFromTicks;

    GenerateNotes(midiLoad);
    Debug.Log($"✅ ノート生成完了！");

    // 🎯 仮の再生時刻を設定（実際には ChartPlaybackManager が SetStartTime() で上書きする）
    double audioStartTime = AudioSettings.dspTime;
    float chartDelayOffset = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
    startTime = audioStartTime + chartDelayOffset;

    Debug.Log($"⏳ 譜面の開始時間を {chartDelayOffset} 秒遅らせる (startTime = {startTime:F3})");

    // 🎯 この段階ではまだ譜面は再生しないが、Updateが動くように isReady を true にする
    isReady = false; // ✅ ← ここは true にしない（勝手に動き出すのを防ぐ）
}


    void GenerateNotes(MidiLoad midiLoad)
    {
        Debug.Log("📜 ノート生成開始...");

        double TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
        double BPM = midiFilePlayer.MPTK_Tempo;

        double quarterNoteDuration = 60.0 / BPM;
        double tickDuration = quarterNoteDuration / TPQN;

        Dictionary<long, List<MPTKEvent>> tickNotesMap = new Dictionary<long, List<MPTKEvent>>();
        int globalIndex = 0;

        foreach (MPTKEvent ev in midiLoad.MPTK_MidiEvents)
        {
            if (ev.Command == MPTKCommand.NoteOn)
            {
                if (!tickNotesMap.ContainsKey(ev.Tick))
                    tickNotesMap[ev.Tick] = new List<MPTKEvent>();

                tickNotesMap[ev.Tick].Add(ev);
            }
        }

        foreach (var kvp in tickNotesMap)
        {
            long tick = kvp.Key;
            List<MPTKEvent> notesAtTick = kvp.Value;

            notesAtTick.Sort((a, b) => b.Value.CompareTo(a.Value));

            foreach (var ev in notesAtTick)
            {
                double noteTime = tick * tickDuration;
                double startZ = -noteSpeed * noteTime;
                float startX = GetFixedXPosition(ev.Value);

                GameObject note = Instantiate(Notes, new Vector3(startX, spawnPoint.position.y, (float)startZ), Quaternion.identity);
                note.SetActive(true);

                NoteController noteController = note.GetComponent<NoteController>();
                if (noteController != null)
                {
                    string uniqueID = globalIndex.ToString();
                    globalIndex++;

                    noteController.Initialize(noteTime, this, uniqueID);
                    noteController.noteValue = ev.Value;
                    noteController.tick = tick;
                    noteControllers.Add(noteController);

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
            case 60: return -2.5f;
            case 61: return -1.5f;
            case 62: return -0.5f;
            case 63: return  0.5f;
            case 64: return  1.5f;
            case 65: return  2.5f;
            default: return 0f;
        }
    }

    public JudgmentManager judgmentManager;

    public List<NoteController> GetNoteControllers()
    {
        return noteControllers;
    }

    public void StartPlayback()
    {
        if (isReady) return;

        startTime = AudioSettings.dspTime;
        isReady = true;
        Debug.Log($"🎵 譜面の再生を開始！ (startTime={startTime:F3})");
    }

    public void SetStartTime(double time)
    {
        startTime = time;
        Debug.Log($"🎵 NotesGenerator: startTime を {startTime:F3} に設定");
    }

    // 🎯 新規追加：選択された曲のMIDIを読み込み → ノーツ生成
    public void LoadSelectedMidiAndGenerateNotes()
    {
        if (SongManager.SelectedSong != null)
        {
            Debug.Log($"🎯 NotesGenerator: 選択されたMIDIを読み込みます → {SongManager.SelectedSong.MidiFileName}");
            midiFilePlayer.MPTK_MidiName = SongManager.SelectedSong.MidiFileName;
            StartCoroutine(LoadMidiFileAsync());
        }
        else
        {
            Debug.LogError("❌ NotesGenerator: SongManager.SelectedSong が null です！");
        }
    }
}
