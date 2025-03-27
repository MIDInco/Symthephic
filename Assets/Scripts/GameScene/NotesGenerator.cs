using System;
using MidiPlayerTK;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using MPTKDemoCatchMusic;

//ロングノーツを実装予定。

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
    public bool isReady { get; private set; } = false;
    public int chartDelayInTicks = 0;

    public Transform judgmentLine;

    public event Action OnChartPlaybackStart;
    public event Action<NoteController> OnNoteGenerated;

    private List<(long tick, double tempo)> cachedTempoEvents = new List<(long, double)>();

    public JudgmentManager judgmentManager;

    private bool isPaused = false;

    void Awake()
    {
        Debug.Log("✅ NotesGenerator の Awake が実行されました");
    }

    void Start()
    {
        if (MusicManager.SelectedMusic != null)
        {
            Debug.Log($"🎯 NotesGenerator: 選択されたMIDIを受け取りました → {MusicManager.SelectedMusic.MidiFileName}");
            Debug.Log("⏳ ただし、譜面の生成は ChartPlaybackManager に任せるため、ここでは実行しません。");
        }
        else
        {
            Debug.LogError("❌ GameScene に MIDI データが渡っていません！");
        }
    }

    void Update()
    {

    double currentTime = GameSceneManager.GetGameDspTime() - startTime;

    if (GameSceneManager.IsPaused || GameSceneManager.IsResuming)
    {
        //Debug.Log("⏸ Update停止中：ポーズ中");
        return;
    }

    if (!isReady)
    {
        //Debug.Log("⏸ Update停止中：isReady が false");
        return;
    }

    //ebug.Log("▶ Update実行中：ノートを動かします");
    noteControllers.RemoveAll(note => note == null);

    foreach (var note in noteControllers)
    {
        note?.UpdatePosition((float)currentTime);
    }

    if (isPaused)
    {
        //Debug.Log("⏸ NotesGenerator: isPaused により停止中");
        return;
    }
}

    public void RemoveNote(NoteController note)
    {
        noteControllers.Remove(note);
    }

    void CacheTempoEvents(MidiLoad midiLoad)
    {
        cachedTempoEvents.Clear();

        foreach (var ev in midiLoad.MPTK_MidiEvents)
        {
            if (ev.Meta == MPTKMeta.SetTempo)
            {
                cachedTempoEvents.Add((ev.Tick, ev.Value));
            }
        }

        cachedTempoEvents.Sort((a, b) => a.tick.CompareTo(b.tick));
        Debug.Log($"📊 テンポイベント数: {cachedTempoEvents.Count}");
    }

    public long GetCurrentTickWithTempo(double dspTime)
    {
        double timeSinceStart = dspTime - startTime;
        double currentTempo = 500000.0;
        long lastTick = 0;
        double currentTickTime = 0.0;
        int tempoIndex = 0;

        foreach (var tempo in cachedTempoEvents)
        {
            double secondsPerTick = currentTempo / 1000000.0 / TPQN;
            double nextTickTime = currentTickTime + (tempo.tick - lastTick) * secondsPerTick;
            if (nextTickTime > timeSinceStart) break;

            currentTickTime = nextTickTime;
            lastTick = tempo.tick;
            currentTempo = tempo.tempo;
            tempoIndex++;
        }

        double remainingTime = timeSinceStart - currentTickTime;
        double secondsPerTickNow = currentTempo / 1000000.0 / TPQN;
        return lastTick + (long)(remainingTime / secondsPerTickNow);
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

        CacheTempoEvents(midiLoad);
        GenerateNotes(midiLoad);
        Debug.Log($"✅ ノート生成完了！");
        
        // 🟡 スコア用：ノーツ数をスコアマネージャーに通知
        ScoreManager.Instance?.SetTotalNotes(noteControllers.Count);

        double audioStartTime = AudioSettings.dspTime;
        float chartDelayOffset = Noteoffset.Instance != null ? Noteoffset.Instance.GetChartDelay() : 0f;
        startTime = audioStartTime + chartDelayOffset;

        Debug.Log($"⏳ 譜面の開始時間を {chartDelayOffset} 秒遅らせる (startTime = {startTime:F3})");

        //isReady = true;
Debug.Log("✅ NotesGenerator: isReady を true に設定しました（譜面再生準備完了）");
    }

    void GenerateNotes(MidiLoad midiLoad)
    {
        Debug.Log("📜 ノート生成開始...");

        double TPQN = midiLoad.MPTK_DeltaTicksPerQuarterNote;
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

        double currentTempo = 500000;
        long lastTick = 0;
        double currentTime = 0.0;
        int tempoIndex = 0;

        foreach (var kvp in tickNotesMap)
        {
            long tick = kvp.Key;
            List<MPTKEvent> notesAtTick = kvp.Value;

            while (tempoIndex < cachedTempoEvents.Count && cachedTempoEvents[tempoIndex].tick <= tick)
            {
                long deltaTicks = cachedTempoEvents[tempoIndex].tick - lastTick;
                double secondsPerTick = currentTempo / 1000000.0 / TPQN;
                currentTime += deltaTicks * secondsPerTick;

                lastTick = cachedTempoEvents[tempoIndex].tick;
                currentTempo = cachedTempoEvents[tempoIndex].tempo;
                tempoIndex++;
            }

            double secondsPerTickNow = currentTempo / 1000000.0 / TPQN;
            double noteTime = currentTime + (tick - lastTick) * secondsPerTickNow;
            Debug.Log($"🧪 ノート生成: tick={tick}, tempo={currentTempo}, TPQN={TPQN}, secondsPerTickNow={secondsPerTickNow:F6}");
Debug.Log($"🕒 ノートタイミング: noteTime={noteTime:F3}, spawnTime={(noteTime - 2.0):F3}, startZ={(-noteSpeed * 2.0f):F2}");


            notesAtTick.Sort((a, b) => b.Value.CompareTo(a.Value));

    foreach (var ev in notesAtTick)
    {
double travelTime = 5.0;
double spawnTime = noteTime - travelTime;
double timeUntilJudgment = noteTime - startTime; // 判定まであと何秒？
double startZ = (timeUntilJudgment + travelTime) * noteSpeed;   // Z+方向に配置
        float startX = GetFixedXPosition(ev.Value);

        GameObject note = Instantiate(Notes, new Vector3(startX, spawnPoint.position.y, (float)startZ), Quaternion.identity);
        note.SetActive(true);

        NoteController noteController = note.GetComponent<NoteController>();
        if (noteController != null)
        {
            string uniqueID = globalIndex.ToString();
            globalIndex++;

            noteController.Initialize(noteTime, this, uniqueID); // noteTimeは判定タイミング
            noteController.noteValue = ev.Value;
            noteController.tick = tick;
            noteControllers.Add(noteController);

            Debug.Log($"🎵 [ノート生成] ID={uniqueID}, Note={noteController.noteValue}, Tick={noteController.tick}, 発音時間={noteTime:F3} sec");

            OnNoteGenerated?.Invoke(noteController);
        }
    }
        }

        Debug.Log("✅ ノート生成完了（テンポ対応）！");
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

    public List<NoteController> GetNoteControllers() => noteControllers;

public void StartPlayback()
{
    // isReady チェックを削除 or 再設定
    isReady = true;
    Debug.Log($"🎵 譜面の再生を開始！ (startTime={startTime:F3})");

    OnChartPlaybackStart?.Invoke();
}

    public void SetStartTime(double time)
    {
        startTime = time;
        Debug.Log($"🎵 NotesGenerator: startTime を {startTime:F3} に設定");
    }

    public void LoadSelectedMidiAndGenerateNotes()
    {   //noteSpeed = 5.0f; // テスト用に固定
        noteSpeed = GameSettings.NoteSpeed; // UIスライダー値（0.5〜10.0）
        Debug.Log($"🎯 NoteSpeed が設定されました: {noteSpeed}");

        if (MusicManager.SelectedMusic != null)
        {
            Debug.Log($"🎯 NotesGenerator: 選択されたMIDIを読み込みます → {MusicManager.SelectedMusic.MidiFileName}");
            midiFilePlayer.MPTK_MidiName = MusicManager.SelectedMusic.MidiFileName;
            StartCoroutine(LoadMidiFileAsync());
        }
        else
        {
            Debug.LogError("❌ NotesGenerator: SongManager.SelectedSong が null です！");
        }
    }

public void PausePlayback()
{
    isPaused = true;
}

public void ResumePlayback()
{
    isPaused = false;
}


    public void ResetState()
{
    isReady = false;

    foreach (var note in noteControllers)
    {
        if (note != null)
            Destroy(note.gameObject);
    }
    noteControllers.Clear();

    Debug.Log("🔁 NotesGenerator: 状態を初期化しました（ノート削除 + isReady false）");
}
}